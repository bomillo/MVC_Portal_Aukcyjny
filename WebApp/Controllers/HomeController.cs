using Microsoft.AspNetCore.Mvc;
using PortalAukcyjny.Models;
using System.Diagnostics;
using WebApp.Context;

namespace PortalAukcyjny.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PortalAukcyjnyContext _context;

        public HomeController(ILogger<HomeController> logger, PortalAukcyjnyContext context)
        {
            _logger = logger;
            _context = context;
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
    }
}