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


    }
}
