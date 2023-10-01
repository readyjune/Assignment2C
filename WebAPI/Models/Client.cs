using System.ComponentModel;

namespace WebAPI.Models
{
    public class Client 
    {
        public int Id { get; set; }
        public string? IPAddress { get; set; }
        public int Port { get; set; }
        public string? NeedHelp { get; set; }

        public string? PythonCode { get; set; } // To store the Python code
        public string? OutputMessage { get; set; } // To store the output after execution
        public int JobsCompleted { get; set; } = 0;
    }

}
