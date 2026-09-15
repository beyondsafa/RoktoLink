using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Data;
using RoktoLink.Models;
using RoktoLink.Models.Enums;
using RoktoLink.Services;
using System.Security.Claims;

namespace RoktoLink.Controllers
{
    [Authorize]
    public class BloodRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BloodCompatibilityService _bloodCompatibilityService;

        public BloodRequestController(ApplicationDbContext context, BloodCompatibilityService bloodCompatibilityService)
        {
            _context = context;
            _bloodCompatibilityService = bloodCompatibilityService;
        }

        public async Task<IActionResult> Index(string bloodType, string district, UrgencyLevel? urgency)
        {
            var query = _context.BloodRequests
                .Where(r => r.Status == RequestStatus.Open || r.Status == RequestStatus.InProgress)
                .AsQueryable();

            if (!string.IsNullOrEmpty(bloodType))
            {
                query = query.Where(r => r.BloodType == bloodType);
            }

            if (!string.IsNullOrEmpty(district))
            {
                query = query.Where(r => r.District.Contains(district));
            }

            if (urgency.HasValue)
            {
                query = query.Where(r => r.UrgencyLevel == urgency.Value);
            }

            var requests = await query.OrderBy(r => r.Deadline).ToListAsync();
            
            ViewBag.BloodType = bloodType;
            ViewBag.District = district;
            ViewBag.Urgency = urgency;
            
            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.BloodRequests
                .Include(r => r.Coordinator)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var compatibleDonorTypes = _bloodCompatibilityService.GetCompatibleDonorTypes(request.BloodType);

            // Find eligible donors
            var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
            var eligibleDonors = await _context.DonorProfiles
                .Include(d => d.User)
                .Where(d => d.IsAvailable && 
                            compatibleDonorTypes.Contains(d.BloodType) && 
                            (d.LastDonationDate == null || d.LastDonationDate < ninetyDaysAgo))
                .ToListAsync();

            ViewBag.EligibleDonors = eligibleDonors;

            var screenings = await _context.MedicalScreenings
                .Include(s => s.Doctor).ThenInclude(doc => doc!.User)
                .Where(s => s.RequestId == id)
                .ToListAsync();

            ViewBag.MedicalScreenings = screenings;

            return View(request);
        }

        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.CoordinatorProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (profile != null && !profile.IsApproved)
            {
                TempData["Error"] = "Your Hospital Coordinator account is pending administrative accreditation and approval. You cannot publish requisitions yet.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BloodRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _context.CoordinatorProfiles.FirstOrDefaultAsync(c => c.UserId == userId);
            if (profile != null && !profile.IsApproved)
            {
                TempData["Error"] = "Your Hospital Coordinator account is pending administrative accreditation and approval. You cannot publish requisitions yet.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                model.CoordinatorId = userId;
                model.Status = RequestStatus.Open;
                model.CreatedAt = DateTime.UtcNow;

                _context.BloodRequests.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Blood request created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status != RequestStatus.Open)
            {
                TempData["Error"] = "Only open requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BloodRequest model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingRequest = await _context.BloodRequests.FindAsync(id);
                if (existingRequest == null || existingRequest.Status != RequestStatus.Open)
                {
                    TempData["Error"] = "Cannot edit this request.";
                    return RedirectToAction(nameof(Index));
                }

                existingRequest.PatientName = model.PatientName;
                existingRequest.BloodType = model.BloodType;
                existingRequest.UnitsNeeded = model.UnitsNeeded;
                existingRequest.Hospital = model.Hospital;
                existingRequest.District = model.District;
                existingRequest.UrgencyLevel = model.UrgencyLevel;
                existingRequest.Deadline = model.Deadline;
                existingRequest.Notes = model.Notes;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Request updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fulfill(int id, int donorProfileId, int unitsGiven)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            var donor = await _context.DonorProfiles.FindAsync(donorProfileId);

            if (request == null || donor == null) return NotFound();

            var record = new DonationRecord
            {
                DonorId = donor.Id,
                RequestId = request.Id,
                UnitsGiven = unitsGiven,
                DonationDate = DateTime.UtcNow,
                ConfirmedByCoordinator = true
            };

            _context.DonationRecords.Add(record);
            
            donor.TotalDonations++;
            donor.LastDonationDate = DateTime.UtcNow;

            request.Status = RequestStatus.Fulfilled;
            request.FulfilledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Request marked as fulfilled successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Coordinator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = RequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Request cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
