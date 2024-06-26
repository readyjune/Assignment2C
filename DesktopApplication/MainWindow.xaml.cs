﻿
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
using System.Windows.Controls;
using WebAPI.Models;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Client> clientsList = new List<Client>();
        private List<Job> pendingJobs = new List<Job>();

        private readonly ServerThread? serverThread;
        private NetworkingThread? networkingThread;
        private static int jobsCompleted = 0;

        private readonly string? providedIPAddress;
        private readonly int providedPort;
        private bool isNetworkingActive = false;
        private string uploadedPythonFilePath = string.Empty;
        private string uploadedPythonFileName = string.Empty;
        private readonly ClientService _clientService = new ClientService();
        private readonly JobService _jobService = new JobService();
        private readonly JobExecutor _jobExecutor = new JobExecutor();

        public MainWindow()
        {
            var startupDialog = new StartupDialog();
            if (startupDialog.ShowDialog() == true)
            {
                providedIPAddress = startupDialog.IPAddress;
                providedPort = startupDialog.Port;

                InitializeComponent();
                clientDataGrid.ItemsSource = clientsList;

                networkingThread = new NetworkingThread(clientsList, RefreshClientList, this);
                serverThread = new ServerThread(clientsList, RefreshClientList);

                serverThread.Start();
                networkingThread.Start();

                FetchClientsAndJobsOnStartup();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }
        private async void FetchClientsAndJobsOnStartup()
        {
            clientsList = await _clientService.FetchClientsAsync();
            RefreshClientList();

            pendingJobs = await _jobService.FetchJobsAsync();
            RefreshPendingJobsList();
        }
        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registeredClient = await _clientService.RegisterClientAsync(providedIPAddress ?? "", providedPort);
            if (registeredClient != null)
            {
                clientsList.Add(registeredClient);
                RefreshClientList();
                JobStatus.Text = "Registration successful!";
            }
            else
            {
                JobStatus.Text = "Registration failed. Check your Web Service.";
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
            clientsList = await _clientService.FetchClientsAsync();
            RefreshClientList();
        }
        private void RefreshClientList()
        {
            // Update the ListView with the latest data
            clientDataGrid.ItemsSource = null;
            clientDataGrid.ItemsSource = clientsList;
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
                    uploadedPythonFileName = Path.GetFileName(uploadedPythonFilePath);
                    var pythonCodeContent = File.ReadAllText(uploadedPythonFilePath);
                    PythonCodeInput.Text = pythonCodeContent;  // Set the TextBox to the actual Python code
                    //PythonCodeInput.Text = uploadedPythonFilePath;
                    JobStatus.Text = "Python code loaded successfully!";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var pythonCode = PythonCodeInput.Text;

            if (!string.IsNullOrWhiteSpace(pythonCode) && await _jobService.SubmitJobAsync(pythonCode, uploadedPythonFilePath, providedIPAddress, providedPort))
            {
                MessageBox.Show("Job added to queue!");
                pendingJobs = await _jobService.FetchJobsAsync();
                RefreshPendingJobsList();
            }
            else
            {
                MessageBox.Show("Job submission failed. Check your Web Service.");
            }
        }



        private void RefreshPendingJobsList()
        {
            // Assuming you have a ListBox named "JobsListBox" in your XAML
            JobsDataGrid.ItemsSource = null;
            JobsDataGrid.ItemsSource = pendingJobs;
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
                networkingThread = new NetworkingThread(clientsList, RefreshClientList, this);
                networkingThread.Start();
                NetworkingButton.Content = "Stop Networking";
                isNetworkingActive = true;
            }
            // Update the status label
            UpdateStatusLabel();
            // Update the status label based on whether the thread is working or idle

        }

        public void IncrementJobsCompleted(Client client)
        {
            if (client.IPAddress == providedIPAddress && client.Port == providedPort)
            {
                Dispatcher.Invoke(() =>
                {
                    jobsCompleted++;
                    UpdateStatusLabel();
                });
            }
        }
        private void UpdateStatusLabel()
        {
            bool isWorking = networkingThread?.IsWorking ?? false;
            string statusText = isWorking ? "Working" : "Idle";
            StatusLabel.Content = $"Status: {statusText}, Jobs Completed: {jobsCompleted}";
        }
        private void QueryNetworkingStatusButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatusLabel();
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
                        client.JobsCompleted++; // Increase the jobs completed count
                    }
                }
                client.ProgressValue = 0;
            });
        }


        private async void RefreshJobsButton_Click(object sender, RoutedEventArgs e)
        {
            pendingJobs = await _jobService.FetchJobsAsync();
            RefreshPendingJobsList();
        }

        private void DownloadPythonCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Job job && job.PythonCode != null)
            {
                // Use the job's filename directly instead of opening a new SaveFileDialog
                var saveFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), job.FileName);

                try
                {
                    File.WriteAllText(saveFilePath, job.PythonCode);
                    MessageBox.Show($"Python code saved successfully to {saveFilePath}!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private async void ExecutePythonCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Job job && job.PythonCode != null)
            {
                // Disable the Execute button to prevent multiple clicks
                button.IsEnabled = false;

                // Get the matching client and set its IsBusy to true
                var matchingClient = clientsList.FirstOrDefault(c => c.IPAddress == providedIPAddress && c.Port == providedPort);
                if (matchingClient != null)
                {
                    matchingClient.IsBusy = true;
                    RefreshClientList();  // To reflect the updated IsBusy status for the client
                }

                // Start the progress bar animation
                int progressInterval = 100 / 5; // Increase progress by 20% every second
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(1000); // Delay for 1 second

                    // Update the progress value of the specific job
                    job.ProgressValue += progressInterval;

                    // Refresh the job data grid to reflect the updated progress
                    JobsDataGrid.Items.Refresh();
                }

                // Actually execute the Python code
                string result = ExecutePythonCode(job.PythonCode);

                // Update the job's Result property with the execution output
                job.Result = result;

                // If the client was found earlier, increment its JobsCompleted count and set its IsBusy to false
                if (matchingClient != null)
                {
                    matchingClient.JobsCompleted++;
                    matchingClient.IsBusy = false;
                }

                // Optionally, display the result to the user
                MessageBox.Show($"Output: {result}");

                // Refresh the jobs results list to reflect the updated result
                RefreshJobResultsList();
                RefreshClientList();  // To reflect the updated JobsCompleted count and IsBusy status for the client

                // Reset progress value for the job and re-enable the Execute button
                job.ProgressValue = 0;
                button.IsEnabled = true;
            }
        }




        private string ExecutePythonCode(string code)
        {
            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope = engine.CreateScope();
            string output = string.Empty;

            // Create a stream to capture the printed output
            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            {
                // Redirect the output stream of the IronPython engine to this stream
                engine.Runtime.IO.SetOutput(ms, sw);

                try
                {
                    engine.Execute(code, scope);

                    // Go back to the beginning of the stream to read its contents
                    ms.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(ms))
                    {
                        output = sr.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    output = "Error executing Python code: " + ex.Message;
                }
            }
            return output;
        }


        private void RefreshJobResultsList()
        {
            JobResultsDataGrid.ItemsSource = null;
            JobResultsDataGrid.ItemsSource = pendingJobs; // Assuming completedJobs is a list of jobs that have been executed
        }
    }
}
