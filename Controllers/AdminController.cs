using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Data;
using RoktoLink.Models;
using RoktoLink.Models.Enums;
using System.Text.Json;

namespace RoktoLink.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            ViewBag.TotalDonors = await _context.DonorProfiles.CountAsync();
            ViewBag.OpenRequests = await _context.BloodRequests.CountAsync(r => r.Status == RequestStatus.Open);
            ViewBag.FulfilledThisMonth = await _context.BloodRequests.CountAsync(r => r.Status == RequestStatus.Fulfilled && r.FulfilledAt.HasValue && r.FulfilledAt.Value.Month == currentMonth && r.FulfilledAt.Value.Year == currentYear);
            ViewBag.CriticalOpen = await _context.BloodRequests.CountAsync(r => r.Status == RequestStatus.Open && r.UrgencyLevel == UrgencyLevel.Critical);

            return View();
        }

        public async Task<IActionResult> Users(int page = 1)
        {
            int pageSize = 10;
            var totalUsers = await _userManager.Users.CountAsync();
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            ViewBag.DoctorProfiles = await _context.DoctorProfiles.Include(d => d.User).ToListAsync();
            ViewBag.CoordinatorProfiles = await _context.CoordinatorProfiles.Include(c => c.User).ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            TempData["Success"] = $"Role for {user.FullName} changed to {newRole}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDoctor(int doctorProfileId)
        {
            var doctor = await _context.DoctorProfiles.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == doctorProfileId);
            if (doctor == null) return NotFound();

            doctor.IsBMDCVerified = true;
            doctor.VerifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"BMDC Registration for Dr. {doctor.User?.FullName} (Reg: {doctor.BMDCRegistrationNumber}) verified successfully.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCoordinator(int coordinatorProfileId)
        {
            var coordinator = await _context.CoordinatorProfiles.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == coordinatorProfileId);
            if (coordinator == null) return NotFound();

            coordinator.IsApproved = true;
            coordinator.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Hospital coordinator account for {coordinator.User?.FullName} ({coordinator.HospitalName}) approved successfully.";
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate, string bloodType)
        {
            // 1. Donation Activity (Last 12 months)
            var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);
            twelveMonthsAgo = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1);
            
            var monthlyData = await _context.DonationRecords
                .Where(d => d.DonationDate >= twelveMonthsAgo)
                .GroupBy(d => new { d.DonationDate.Year, d.DonationDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var months = new List<string>();
            var donationCounts = new List<int>();
            
            for (int i = 0; i < 12; i++)
            {
                var d = twelveMonthsAgo.AddMonths(i);
                months.Add(d.ToString("MMM yyyy"));
                
                var data = monthlyData.FirstOrDefault(m => m.Year == d.Year && m.Month == d.Month);
                donationCounts.Add(data?.Count ?? 0);
            }

            ViewBag.MonthsJson = JsonSerializer.Serialize(months);
            ViewBag.DonationCountsJson = JsonSerializer.Serialize(donationCounts);

            // 2. Blood Type Availability
            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var eligibleDonors = await _context.DonorProfiles
                .Where(d => d.IsAvailable && (d.LastDonationDate == null || d.LastDonationDate < ninetyDaysAgo))
                .GroupBy(d => d.BloodType)
                .Select(g => new { BloodType = g.Key, Count = g.Count() })
                .ToListAsync();

            var allTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            var bloodTypeLabels = allTypes.ToList();
            var bloodTypeCounts = allTypes.Select(bt => eligibleDonors.FirstOrDefault(d => d.BloodType == bt)?.Count ?? 0).ToList();

            ViewBag.BloodTypeLabelsJson = JsonSerializer.Serialize(bloodTypeLabels);
            ViewBag.BloodTypeCountsJson = JsonSerializer.Serialize(bloodTypeCounts);

            // 3. Fulfillment Log
            var query = _context.BloodRequests.Include(r => r.Coordinator).AsQueryable();

            if (startDate.HasValue) query = query.Where(r => r.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(r => r.CreatedAt <= endDate.Value.AddDays(1));
            if (!string.IsNullOrEmpty(bloodType)) query = query.Where(r => r.BloodType == bloodType);

            var requestsLog = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var fulfilledReqs = requestsLog.Where(r => r.Status == RequestStatus.Fulfilled && r.FulfilledAt.HasValue).ToList();
            if (fulfilledReqs.Any())
            {
                var avgHours = fulfilledReqs.Average(r => (r.FulfilledAt!.Value - r.CreatedAt).TotalHours);
                ViewBag.AvgFulfillmentHours = Math.Round(avgHours, 1);
            }
            else
            {
                ViewBag.AvgFulfillmentHours = 0;
            }

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.FilterBloodType = bloodType;

            return View(requestsLog);
        }
    }
}
