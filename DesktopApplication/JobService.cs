using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;

namespace DesktopApplication
{
    public class JobService
    {
        private readonly string _apiBase = "http://localhost:5074/api/jobs";

        public async Task<List<Job>> FetchJobsAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(_apiBase);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Job>>(jsonString);
                }
                return new List<Job>();
            }
        }

        public async Task<bool> SubmitJobAsync(string pythonCode, string filePath, string? ipAddress, int? port)
        {
            try
            {
                var newJob = new Job
                {
                    PythonCode = pythonCode,
                    FileName = Path.GetFileName(filePath),
                    IPAddress = ipAddress,
                    Port = port,
                    Status = "Pending"
                };

                using (HttpClient httpClient = new HttpClient())
                {
                    var jsonContent = new StringContent(JsonConvert.SerializeObject(newJob), Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(_apiBase, jsonContent);

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                // Log exception or handle accordingly
            }
            return false;
        }
    }

}
