﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string NeedHelp { get; set; } = "No"; // setting a default value

        public string? PythonCode { get; set; } // To store the Python code
        public string? OutputMessage { get; set; } // To store the output after execution
        public int JobsCompleted { get; set; } = 0;
    }

}

