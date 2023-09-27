using IronPython.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using WebAPI.Models;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Client> clientsList = new List<Client>(); // Initialize the list
        private readonly ServerThread serverThread;
        private NetworkingThread networkingThread;
        private static int jobsCompleted = 0; // Static counter for completed jobs

        private readonly string providedIPAddress;
        private readonly int providedPort;
        private bool isNetworkingActive = false; // A flag to track the state of networking


        public MainWindow()
        {
            var startupDialog = new StartupDialog();
            if (startupDialog.ShowDialog() == true)
            {
                providedIPAddress = startupDialog.IPAddress;
                providedPort = startupDialog.Port;

                InitializeComponent();
                clientDataGrid.ItemsSource = clientsList;

                networkingThread = new NetworkingThread(clientsList, RefreshClientList);
                serverThread = new ServerThread(clientsList, RefreshClientList);

                serverThread.Start();
                networkingThread.Start();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new Client
                {
                    IPAddress = providedIPAddress,
                    Port = providedPort
                };

                using (HttpClient httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(client), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("http://localhost:5074/api/clients/Register", jsonContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    MessageBox.Show($"Response Status Code: {response.StatusCode}\nResponse Content: {responseContent}");

                    if (response.IsSuccessStatusCode)
                    {
                        var returnedClient = JsonConvert.DeserializeObject<Client>(responseContent);
                        clientsList.Add(returnedClient);
                        RefreshClientList();
                        MessageBox.Show("Registration successful!");
                    }
                    else
                    {
                        MessageBox.Show("Registration failed. Check your Web Service.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }



        private async void StartJobButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create an HttpClient to send requests to the WebAPI
                using (HttpClient httpClient = new HttpClient())
                {
                    // Define the job data (you may need to adjust this)
                    var jobData = new
                    {
                        TaskType = "PythonExecution",
                        PythonCode = PythonCodeInput.Text
                    };

                    // Serialize the job data to JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(jobData), Encoding.UTF8, "application/json");

                    foreach (var client in clientsList)
                    {
                        // Check if the client is busy
                        if (client.IsBusy)
                        {
                            // Skip this client if they are busy
                            continue;
                        }

                        var apiUrl = $"http://localhost:5074/api/clients/{client.Id}/jobCompleted"; // Construct the API URL

                        var response = await httpClient.PostAsync(apiUrl, jsonContent);

                        // Check if the job initiation was successful for this client
                        if (response.IsSuccessStatusCode)
                        {
                            // Job initiation successful for this client, you can update UI or perform other actions
                            MessageBox.Show($"Job started for client {client.Id}");

                            // Mark the client as busy
                            client.IsBusy = true;
                        }
                        else
                        {
                            // Handle job initiation failure for this client
                            // You may want to log the error or take appropriate action
                            MessageBox.Show($"Failed to start job for client {client.Id}. Check your Web Service.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Send a GET request to the WebAPI's endpoint to retrieve client information
                    var response = await httpClient.GetAsync("http://localhost:5074/api/clients");

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response JSON to get the list of clients
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var updatedClientsList = JsonConvert.DeserializeObject<List<Client>>(jsonString); // Store the updated list

                        // Update the existing clientsList without reassigning it
                        clientsList.Clear();
                        foreach (var client in updatedClientsList)
                        {
                            clientsList.Add(client);
                        }

                        // Update the ListView with the latest data
                        RefreshClientList();
                    }
                    else
                    {
                        MessageBox.Show("Failed to retrieve client information. Check your Web Service.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void RefreshClientList()
        {
            // Update the ListView with the latest data
            clientDataGrid.ItemsSource = null;
            clientDataGrid.ItemsSource = clientsList;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string pythonCode = PythonCodeInput.Text;

            try
            {
                var engine = Python.CreateEngine();
                var scope = engine.CreateScope();
                var script = engine.CreateScriptSourceFromString(pythonCode);
                script.Execute(scope);

                // Handle the execution result as needed
                // For example, you can update the UI or perform other actions.
                MessageBox.Show("Python code executed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Python code execution error: " + ex.Message);
            }
        }

        private void NetworkingButton_Click(object sender, RoutedEventArgs e)
        {
            if (isNetworkingActive)
            {
                // Stop the networking thread
                networkingThread.Stop(); // You'd need to implement a Stop method in your NetworkingThread class
                NetworkingButton.Content = "Start Networking";
                isNetworkingActive = false;
            }
            else
            {
                // Recreate and start the networking thread
                networkingThread = new NetworkingThread(clientsList, RefreshClientList);
                networkingThread.Start();
                NetworkingButton.Content = "Stop Networking";
                isNetworkingActive = true;
            }

            // Update the status label
            StatusLabel.Content = $"Status: {(networkingThread.IsWorking ? "Working" : "Idle")}, Jobs Completed: {jobsCompleted}";
        }

        public static void IncrementJobsCompleted()
        {
            jobsCompleted++;
        }
        private void QueryNetworkingStatusButton_Click(object sender, RoutedEventArgs e)
        {
            bool isWorking = networkingThread.IsWorking; // Assuming you've implemented this method in NetworkingThread
            StatusLabel.Content = $"Status: {(isWorking ? "Working" : "Idle")}, Jobs Completed: {jobsCompleted}";
        }
    }
}
