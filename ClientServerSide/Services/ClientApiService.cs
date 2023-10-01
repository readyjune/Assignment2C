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
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseApiUrl;

        public ClientApiService(string baseApiUrl)
        {
            _baseApiUrl = baseApiUrl;
        }

        public async Task UpdateClientNeedHelpAsync(string ipAddress, int port)
        {
            try
            {
                var updateRequest = new
                {
                    IPAddress = ipAddress,
                    Port = port,
                    NeedHelp = "Yes"
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseApiUrl}/clients/update-need-help", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("NeedHelp updated successfully."); // Log success
                }
                else
                {
                    Console.WriteLine($"Error updating 'NeedHelp' property: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating 'NeedHelp' property: {ex.Message}");
            }
        }

        public async Task UpdateClientNoHelpAsync(string ipAddress, int port)
        {
            try
            {
                var updateRequest = new
                {
                    IPAddress = ipAddress,
                    Port = port,
                    NeedHelp = "No" // Set NeedHelp to "No"
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseApiUrl}/clients/update-need-help", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("NeedHelp updated to 'No' successfully."); // Log success
                }
                else
                {
                    Console.WriteLine($"Error updating 'NeedHelp' property to 'No': {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating 'NeedHelp' property to 'No': {ex.Message}");
            }
        }

        public async Task UpdateClientPythonCode(string clientIP, int clientPort, string pythonCode)
        {
            try
            {
                var updateRequest = new
                {
                    IPAddress = clientIP,
                    Port = clientPort,
                    PythonCode = pythonCode
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseApiUrl}/clients/update-python-code", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Python code updated successfully for client {clientIP}:{clientPort}.");
                }
                else
                {
                    Console.WriteLine($"Error updating Python code for client {clientIP}:{clientPort}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Python code for client {clientIP}:{clientPort}: {ex.Message}");
            }
        }

        public async Task RegisterClientAsync(string ipAddress, int port)
        {
            try
            {
                var client = new
                {
                    IPAddress = ipAddress,
                    Port = port
                };

                var content = new StringContent(JsonConvert.SerializeObject(client), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseApiUrl}/clients/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error registering client: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering client: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/Clients");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Client>>(content);
        }

        public async Task UnregisterClientAsync(int clientId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseApiUrl}/Clients/{clientId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error unregistering client: {response.ReasonPhrase}");
            }
        }

        public async Task UpdateClientOutputAsync(string ipAddress, int port, string output)
        {
            try
            {
                var updateRequest = new
                {
                    IPAddress = ipAddress,
                    Port = port,
                    OutputMessage = output
                };

                var content = new StringContent(JsonConvert.SerializeObject(updateRequest), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseApiUrl}/clients/update-output", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Output updated successfully for client {ipAddress}:{port}.");
                }
                else
                {
                    Console.WriteLine($"Error updating output for client {ipAddress}:{port}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating output for client {ipAddress}:{port}: {ex.Message}");
            }
        }

        public async Task DeleteClientAsync(string ipAddress, int port)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseApiUrl}/clients?ipAddress={ipAddress}&port={port}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error deleting client {ipAddress}:{port}: {response.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine($"Client {ipAddress}:{port} deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting client {ipAddress}:{port}: {ex.Message}");
            }
        }

        public async Task IncrementJobsCompletedAsync(string ipAddress, int port)
        {
            try
            {
                var clientToUpdate = await _httpClient.GetAsync($"{_baseApiUrl}/Clients?ipAddress={ipAddress}&port={port}");
                
                

                if (clientToUpdate.IsSuccessStatusCode)
                {
                    var responseBody = await clientToUpdate.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response Body from /Clients endpoint: {responseBody}");

                    // Deserialize into a list of Client objects
                    var clients = JsonConvert.DeserializeObject<List<Client>>(responseBody);

                    // Find the appropriate client based on ipAddress and port
                    var client = clients.FirstOrDefault(c => c.IPAddress == ipAddress && c.Port == port);

                    if (client == null)
                    {
                        Console.WriteLine($"No client found with IP {ipAddress} and port {port}.");
                        return;
                    }

                    client.JobsCompleted++;

                    var content = new StringContent(JsonConvert.SerializeObject(client), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PutAsync($"{_baseApiUrl}/clients/update-jobs-completed", content);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"JobsCompleted incremented for client {ipAddress}:{port}.");
                    }
                    else
                    {
                        Console.WriteLine($"Error incrementing JobsCompleted for client {ipAddress}:{port}: {response.ReasonPhrase}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error fetching client {ipAddress}:{port} for incrementing: {clientToUpdate.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error incrementing JobsCompleted for client {ipAddress}:{port}: {ex.Message}");
            }
        }


    }
}
