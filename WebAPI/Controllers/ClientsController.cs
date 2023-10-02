using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ClientContext _context;
        private readonly object _lock = new object();  // Lock for concurrency control
        public ClientsController(ClientContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        // POST: api/Clients/Register
        [HttpPost("Register")]
        public async Task<ActionResult<Client>> RegisterClient(Client client)
        {
            lock (_lock)
            {
                var existingClient = _context.Clients.FirstOrDefault(c => c.IPAddress == client.IPAddress && c.Port == client.Port);
                if (existingClient != null)
                {
                    return BadRequest("IP and Port combination already registered.");
                }

                _context.Clients.Add(client);
                _context.SaveChanges();
            }

            return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
        }

        // DELETE: api/Clients
        [HttpDelete]
        public async Task<ActionResult<Client>> DeleteClient(string ipAddress, int port)
        {
            lock (_lock)
            {
                var client = _context.Clients.FirstOrDefault(c => c.IPAddress == ipAddress && c.Port == port);
                if (client == null)
                {
                    return NotFound();
                }

                _context.Clients.Remove(client);
                _context.SaveChanges();
            }

            return NoContent();
        }

        [HttpPut("update-need-help")]
        public async Task<IActionResult> UpdateClientNeedHelp([FromBody] Client clientUpdate)
        {
            try
            {
                lock (_lock)
                {
                    var client = _context.Clients.FirstOrDefault(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                    if (client == null)
                    {
                        Console.WriteLine("Client not found.");
                        return NotFound();
                    }

                    client.NeedHelp = clientUpdate.NeedHelp;

                    _context.Entry(client).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                Console.WriteLine("NeedHelp updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating 'NeedHelp' property: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPut("update-python-code")]
        public async Task<IActionResult> UpdateClientPythonCode([FromBody] Client clientUpdate)
        {
            try
            {
                lock (_lock)
                {
                    var client = _context.Clients.FirstOrDefault(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                    if (client == null)
                    {
                        return NotFound();
                    }

                    client.PythonCode = clientUpdate.PythonCode;

                    _context.Entry(client).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Python code: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }



        [HttpPut("update-output")]
        public async Task<IActionResult> UpdateClientOutput([FromBody] Client clientUpdate)
        {
            try
            {
                lock (_lock)
                {
                    var client = _context.Clients.FirstOrDefault(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                    if (client == null)
                    {
                        Console.WriteLine("Client not found.");
                        return NotFound();
                    }

                    client.OutputMessage = clientUpdate.OutputMessage;

                    _context.Entry(client).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                Console.WriteLine("Output updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating output: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        // GET: api/Clients/get-python-code
        [HttpGet("get-python-code")]
        public async Task<ActionResult<string>> GetPythonCode(string ipAddress, int port)
        {
            // Find the client based on the provided IP address and port
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == ipAddress && c.Port == port);

            if (client == null)
            {
                return NotFound("Client not found.");
            }

            return Ok(client.PythonCode); // Return the Python code associated with the client
        }

        [HttpPut("update-jobs-completed")]
        public async Task<IActionResult> UpdateJobsCompleted([FromBody] Client clientUpdate)
        {
            try
            {
                lock (_lock)
                {
                    var client = _context.Clients.FirstOrDefault(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                    if (client == null)
                    {
                        return NotFound();
                    }

                    client.JobsCompleted = clientUpdate.JobsCompleted;

                    _context.Entry(client).State = EntityState.Modified;
                    _context.SaveChanges();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating JobsCompleted: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        // GET: api/Clients/client-details
        [HttpGet("client-details")]
        public async Task<ActionResult<Client>> GetClientDetails(string ipAddress, int port)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == ipAddress && c.Port == port);
            if (client == null)
            {
                return NotFound("Client not found.");
            }
            return Ok(client);
        }

    }
}
