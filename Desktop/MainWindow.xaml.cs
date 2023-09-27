using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using WebAPI.Models;

namespace Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create an HttpClient to send a registration request
                using (HttpClient client = new HttpClient())
                {
                    // Define the registration data (you may need to adjust this)
                    var registrationData = new
                    {
                        IPAddress = "your_ip_address_here",
                        Port = 8080, // Your chosen port number
                    };

                    // Serialize the data to JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(registrationData), Encoding.UTF8, "application/json");

                    // Send a POST request to the Web Service's registration endpoint
                    var response = await client.PostAsync("http://http://localhost:5074//api/clients/register", jsonContent);

                    // Check if the registration was successful (you may need to adjust the status code)
                    if (response.IsSuccessStatusCode)
                    {
                        // Registration successful, update UI or perform other actions
                        MessageBox.Show("Registration successful!");
                    }
                    else
                    {
                        // Registration failed, handle errors
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
                // Create an HttpClient to send requests to other clients
                using (HttpClient httpClient = new HttpClient())
                {
                    // Define the job data (you may need to adjust this)
                    var jobData = new
                    {
                        // Define the job parameters here
                    };

                    // Serialize the job data to JSON
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(jobData), Encoding.UTF8, "application/json");

                    // Send POST requests to other clients to initiate the job
                    foreach (var clientUrl in listOfClientUrls) // Replace with your list of client URLs
                    {
                        var response = await httpClient.PostAsync($"{clientUrl}/api/startjob", jsonContent);

                        // Check if the job initiation was successful for each client
                        if (response.IsSuccessStatusCode)
                        {
                            // Job initiation successful for this client, you can update UI or perform other actions
                        }
                        else
                        {
                            // Handle job initiation failure for this client
                            // You may want to log the error or take appropriate action
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
                        var clients = JsonConvert.DeserializeObject<List<Client>>(jsonString);

                        // Display the client information in your WPF UI (e.g., in a ListView or DataGrid)
                        // For example:
                        // clientListView.ItemsSource = clients;

                        // You can update your UI components with the client information as needed
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




    }
}
