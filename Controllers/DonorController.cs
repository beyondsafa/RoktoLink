using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Data;
using RoktoLink.Models;
using RoktoLink.Models.Enums;
using RoktoLink.Services;
using System.Security.Claims;

namespace RoktoLink.Controllers
{
    [Authorize(Roles = "Donor")]
    public class DonorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BloodCompatibilityService _bloodCompatibilityService;

        public DonorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, BloodCompatibilityService bloodCompatibilityService)
        {
            _context = context;
            _userManager = userManager;
            _bloodCompatibilityService = bloodCompatibilityService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var profile = await _context.DonorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            
            if (profile == null)
            {
                TempData["Error"] = "Please complete your donor profile first.";
                return RedirectToAction(nameof(Profile));
            }

            var openRequests = new List<BloodRequest>();
            if (!string.IsNullOrEmpty(profile.BloodType))
            {
                // Find compatible requests (if a request needs blood type X, what donors can give? wait.
                // The service GetCompatibleDonorTypes(recipientBloodType) returns a list of compatible DONOR blood types for a given RECIPIENT.
                // But from the donor's perspective, they want to see requests they can donate TO.
                // i.e., donor.BloodType is in GetCompatibleDonorTypes(request.BloodType).
                // Or simply: Get all open requests, and filter where donor.BloodType is in compatible types.
                var allOpenRequests = await _context.BloodRequests
                    .Include(r => r.Coordinator)
                    .Where(r => r.Status == RequestStatus.Open)
                    .ToListAsync();

                openRequests = allOpenRequests
                    .Where(r => _bloodCompatibilityService.GetCompatibleDonorTypes(r.BloodType).Contains(profile.BloodType))
                    .ToList();
            }

            var donationHistory = await _context.DonationRecords
                .Include(d => d.Request)
                .Where(d => d.DonorId == profile.Id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewBag.Profile = profile;
            ViewBag.DonationHistory = donationHistory;

            return View(openRequests);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            var user = await _userManager.FindByIdAsync(userId);
            var profile = await _context.DonorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new DonorProfile
                {
                    UserId = userId,
                    District = user?.District ?? string.Empty
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(DonorProfile model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();
            
            if (model.UserId != userId)
            {
                return Unauthorized();
            }

            var existingProfile = await _context.DonorProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingProfile == null)
            {
                model.TotalDonations = 0;
                _context.DonorProfiles.Add(model);
            }
            else
            {
                existingProfile.BloodType = model.BloodType;
                existingProfile.District = model.District;
                existingProfile.IsAvailable = model.IsAvailable;
                existingProfile.LastDonationDate = model.LastDonationDate;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int requestId)
        {
            var request = await _context.BloodRequests.FindAsync(requestId);
            if (request == null || request.Status != RequestStatus.Open)
            {
                TempData["Error"] = "Request not found or not open.";
                return RedirectToAction(nameof(Dashboard));
            }

            request.Status = RequestStatus.InProgress;
            await _context.SaveChangesAsync();

            TempData["Success"] = "You have confirmed your availability for this request! The coordinator has been notified.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
