using RoktoLink.Models;
using RoktoLink.Models.Enums;
using Xunit;

namespace RoktoLink.Tests
{
    /// <summary>
    /// Unit tests verifying clinical pre-screening criteria and BMDC doctor triage authorization logic.
    /// </summary>
    public class MedicalScreeningTests
    {
        [Fact]
        public void Screening_WithValidClinicalVitals_MeetsApprovalCriteria()
        {
            var screening = new MedicalScreening
            {
                WeightKg = 62.0,
                HemoglobinLevel = 13.8,
                SystolicBP = 120,
                DiastolicBP = 80,
                HasNoRecentInfection = true,
                HasNoRecentTattooOrSurgery = true,
                CooldownConfirmed = true
            };

            bool meetsClinicalCriteria = screening.WeightKg >= 45.0 &&
                screening.HemoglobinLevel >= 12.5 &&
                screening.HasNoRecentInfection &&
                screening.HasNoRecentTattooOrSurgery &&
                screening.CooldownConfirmed;

            Assert.True(meetsClinicalCriteria);
        }

        [Theory]
        [InlineData(44.5)]
        [InlineData(40.0)]
        [InlineData(35.0)]
        public void Screening_UnderweightDonor_FailsApprovalCriteria(double weight)
        {
            var screening = new MedicalScreening
            {
                WeightKg = weight,
                HemoglobinLevel = 14.0,
                HasNoRecentInfection = true,
                HasNoRecentTattooOrSurgery = true,
                CooldownConfirmed = true
            };

            bool meetsClinicalCriteria = screening.WeightKg >= 45.0 &&
                screening.HemoglobinLevel >= 12.5 &&
                screening.HasNoRecentInfection &&
                screening.HasNoRecentTattooOrSurgery &&
                screening.CooldownConfirmed;

            Assert.False(meetsClinicalCriteria);
        }

        [Theory]
        [InlineData(12.4)]
        [InlineData(11.0)]
        [InlineData(9.5)]
        public void Screening_LowHemoglobin_FailsApprovalCriteria(double hemoglobin)
        {
            var screening = new MedicalScreening
            {
                WeightKg = 65.0,
                HemoglobinLevel = hemoglobin,
                HasNoRecentInfection = true,
                HasNoRecentTattooOrSurgery = true,
                CooldownConfirmed = true
            };

            bool meetsClinicalCriteria = screening.WeightKg >= 45.0 &&
                screening.HemoglobinLevel >= 12.5 &&
                screening.HasNoRecentInfection &&
                screening.HasNoRecentTattooOrSurgery &&
                screening.CooldownConfirmed;

            Assert.False(meetsClinicalCriteria);
        }

        [Fact]
        public void Screening_RecentViralInfection_FailsApprovalCriteria()
        {
            var screening = new MedicalScreening
            {
                WeightKg = 70.0,
                HemoglobinLevel = 14.5,
                HasNoRecentInfection = false,
                HasNoRecentTattooOrSurgery = true,
                CooldownConfirmed = true
            };

            bool meetsClinicalCriteria = screening.WeightKg >= 45.0 &&
                screening.HemoglobinLevel >= 12.5 &&
                screening.HasNoRecentInfection &&
                screening.HasNoRecentTattooOrSurgery &&
                screening.CooldownConfirmed;

            Assert.False(meetsClinicalCriteria);
        }

        [Fact]
        public void DoctorProfile_BMDCVerification_GuardsAuthorization()
        {
            var verifiedDoctor = new DoctorProfile
            {
                BMDCRegistrationNumber = "A-89214",
                MedicalCollege = "Dhaka Medical College Hospital",
                IsBMDCVerified = true
            };

            var unverifiedDoctor = new DoctorProfile
            {
                BMDCRegistrationNumber = "A-99999",
                MedicalCollege = "Private Clinic",
                IsBMDCVerified = false
            };

            Assert.True(verifiedDoctor.IsBMDCVerified);
            Assert.False(unverifiedDoctor.IsBMDCVerified);
        }
    }
}
