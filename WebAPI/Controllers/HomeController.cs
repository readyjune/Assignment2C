﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ClientContext _context; // Add this line

        public HomeController(ILogger<HomeController> logger, ClientContext context) // Add context parameter
        {
            _logger = logger;
            _context = context; // Assign the context
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Dashboard()
        {
            var clients = _context.Clients.ToList();
            return View(clients);
        }
    }
}