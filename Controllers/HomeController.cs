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
            var totalDonations = await _context.DonationRecords.CountAsync();

            ViewBag.OpenRequests = openRequests;
            ViewBag.TotalDonors = totalDonors;
            ViewBag.TotalDonations = totalDonations;

            return View();
        }

        /// <summary>
        /// Displays official verified institutional blood banks and emergency hotlines across Dhaka.
        /// </summary>
        [HttpGet]
        public IActionResult BloodCenters(string? area)
        {
            var centers = new List<Models.ViewModels.BloodCenterViewModel>
            {
                new()
                {
                    Name = "Bangladesh Red Crescent Society (BDRCS) Central Blood Center",
                    Organization = "Bangladesh Red Crescent Society",
                    Area = "Mohammadpur",
                    Address = "7/5, Aurangzeb Road, Mohammadpur, Dhaka",
                    EmergencyHotline = "01811-458537",
                    Landline = "02-41023562",
                    OperatingHours = "24/7 Emergency Service",
                    AvailableServices = "Whole Blood, Red Cell Concentrate, Platelet Concentrate, Fresh Frozen Plasma"
                },
                new()
                {
                    Name = "Quantum Foundation Blood Lab",
                    Organization = "Quantum Foundation",
                    Area = "Shantinagar",
                    Address = "31/V, Shilpacharya Zainul Abedin Sarak, Shantinagar, Dhaka",
                    EmergencyHotline = "01714-974333",
                    Landline = "02-222221441",
                    OperatingHours = "24/7 Emergency Service",
                    AvailableServices = "Whole Blood, Apheresis Platelets, Cryoprecipitate, Automated Screening"
                },
                new()
                {
                    Name = "Holy Family Red Crescent Blood Center",
                    Organization = "Holy Family Red Crescent Medical College Hospital",
                    Area = "Eskaton",
                    Address = "1 Eskaton Garden Road, Ramna, Dhaka",
                    EmergencyHotline = "01811-458536",
                    Landline = "02-8311721",
                    OperatingHours = "24/7 Emergency Service",
                    AvailableServices = "Whole Blood, Cross-matching, Emergency Transfusion"
                },
                new()
                {
                    Name = "Sandhani DMCH Unit",
                    Organization = "Sandhani (Dhaka Medical College)",
                    Area = "Shahbagh",
                    Address = "Dhaka Medical College Hospital Campus, Secretariate Road, Dhaka",
                    EmergencyHotline = "01552-329868",
                    Landline = "02-9668690",
                    OperatingHours = "24/7 Voluntary Donor Coordination",
                    AvailableServices = "Voluntary Whole Blood Donation, Emergency Matchmaking"
                },
                new()
                {
                    Name = "Sandhani Central Unit (BSMMU)",
                    Organization = "Sandhani (Bangabandhu Sheikh Mujib Medical University)",
                    Area = "Shahbagh",
                    Address = "Room 35, Tin-shed Outdoor Building, BSMMU, Shahbagh, Dhaka",
                    EmergencyHotline = "01712-286868",
                    Landline = "02-8621658",
                    OperatingHours = "8:00 AM - 10:00 PM (Emergency Call Center 24/7)",
                    AvailableServices = "Thalassemia Whole Blood Support, Component Separation"
                },
                new()
                {
                    Name = "Badhan Central Blood Donation Network",
                    Organization = "Badhan (University of Dhaka)",
                    Area = "Shahbagh",
                    Address = "Ground Floor, Teacher-Student Centre (TSC), University of Dhaka, Dhaka",
                    EmergencyHotline = "01534-982674",
                    Landline = "02-8629042",
                    OperatingHours = "24/7 Emergency Student Volunteer Network",
                    AvailableServices = "Free Blood Grouping, Rapid Volunteer Donor Mobilization"
                },
                new()
                {
                    Name = "Police Blood Bank",
                    Organization = "Bangladesh Police",
                    Area = "Rajarbagh",
                    Address = "Central Police Hospital, Rajarbagh, Motijheel, Dhaka",
                    EmergencyHotline = "01713-398386",
                    Landline = "02-9333919",
                    OperatingHours = "24/7 Emergency Service",
                    AvailableServices = "Whole Blood, Platelets, Emergency Trauma Reserve"
                },
                new()
                {
                    Name = "BIRDEM General Hospital Blood Transfusion Dept",
                    Organization = "Diabetic Association of Bangladesh",
                    Area = "Shahbagh",
                    Address = "122 Kazi Nazrul Islam Avenue, Shahbagh, Dhaka",
                    EmergencyHotline = "01713-047464",
                    Landline = "02-9661551",
                    OperatingHours = "24/7 Emergency Service",
                    AvailableServices = "Whole Blood, Diabetic Patient Safe Transfusion, Blood Separation"
                }
            };

            if (!string.IsNullOrEmpty(area))
            {
                centers = centers.Where(c => c.Area.Equals(area, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.SelectedArea = area;
            return View(centers);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
