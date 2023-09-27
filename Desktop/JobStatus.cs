using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desktop
{
    public class JobStatus
    {
        public int Id { get; set; }
        public string ClientName { get; set; } // Add any other properties you need
        public int JobsCompleted { get; set; }
    }
}
