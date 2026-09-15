using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoktoLink.Models
{
    /// <summary>
    /// Represents an institutional blood bank or hospital coordinator profile with administrative accreditation status.
    /// </summary>
    public class CoordinatorProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Hospital or institutional affiliation is required.")]
        [StringLength(150, ErrorMessage = "Hospital name cannot exceed 150 characters.")]
        [Display(Name = "Hospital / Institution Name")]
        public string HospitalName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department or clinical designation is required.")]
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
        [Display(Name = "Department / Designation")]
        public string Department { get; set; } = "Blood Transfusion Unit";

        [StringLength(50)]
        [Display(Name = "Institutional Staff ID / Accreditation No.")]
        public string? StaffId { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime? ApprovedAt { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }
}
