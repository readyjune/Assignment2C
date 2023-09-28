
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using WebAPI.Models;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Client> clientsList = new List<Client>(); // Initialize the list
        private List<Job> pendingJobs = new List<Job>();

        private readonly ServerThread? serverThread; // Use nullable type
        private NetworkingThread? networkingThread; // Use nullable type
        private static int jobsCompleted = 0; // Static counter for completed jobs

        private readonly string? providedIPAddress; // Use nullable type
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

                // Fetch clients on startup
                FetchClientsAsync();
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
                        if (returnedClient != null)
                        {
                            clientsList.Add(returnedClient);
                        }
                        RefreshClientList();
                        JobStatus.Text = "Registration successful!";
                    }
                    else
                    {
                        JobStatus.Text = "Registration failed. Check your Web Service.";
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
            // Ensure this runs on the UI thread
            Dispatcher.Invoke(() =>
            {
                JobStatus.Text = "Starting jobs for available clients...";
            });

            // Check if there are pending jobs
            if (pendingJobs.Count == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    JobStatus.Text = "No pending jobs.";
                });
                return;
            }

            var jobToExecute = pendingJobs.First();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var jobData = new
                    {
                        TaskType = "PythonExecution",
                        PythonCode = jobToExecute.PythonCode  // Getting the code from the job now
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(jobData), Encoding.UTF8, "application/json");

                    foreach (var client in clientsList)
                    {
                        if (client.IsBusy)
                        {
                            continue;
                        }

                        var apiUrl = $"http://localhost:5074/api/clients/{client.Id}/jobCompleted";

                        var response = await httpClient.PostAsync(apiUrl, jsonContent);

                        // Ensure the following code runs on the UI thread
                        Dispatcher.Invoke(() =>
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show($"Job started for client {client.Id}");
                                client.IsBusy = true;
                                jobToExecute.Status = "In Progress";  // Updating the job status
                                IncrementProgressForClient(client);
                                RefreshClientList();

                            }
                            else
                            {
                                MessageBox.Show($"Failed to start job for client {client.Id}. Check your Web Service.");
                            }
                        });
                    }

                    // Ensure this runs on the UI thread
                    Dispatcher.Invoke(() =>
                    {
                        JobStatus.Text = "Job initiation completed. Check individual client statuses.";
                        RefreshPendingJobsList();  // Refresh the jobs list to reflect the status change
                    });

                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                });
            }
        }



        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await FetchClientsAsync();
        }
        private void RefreshClientList()
        {
            // Update the ListView with the latest data
            clientDataGrid.ItemsSource = null;
            clientDataGrid.ItemsSource = clientsList;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var pythonCode = PythonCodeInput.Text;

            if (string.IsNullOrWhiteSpace(pythonCode))
            {
                MessageBox.Show("Please provide valid Python code.");
                return;
            }

            var newJob = new Job
            {
                PythonCode = pythonCode,
                Status = "Pending"
            };

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(newJob), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("http://localhost:5074/api/jobs", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Job added to queue!");
                        await FetchJobsAsync();  // Refresh the job list after submitting
                    }
                    else
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"HTTP Error: {response.StatusCode}\nReason: {response.ReasonPhrase}\nBody: {responseBody}");
                        JobStatus.Text = "Failed to retrieve client information. Check your Web Service.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private async Task FetchJobsAsync()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("http://localhost:5074/api/jobs");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var updatedJobsList = JsonConvert.DeserializeObject<List<Job>>(jsonString);

                        pendingJobs.Clear();
                        foreach (var job in updatedJobsList)
                        {
                            pendingJobs.Add(job);
                        }

                        RefreshPendingJobsList();
                        JobStatus.Text = "Job information refreshed successfully.";
                    }
                    else
                    {
                        JobStatus.Text = "Failed to retrieve job information. Check your Web Service.";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }


        private void RefreshPendingJobsList()
        {
            // Assuming you have a ListBox named "JobsListBox" in your XAML
            JobsListBox.ItemsSource = null;
            JobsListBox.ItemsSource = pendingJobs;
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

            // Update the status label based on whether the thread is working or idle
            bool isWorking = networkingThread?.IsWorking ?? false; // Assuming the IsWorking property exists in NetworkingThread

            if (isWorking)
            {
                StatusLabel.Content = $"Status: Working, Jobs Completed: {jobsCompleted}";
            }
            else
            {
                // Check if networking is active but the thread is not working
                if (isNetworkingActive)
                {
                    StatusLabel.Content = $"Status: Ready, Jobs Completed: {jobsCompleted}";
                }
                else
                {
                    StatusLabel.Content = $"Status: Idle, Jobs Completed: {jobsCompleted}";
                }
            }
        }

        public static void IncrementJobsCompleted()
        {
            jobsCompleted++;
        }
        private void QueryNetworkingStatusButton_Click(object sender, RoutedEventArgs e)
        {
            bool isWorking = networkingThread?.IsWorking ?? false;  // Assuming you've implemented this method in NetworkingThread
            StatusLabel.Content = $"Status: {(isWorking ? "Working" : "Idle")}, Jobs Completed: {jobsCompleted}";
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
                    var pythonCode = File.ReadAllText(openFileDialog.FileName);
                    PythonCodeInput.Text = pythonCode;
                    JobStatus.Text = "Python code loaded successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private async Task FetchClientsAsync()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("http://localhost:5074/api/clients");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var updatedClientsList = JsonConvert.DeserializeObject<List<Client>>(jsonString);

                        clientsList.Clear();
                        clientsList.AddRange(updatedClientsList);
                        RefreshClientList();
                        JobStatus.Text = "Client information refreshed successfully.";
                    }
                    else
                    {
                        JobStatus.Text = $"Failed to retrieve client information. Status: {response.StatusCode}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }


        private async void IncrementProgressForClient(Client client)
        {
            // Example to fake a progress increment for a client
            await Task.Run(async () =>
            {
                while (client.ProgressValue < 100)
                {
                    await Task.Delay(1000); // Delay for 1 second
                    Dispatcher.Invoke(() =>
                    {
                        client.ProgressValue += 10;
                    });


                    if (client.ProgressValue >= 100)
                    {
                        client.ProgressValue = 100;
                        client.IsBusy = false;
                        
                    }
                }
                client.ProgressValue = 0;
                
            });
        }


        private async void RefreshJobsButton_Click(object sender, RoutedEventArgs e)
        {
            await FetchJobsAsync();
        }

        

    }
}
