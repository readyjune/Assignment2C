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
    public class JobsController : ControllerBase
    {
        private readonly ClientContext _context;

        public JobsController(ClientContext context)
        {
            _context = context;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        // GET: api/Jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return job;
        }

        // DELETE: api/Jobs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // (Optional) GET: api/Jobs/ForClient/{clientId}
        // Get all jobs for a specific client
        [HttpGet("ForClient/{clientId}")]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobsForClient(int clientId)
        {
            return await _context.Jobs.Where(j => j.ClientId == clientId).ToListAsync();
        }

        // POST: api/Jobs
        [HttpPost]
        public async Task<ActionResult<Job>> CreateJob(Job job)
        {
            if (job == null)
            {
                return BadRequest("Invalid job data.");
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }
    }
}
