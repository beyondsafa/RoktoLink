using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoktoLink.Models
{
    public class DonationRecord
    {
        public int Id { get; set; }

        public int DonorId { get; set; }

        [ForeignKey("DonorId")]
        public DonorProfile? Donor { get; set; }

        public int RequestId { get; set; }

        [ForeignKey("RequestId")]
        public BloodRequest? Request { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, 10)]
        public int UnitsGiven { get; set; }

        public bool ConfirmedByCoordinator { get; set; } = false;
    }
}
