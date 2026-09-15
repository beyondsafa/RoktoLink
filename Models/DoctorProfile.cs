using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoktoLink.Models
{
    /// <summary>
    /// Represents a verified or applicant volunteer medical doctor with BMDC registration.
    /// </summary>
    public class DoctorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "BMDC Registration Number is required.")]
        [StringLength(50, ErrorMessage = "BMDC Number cannot exceed 50 characters.")]
        [Display(Name = "BMDC Registration Number")]
        public string BMDCRegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Medical college or hospital affiliation is required.")]
        [StringLength(150)]
        [Display(Name = "Medical College / Affiliated Hospital")]
        public string MedicalCollege { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinical designation is required.")]
        [StringLength(100)]
        [Display(Name = "Designation / Specialization")]
        public string Designation { get; set; } = "Medical Officer";

        public bool IsBMDCVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
