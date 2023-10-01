using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Controllers
{
    public class ClientsWebController : Controller
    {
        private readonly ClientContext _context;

        public ClientsWebController(ClientContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            return View("ClientsInfo", clients);  // Render the Razor view with the client data
        }
    }
}
