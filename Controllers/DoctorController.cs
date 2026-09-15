using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Data;
using RoktoLink.Models;
using RoktoLink.Models.Enums;

namespace RoktoLink.Controllers
{
    /// <summary>
    /// Handles medical pre-screening triage, clinical evaluation, and donor contact clearance authorization by BMDC-registered doctors.
    /// </summary>
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> TriageQueue()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);

            ViewBag.DoctorProfile = doctorProfile;

            var pendingScreenings = await _context.MedicalScreenings
                .Include(s => s.Donor).ThenInclude(d => d!.User)
                .Include(s => s.Request).ThenInclude(r => r!.Coordinator)
                .Include(s => s.Doctor).ThenInclude(doc => doc!.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(pendingScreenings);
        }

        [HttpGet]
        public async Task<IActionResult> Screen(int id)
        {
            var screening = await _context.MedicalScreenings
                .Include(s => s.Donor).ThenInclude(d => d!.User)
                .Include(s => s.Request)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            ViewBag.DoctorProfile = doctorProfile;

            return View(screening);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Screen(int id, MedicalScreening model, string decision)
        {
            var screening = await _context.MedicalScreenings
                .Include(s => s.Donor)
                .Include(s => s.Request)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
            {
                TempData["Error"] = "Doctor profile not found.";
                return RedirectToAction(nameof(TriageQueue));
            }

            if (!doctorProfile.IsBMDCVerified)
            {
                TempData["Error"] = "Your BMDC Registration is currently pending admin verification. You cannot authorize clearances yet.";
                return RedirectToAction(nameof(TriageQueue));
            }

            screening.WeightKg = model.WeightKg;
            screening.HemoglobinLevel = model.HemoglobinLevel;
            screening.SystolicBP = model.SystolicBP;
            screening.DiastolicBP = model.DiastolicBP;
            screening.HasNoRecentInfection = model.HasNoRecentInfection;
            screening.HasNoRecentTattooOrSurgery = model.HasNoRecentTattooOrSurgery;
            screening.CooldownConfirmed = model.CooldownConfirmed;
            screening.DoctorRemarks = model.DoctorRemarks;
            screening.DoctorId = doctorProfile.Id;
            screening.ScreenedAt = DateTime.UtcNow;

            if (decision == "Approve")
            {
                if (model.WeightKg < 45.0 || model.HemoglobinLevel < 12.5 || !model.HasNoRecentInfection || !model.HasNoRecentTattooOrSurgery || !model.CooldownConfirmed)
                {
                    TempData["Error"] = "Cannot approve donor: Clinical criteria not met (Weight >= 45kg, Hb >= 12.5 g/dL, no recent infection).";
                    return View(screening);
                }

                screening.Status = ScreeningStatus.Approved;
                if (screening.Request != null && screening.Request.Status == RequestStatus.Open)
                {
                    screening.Request.Status = RequestStatus.InProgress;
                }

                TempData["Success"] = $"Donor medically cleared and authorized by Dr. {doctorProfile.User?.FullName ?? "Volunteer Doctor"} (BMDC: {doctorProfile.BMDCRegistrationNumber}). Contact details unlocked for the hospital coordinator.";
            }
            else
            {
                screening.Status = ScreeningStatus.Rejected;
                TempData["Success"] = "Donor screening marked as rejected due to clinical contraindications.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TriageQueue));
        }
    }
}
