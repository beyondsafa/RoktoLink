using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Models;

namespace RoktoLink.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DonorProfile> DonorProfiles { get; set; } = null!;
        public DbSet<BloodRequest> BloodRequests { get; set; } = null!;
        public DbSet<DonationRecord> DonationRecords { get; set; } = null!;
        public DbSet<DoctorProfile> DoctorProfiles { get; set; } = null!;
        public DbSet<CoordinatorProfile> CoordinatorProfiles { get; set; } = null!;
        public DbSet<MedicalScreening> MedicalScreenings { get; set; } = null!;
    }
}
