using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RoktoLink.Models.Enums;

namespace RoktoLink.Models
{
    /// <summary>
    /// Represents clinical triage conducted by a BMDC-verified doctor for a donor volunteering for a blood request.
    /// </summary>
    public class MedicalScreening
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RequestId { get; set; }

        [ForeignKey("RequestId")]
        public virtual BloodRequest? Request { get; set; }

        [Required]
        public int DonorId { get; set; }

        [ForeignKey("DonorId")]
        public virtual DonorProfile? Donor { get; set; }

        public int? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual DoctorProfile? Doctor { get; set; }

        [Range(30.0, 200.0, ErrorMessage = "Please enter a valid weight in kg.")]
        [Display(Name = "Weight (kg)")]
        public double WeightKg { get; set; } = 50.0;

        [Range(5.0, 25.0, ErrorMessage = "Please enter a valid hemoglobin level in g/dL.")]
        [Display(Name = "Hemoglobin (g/dL)")]
        public double HemoglobinLevel { get; set; } = 13.0;

        [Range(60, 250)]
        [Display(Name = "Systolic BP")]
        public int SystolicBP { get; set; } = 120;

        [Range(40, 150)]
        [Display(Name = "Diastolic BP")]
        public int DiastolicBP { get; set; } = 80;

        [Display(Name = "No active infection or fever in the last 14 days")]
        public bool HasNoRecentInfection { get; set; } = true;

        [Display(Name = "No major surgery or tattoo in the last 6 months")]
        public bool HasNoRecentTattooOrSurgery { get; set; } = true;

        [Display(Name = "90-day whole blood cooldown confirmed")]
        public bool CooldownConfirmed { get; set; } = true;

        public ScreeningStatus Status { get; set; } = ScreeningStatus.Pending;

        [StringLength(500)]
        [Display(Name = "Doctor Remarks / Clinical Notes")]
        public string? DoctorRemarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ScreenedAt { get; set; }
    }
}
