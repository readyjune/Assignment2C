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
            // Check if the IP and Port are already registered
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == client.IPAddress && c.Port == client.Port);
            if (existingClient != null)
            {
                return BadRequest("IP and Port combination already registered.");
            }

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
        }

        // DELETE: api/Clients
        [HttpDelete]
        public async Task<ActionResult<Client>> DeleteClient(string ipAddress, int port)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == ipAddress && c.Port == port);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("update-need-help")]
        public async Task<IActionResult> UpdateClientNeedHelp([FromBody] Client clientUpdate)
        {
            try
            {
                // Find the client based on IPAddress and Port
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                if (client == null)
                {
                    Console.WriteLine("Client not found."); // Log client not found
                    return NotFound();
                }

                // Update the 'NeedHelp' property based on the request data
                client.NeedHelp = clientUpdate.NeedHelp;

                _context.Entry(client).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                Console.WriteLine("NeedHelp updated successfully."); // Log success

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating 'NeedHelp' property: {ex.Message}");
                return StatusCode(500, "Internal server error"); // Log and return a 500 status code on error
            }
        }



        [HttpPut("update-python-code")]
        public async Task<IActionResult> UpdateClientPythonCode([FromBody] Client clientUpdate)
        {
            try
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                if (client == null)
                {
                    return NotFound();
                }

                // Update the 'PythonCode' property based on the request data
                client.PythonCode = clientUpdate.PythonCode;

                _context.Entry(client).State = EntityState.Modified;
                await _context.SaveChangesAsync();

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
                // Find the client based on IPAddress and Port
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);

                if (client == null)
                {
                    Console.WriteLine("Client not found."); // Log client not found
                    return NotFound();
                }

                // Update the 'Output' property based on the request data
                client.OutputMessage = clientUpdate.OutputMessage;

                _context.Entry(client).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                Console.WriteLine("Output updated successfully."); // Log success

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating output: {ex.Message}");
                return StatusCode(500, "Internal server error"); // Log and return a 500 status code on error
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
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.IPAddress == clientUpdate.IPAddress && c.Port == clientUpdate.Port);
                if (client == null)
                {
                    return NotFound();
                }

                client.JobsCompleted = clientUpdate.JobsCompleted;

                _context.Entry(client).State = EntityState.Modified;
                await _context.SaveChangesAsync();

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
