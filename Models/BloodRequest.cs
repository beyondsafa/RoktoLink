using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RoktoLink.Models.Enums;

namespace RoktoLink.Models
{
    public class BloodRequest
    {
        public int Id { get; set; }

        [Required]
        public string CoordinatorId { get; set; } = string.Empty;

        [ForeignKey("CoordinatorId")]
        public ApplicationUser? Coordinator { get; set; }

        [Required]
        [StringLength(100)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [StringLength(5)]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        [Range(1, 50)]
        public int UnitsNeeded { get; set; }

        [Required]
        [StringLength(150)]
        public string Hospital { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string District { get; set; } = string.Empty;

        public UrgencyLevel UrgencyLevel { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Open;

        [Required]
        public DateTime Deadline { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? FulfilledAt { get; set; }
    }
}
