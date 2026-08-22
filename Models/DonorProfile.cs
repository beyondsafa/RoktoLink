using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoktoLink.Models
{
    public class DonorProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [StringLength(5)]
        public string BloodType { get; set; } = string.Empty;

        public DateTime? LastDonationDate { get; set; }

        [Required]
        [StringLength(50)]
        public string District { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        public int TotalDonations { get; set; } = 0;
    }
}
