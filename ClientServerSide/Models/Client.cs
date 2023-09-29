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
        public int JobsCompleted { get; set; }
        public bool IsBusy { get; set; }
        public double ProgressValue { get; set; }
    }
}
