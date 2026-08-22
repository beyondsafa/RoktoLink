using Microsoft.AspNetCore.Identity;
using RoktoLink.Models;
using RoktoLink.Models.Enums;

namespace RoktoLink.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            string[] roles = { "Admin", "Coordinator", "Donor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (await userManager.FindByEmailAsync("admin@roktolink.local") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@roktolink.local",
                    Email = "admin@roktolink.local",
                    FullName = "System Admin",
                    District = "Dhaka",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            if (await userManager.FindByEmailAsync("coordinator@roktolink.local") == null)
            {
                var coordinator = new ApplicationUser
                {
                    UserName = "coordinator@roktolink.local",
                    Email = "coordinator@roktolink.local",
                    FullName = "Hospital Coordinator",
                    District = "Dhaka",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(coordinator, "Coord@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinator, "Coordinator");
                }
            }

            ApplicationUser donor1 = null!;
            if (await userManager.FindByEmailAsync("donor@roktolink.local") == null)
            {
                donor1 = new ApplicationUser
                {
                    UserName = "donor@roktolink.local",
                    Email = "donor@roktolink.local",
                    FullName = "Hero Donor",
                    District = "Dhaka",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(donor1, "Donor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(donor1, "Donor");
                }
            }
            else
            {
                donor1 = (await userManager.FindByEmailAsync("donor@roktolink.local"))!;
            }

            ApplicationUser donor2 = null!;
            if (await userManager.FindByEmailAsync("donor2@roktolink.local") == null)
            {
                donor2 = new ApplicationUser
                {
                    UserName = "donor2@roktolink.local",
                    Email = "donor2@roktolink.local",
                    FullName = "Another Donor",
                    District = "Chittagong",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(donor2, "Donor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(donor2, "Donor");
                }
            }
            else
            {
                donor2 = (await userManager.FindByEmailAsync("donor2@roktolink.local"))!;
            }

            if (!context.DonorProfiles.Any())
            {
                context.DonorProfiles.AddRange(
                    new DonorProfile { UserId = donor1.Id, BloodType = "O+", District = "Dhaka", IsAvailable = true, TotalDonations = 1, LastDonationDate = DateTime.UtcNow.AddDays(-100) },
                    new DonorProfile { UserId = donor2.Id, BloodType = "A+", District = "Chittagong", IsAvailable = true, TotalDonations = 0, LastDonationDate = null }
                );
                await context.SaveChangesAsync();
            }

            var coordinatorUser = await userManager.FindByEmailAsync("coordinator@roktolink.local");

            if (!context.BloodRequests.Any() && coordinatorUser != null)
            {
                context.BloodRequests.AddRange(
                    new BloodRequest { CoordinatorId = coordinatorUser.Id, PatientName = "John Doe", BloodType = "O+", UnitsNeeded = 2, Hospital = "DMCH", District = "Dhaka", UrgencyLevel = UrgencyLevel.Critical, Status = RequestStatus.Open, Deadline = DateTime.UtcNow.AddDays(1), Notes = "Urgent need." },
                    new BloodRequest { CoordinatorId = coordinatorUser.Id, PatientName = "Jane Doe", BloodType = "A+", UnitsNeeded = 1, Hospital = "Square Hospital", District = "Dhaka", UrgencyLevel = UrgencyLevel.Urgent, Status = RequestStatus.Open, Deadline = DateTime.UtcNow.AddDays(2), Notes = "Need A+ blood." },
                    new BloodRequest { CoordinatorId = coordinatorUser.Id, PatientName = "Bob Smith", BloodType = "B+", UnitsNeeded = 3, Hospital = "Apollo Hospital", District = "Dhaka", UrgencyLevel = UrgencyLevel.Normal, Status = RequestStatus.InProgress, Deadline = DateTime.UtcNow.AddDays(5), Notes = "Scheduled surgery." }
                );
                await context.SaveChangesAsync();
            }

            if (!context.DonationRecords.Any())
            {
                var req = context.BloodRequests.FirstOrDefault();
                var dp = context.DonorProfiles.FirstOrDefault();
                if (req != null && dp != null)
                {
                    context.DonationRecords.AddRange(
                        new DonationRecord { DonorId = dp.Id, RequestId = req.Id, UnitsGiven = 1, ConfirmedByCoordinator = true },
                        new DonationRecord { DonorId = dp.Id, RequestId = req.Id, UnitsGiven = 1, ConfirmedByCoordinator = false }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
