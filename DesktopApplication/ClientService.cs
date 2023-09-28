using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;

namespace DesktopApplication
{
    public class ClientService
    {
        private readonly string _apiBase = "http://localhost:5074/api/clients";

        public async Task<List<Client>> FetchClientsAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(_apiBase);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Client>>(jsonString);
                }
                return new List<Client>();
            }
        }

        public async Task<Client?> RegisterClientAsync(string ipAddress, int port)
        {
            try
            {
                var client = new Client
                {
                    IPAddress = ipAddress,
                    Port = port
                };

                using (HttpClient httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(client), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{_apiBase}/Register", jsonContent);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<Client>(responseContent);
                    }
                }
            }
            catch
            {
                // Log exception or handle accordingly
            }
            return null;
        }
    }

}
