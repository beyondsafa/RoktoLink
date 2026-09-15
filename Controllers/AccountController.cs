using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoktoLink.Models;
using System.ComponentModel.DataAnnotations;

namespace RoktoLink.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Data.ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Role == "Doctor" && string.IsNullOrWhiteSpace(model.BMDCRegistrationNumber))
            {
                ModelState.AddModelError("BMDCRegistrationNumber", "BMDC Registration Number is required for volunteer doctors.");
            }

            if (model.Role == "Coordinator" && string.IsNullOrWhiteSpace(model.HospitalName))
            {
                ModelState.AddModelError("HospitalName", "Hospital or institution name is required for coordinators.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    District = model.District,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var assignedRole = model.Role is "Doctor" or "Coordinator" ? model.Role : "Donor";
                    await _userManager.AddToRoleAsync(user, assignedRole);

                    if (assignedRole == "Doctor")
                    {
                        var doctorProfile = new DoctorProfile
                        {
                            UserId = user.Id,
                            BMDCRegistrationNumber = model.BMDCRegistrationNumber?.Trim() ?? string.Empty,
                            MedicalCollege = model.MedicalCollege?.Trim() ?? "General Practice",
                            Designation = model.Designation?.Trim() ?? "Medical Officer",
                            IsBMDCVerified = false
                        };
                        _context.DoctorProfiles.Add(doctorProfile);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Doctor registration submitted successfully. Your account is awaiting BMDC verification by administration.";
                    }
                    else if (assignedRole == "Coordinator")
                    {
                        var coordinatorProfile = new CoordinatorProfile
                        {
                            UserId = user.Id,
                            HospitalName = model.HospitalName?.Trim() ?? string.Empty,
                            Department = model.HospitalDesignation?.Trim() ?? "Blood Transfusion Unit",
                            StaffId = model.HospitalStaffId?.Trim(),
                            IsApproved = false
                        };
                        _context.CoordinatorProfiles.Add(coordinatorProfile);
                        await _context.SaveChangesAsync();

                        TempData["Success"] = "Hospital Coordinator registration submitted successfully. Your account is awaiting administrative approval.";
                    }
                    else
                    {
                        TempData["Success"] = "Registration successful. Please log in.";
                    }

                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Logged in successfully.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Index", "Home");
        }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "District / Area")]
        public string District { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Account Role")]
        public string Role { get; set; } = "Donor";

        [Display(Name = "BMDC Registration Number (Doctors only)")]
        [StringLength(50)]
        public string? BMDCRegistrationNumber { get; set; }

        [Display(Name = "Medical College / Affiliation (Doctors only)")]
        [StringLength(150)]
        public string? MedicalCollege { get; set; }

        [Display(Name = "Designation / Specialization (Doctors only)")]
        [StringLength(100)]
        public string? Designation { get; set; }

        [Display(Name = "Hospital / Institution Name (Coordinators only)")]
        [StringLength(150)]
        public string? HospitalName { get; set; }

        [Display(Name = "Department / Designation (Coordinators only)")]
        [StringLength(100)]
        public string? HospitalDesignation { get; set; }

        [Display(Name = "Staff ID / Accreditation No. (Coordinators only)")]
        [StringLength(50)]
        public string? HospitalStaffId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
