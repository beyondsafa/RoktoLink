using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Data;
using RoktoLink.Models;
using System.Diagnostics;
using RoktoLink.Models.Enums;

namespace RoktoLink.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var openRequests = await _context.BloodRequests.CountAsync(r => r.Status == RequestStatus.Open);
            var totalDonors = await _context.DonorProfiles.CountAsync();

            ViewBag.OpenRequests = openRequests;
            ViewBag.TotalDonors = totalDonors;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
