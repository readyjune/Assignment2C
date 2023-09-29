using ClientServer.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IronPython.Hosting;
using IronPython.Runtime;
using System.Net.Http;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string uploadedPythonFilePath;
        private string uploadedPythonFileName;
        private string providedIPAddress;
        private int providedPort;
        private readonly ClientApiService _clientApiService;
        private DispatcherTimer _monitoringTimer;
        public MainWindow()
        {
            InitializeComponent();
            _clientApiService = new ClientApiService("http://localhost:5074/api");

            // Attach the Loaded event to call InitializeAsync method
            this.Loaded += async (sender, e) => await InitializeAsync();

            StartMonitoring();
        }
        private void StartMonitoring()
        {
            _monitoringTimer = new DispatcherTimer();
            _monitoringTimer.Interval = TimeSpan.FromSeconds(15);
            _monitoringTimer.Tick += CheckClientsStatus;
            _monitoringTimer.Start();
        }

        private async void CheckClientsStatus(object sender, EventArgs e)
        {
            try
            {
                var allClients = await _clientApiService.GetAllClientsAsync();

                foreach (var client in allClients)
                {
                    if (client.IPAddress == providedIPAddress && client.Port == providedPort)
                    {
                        // Skip this iteration if the client is the current client (i.e., has the same IP and port)
                        continue;
                    }
                    Console.WriteLine("jinwoo");
                    if (!string.IsNullOrEmpty(client.NeedHelp) && client.NeedHelp.Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase))
                    {
                        // Retrieve Python code from the database for the client (A)
                        string pythonCode = await RetrievePythonCodeFromApiAsync(client.IPAddress, client.Port); // Await here

                        // Execute the Python code and capture the output
                        string output = ExecutePythonCode(pythonCode);

                        // Send the output back to the original client (A)
                        SendOutputToClient(client.IPAddress, client.Port, output);

                        // Update the 'NeedHelp' status of the client to 'No' after sending the help message
                        await _clientApiService.UpdateClientNoHelpAsync(client.IPAddress, client.Port);
                        Console.WriteLine($"Updated NeedHelp status to 'No' for {client.IPAddress}:{client.Port}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during monitoring: {ex.Message}");
            }
        }
        // Function to execute Python code and capture the output
        private string ExecutePythonCode(string pythonCode)
        {
            try
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var output = new MemoryStream();
                engine.Runtime.IO.SetOutput(output, Encoding.UTF8);

                // Execute the Python code
                engine.Execute(pythonCode, scope);

                // Capture the output
                output.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(output);
                var capturedOutput = reader.ReadToEnd();
                return capturedOutput;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing Python code: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
        // Function to retrieve Python code from the database for a specific client using an API
        private async Task<string> RetrievePythonCodeFromApiAsync(string ipAddress, int port)
        {
            try
            {
                // Replace 'your-api-url-here' with the actual URL of your API endpoint
                string apiUrl = $"http://localhost:5074/api/Clients/get-python-code?ipAddress={ipAddress}&port={port}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content (Python code) from the API
                        string pythonCode = await response.Content.ReadAsStringAsync();
                        return pythonCode;
                    }
                    else
                    {
                        // Handle the case where the API request fails (e.g., client not found)
                        Console.WriteLine($"API request failed with status code: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving Python code from API: {ex.Message}");
                return null;
            }
        }


        // Function to send the output back to the original client (A)
        private void SendOutputToClient(string targetIPAddress, int targetPort, string output)
        {
            try
            {
                using (TcpClient client = new TcpClient(targetIPAddress, targetPort))
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                    // Send the message type first
                    writer.WriteLine("Output");

                    // Send the output of the executed Python code
                    writer.WriteLine(output);

                    Console.WriteLine($"Sent output to {targetIPAddress}:{targetPort}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending output to {targetIPAddress}:{targetPort}: {ex.Message}");
            }
        }
        private void SendHelpRequestToServer(string targetIPAddress, int targetPort)
        {
            try
            {
                using (TcpClient client = new TcpClient(targetIPAddress, targetPort))
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                    // Send the message type first
                    writer.WriteLine("HelpRequest");

                    // Then send the source client's IP and port (i.e., this client's IP and port)
                    writer.WriteLine(providedIPAddress); // this client's IP
                    writer.WriteLine(providedPort.ToString()); // this client's port

                    // Then send the actual help request message
                    writer.WriteLine("Do you need help?");

                    Console.WriteLine($"Sent help message to {targetIPAddress}:{targetPort}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending help message to {targetIPAddress}:{targetPort}: {ex.Message}");
            }
        }

        private async Task SendHelpMessageAsync(string ipAddress, int port)
        {
            try
            {
                SendHelpRequestToServer(ipAddress, port);
                // Update the 'NeedHelp' status of the client to 'No' after sending the help message
                await _clientApiService.UpdateClientNoHelpAsync(ipAddress, port);
                Console.WriteLine($"Updated NeedHelp status to 'No' for {ipAddress}:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending help message to {ipAddress}:{port}: {ex.Message}");
            }
        }
        private async Task InitializeAsync()
        {
            OpenDialog openDialog = new OpenDialog();

            if (openDialog.ShowDialog() == true)
            {
                providedIPAddress = openDialog.IPAddress;
                providedPort = openDialog.Port; // Get the port from user input

                // Send IP and port to the server
                SendToServer("Connection", providedIPAddress, providedPort.ToString());

                // Register the client in the WebAPI server
                try
                {
                    await _clientApiService.RegisterClientAsync(providedIPAddress, providedPort);
                    MessageBox.Show("Client registered successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error registering client: " + ex.Message);
                    this.Close();  // Close the MainWindow if registration fails
                }
            }
            else
            {
                this.Close();  // Close the MainWindow if the user cancels the OpenDialog or doesn't provide valid input
            }
        }


        private void BrowsePythonCodeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python Files|*.py|All Files|*.*";
            openFileDialog.DefaultExt = ".py";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    uploadedPythonFilePath = openFileDialog.FileName;  // Save the file path
                    uploadedPythonFileName = System.IO.Path.GetFileName(uploadedPythonFilePath);
                    var pythonCodeContent = File.ReadAllText(uploadedPythonFilePath);
                    PythonCodeInput.Text = pythonCodeContent;  // Set the TextBox to the actual Python code

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Send the filename, code, and the actual Python code to the server
            SendToServer("Upload", uploadedPythonFileName, PythonCodeInput.Text, File.ReadAllText(uploadedPythonFilePath));

            Console.WriteLine("Before updating");
            // Update the 'NeedHelp' property after uploading the Python file
            await _clientApiService.UpdateClientNeedHelpAsync(providedIPAddress, providedPort);
            Console.WriteLine("After updating");
        }

        private void SendToServer(string messageType, string firstData, string secondData, string pythonCode = null)
        {
            try
            {
                using (TcpClient client = new TcpClient(providedIPAddress, providedPort))  // Connect to the server using providedIPAddress and providedPort
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                    StreamReader reader = new StreamReader(stream);

                    // Start the communication
                    writer.WriteLine(messageType);
                    writer.WriteLine(firstData);
                    writer.WriteLine(secondData);

                    // If Python code is provided, send it to the server
                    if (!string.IsNullOrEmpty(pythonCode))
                    {
                        writer.WriteLine(pythonCode);
                    }

                    // Read the response from the server
                    string response = reader.ReadLine();
                    MessageBox.Show(response);  // Show the server's response
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while communicating with the server: " + ex.Message);
            }
        }

    }
}
