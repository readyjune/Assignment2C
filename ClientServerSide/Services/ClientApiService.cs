using ClientServerSide.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public async Task RegisterClientAsync(string ipAddress, int port)
        {
            var client = new
            {
                IPAddress = ipAddress,
                Port = port
            };

            var content = new StringContent(JsonConvert.SerializeObject(client), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseApiUrl}/Clients/Register", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error registering client: {response.ReasonPhrase}");
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
