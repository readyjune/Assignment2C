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

        public MainWindow()
        {
            InitializeComponent();
            _clientApiService = new ClientApiService("http://localhost:5074/api");

            // Attach the Loaded event to call InitializeAsync method
            this.Loaded += async (sender, e) => await InitializeAsync();
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

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Send the filename and code to the server
            SendToServer("Upload", uploadedPythonFileName, PythonCodeInput.Text);
        }

        private void SendToServer(string messageType, string firstData, string secondData)
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
