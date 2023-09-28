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

        // DELETE: api/Clients/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<Client>> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Clients/{id}/jobCompleted
        [HttpPost("{id}/jobCompleted")]
        public async Task<ActionResult<Client>> JobCompleted(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            client.JobsCompleted += 1;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Clients/{id}/AssignJob
        [HttpPost("{id}/AssignJob")]
        public async Task<ActionResult<Job>> AssignJob(int id, Job job)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            if (client.IsBusy)
            {
                return BadRequest("Client is currently busy.");
            }

            // You can add additional validations here if needed

            job.ClientId = id;
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            client.IsBusy = true;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(AssignJob), new { id = job.Id }, job);
        }

     
    }
}
