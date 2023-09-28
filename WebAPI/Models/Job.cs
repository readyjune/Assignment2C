namespace WebAPI.Models
{
    public class Job
    {
        public int Id { get; set; }  // Unique identifier for the job
        public int? ClientId { get; set; }
        public string? FileName { get; set; }
        public string? PythonCode { get; set; } // The Python code to execute
        public string? InputData { get; set; }  // Any input data for the Python code
        public string? OutputData { get; set; } 
        public string? Status { get; set; }     // e.g., pending, in progress, completed
        public string? Result { get; set; }     // Output/result of the Python execution
        public string? IPAddress { get; set; }
        public int? Port { get; set; }
        public int ProgressValue { get; set; } = 0;
    }
}
