using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerSide.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string? IPAddress { get; set; }
        public int Port { get; set; }
        public string? NeedHelp { get; set; }

        public string? PythonCode { get; set; } // To store the Python code
        public string? OutputMessage { get; set; } // To store the output after execution
    }

}

