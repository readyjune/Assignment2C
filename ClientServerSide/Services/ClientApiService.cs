using ClientServerSide.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer.Services
{
    public class ClientApiService
    {
        // Singleton HttpClient
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseApiUrl;

        public ClientApiService(string baseApiUrl)
        {
            _baseApiUrl = baseApiUrl;

        }
        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? data = null)
        {
            var request = new HttpRequestMessage(method, $"{_baseApiUrl}/{endpoint}");

            if (data != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            }

            return await _httpClient.SendAsync(request);
        }
        public async Task UpdateClientNeedHelpAsync(string ipAddress, int port)
        {
            var client = new Client
            {
                IPAddress = ipAddress,
                Port = port,
                NeedHelp = "Yes"
            };
            var response = await SendAsync(HttpMethod.Put, "clients/update-need-help", client);
            HandleResponse(response, $"Error updating 'NeedHelp' property for client {ipAddress}:{port}");
        }

        public async Task UpdateClientNoHelpAsync(string ipAddress, int port)
        {
            var client = new Client
            {
                IPAddress = ipAddress,
                Port = port,
                NeedHelp = "No"
            };
            var response = await SendAsync(HttpMethod.Put, "clients/update-need-help", client);
            HandleResponse(response, $"Error updating 'NeedHelp' property to 'No' for client {ipAddress}:{port}");
        }

        public async Task UpdateClientPythonCode(string clientIP, int clientPort, string pythonCode)
        {
            var client = new Client
            {
                IPAddress = clientIP,
                Port = clientPort,
                PythonCode = pythonCode
            };
            var response = await SendAsync(HttpMethod.Put, "clients/update-python-code", client);
            HandleResponse(response, $"Error updating Python code for client {clientIP}:{clientPort}");
        }
        private void HandleResponse(HttpResponseMessage response, string errorMessage)
        {
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{errorMessage}: {response.ReasonPhrase}");
            }
            else
            {
                Console.WriteLine($"Operation was successful for client.");
            }
        }
        public async Task RegisterClientAsync(string ipAddress, int port)
        {
            var client = new Client
            {
                IPAddress = ipAddress,
                Port = port
            };
            var response = await SendAsync(HttpMethod.Post, "clients/register", client);
            HandleResponse(response, $"Error registering client {ipAddress}:{port}");
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "Clients");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Client>>(content);
            }
            else
            {
                HandleResponse(response, "Error fetching clients.");
                return new List<Client>(); // Return an empty list on failure
            }
        }

        public async Task UnregisterClientAsync(int clientId)
        {
            var response = await SendAsync(HttpMethod.Delete, $"Clients/{clientId}");
            HandleResponse(response, $"Error unregistering client with ID {clientId}");
        }

        public async Task UpdateClientOutputAsync(string ipAddress, int port, string output)
        {
            var client = new Client
            {
                IPAddress = ipAddress,
                Port = port,
                OutputMessage = output
            };
            var response = await SendAsync(HttpMethod.Put, "clients/update-output", client);
            HandleResponse(response, $"Error updating output for client {ipAddress}:{port}");
        }

        public async Task DeleteClientAsync(string ipAddress, int port)
        {
            var response = await SendAsync(HttpMethod.Delete, $"clients?ipAddress={ipAddress}&port={port}");
            HandleResponse(response, $"Error deleting client {ipAddress}:{port}");
        }

        public async Task IncrementJobsCompletedAsync(string ipAddress, int port)
        {
            var response = await SendAsync(HttpMethod.Get, $"Clients?ipAddress={ipAddress}&port={port}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var clients = JsonConvert.DeserializeObject<List<Client>>(responseBody);

                var client = clients.FirstOrDefault(c => c.IPAddress == ipAddress && c.Port == port);

                if (client == null)
                {
                    Console.WriteLine($"No client found with IP {ipAddress} and port {port}.");
                    return;
                }

                client.JobsCompleted++;
                var updateResponse = await SendAsync(HttpMethod.Put, "clients/update-jobs-completed", client);
                HandleResponse(updateResponse, $"Error incrementing JobsCompleted for client {ipAddress}:{port}");
            }
            else
            {
                HandleResponse(response, $"Error fetching client {ipAddress}:{port} for incrementing");
            }
        }


    }
}
