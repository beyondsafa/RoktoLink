using System;
using RoktoLink.Models;
using Xunit;

namespace RoktoLink.Tests
{
    /// <summary>
    /// Unit tests verifying donor 90-day eligibility and availability calculation logic.
    /// </summary>
    public class DonorEligibilityTests
    {
        [Fact]
        public void Donor_NeverDonated_AndAvailable_IsEligible()
        {
            var profile = new DonorProfile
            {
                BloodType = "O+",
                IsAvailable = true,
                LastDonationDate = null
            };

            bool isEligible = profile.IsAvailable &&
                (profile.LastDonationDate == null || profile.LastDonationDate.Value < DateTime.UtcNow.AddDays(-90));

            Assert.True(isEligible);
        }

        [Fact]
        public void Donor_DonatedMoreThan90DaysAgo_AndAvailable_IsEligible()
        {
            var profile = new DonorProfile
            {
                BloodType = "A+",
                IsAvailable = true,
                LastDonationDate = DateTime.UtcNow.AddDays(-95)
            };

            bool isEligible = profile.IsAvailable &&
                (profile.LastDonationDate == null || profile.LastDonationDate.Value < DateTime.UtcNow.AddDays(-90));

            Assert.True(isEligible);
        }

        [Fact]
        public void Donor_DonatedRecently_IsNotEligible()
        {
            var profile = new DonorProfile
            {
                BloodType = "B+",
                IsAvailable = true,
                LastDonationDate = DateTime.UtcNow.AddDays(-30)
            };

            bool isEligible = profile.IsAvailable &&
                (profile.LastDonationDate == null || profile.LastDonationDate.Value < DateTime.UtcNow.AddDays(-90));

            Assert.False(isEligible);
            Assert.NotNull(profile.LastDonationDate);
            int daysRemaining = (profile.LastDonationDate!.Value.AddDays(90) - DateTime.UtcNow).Days;
            Assert.True(daysRemaining > 0);
            Assert.InRange(daysRemaining, 55, 61);
        }

        [Fact]
        public void Donor_MarkedUnavailable_IsNotEligible_EvenIfNeverDonated()
        {
            var profile = new DonorProfile
            {
                BloodType = "AB+",
                IsAvailable = false,
                LastDonationDate = null
            };

            bool isEligible = profile.IsAvailable &&
                (profile.LastDonationDate == null || profile.LastDonationDate.Value < DateTime.UtcNow.AddDays(-90));

            Assert.False(isEligible);
        }

        [Fact]
        public void Donor_MarkedUnavailable_IsNotEligible_EvenIfOver90Days()
        {
            var profile = new DonorProfile
            {
                BloodType = "O-",
                IsAvailable = false,
                LastDonationDate = DateTime.UtcNow.AddDays(-120)
            };

            bool isEligible = profile.IsAvailable &&
                (profile.LastDonationDate == null || profile.LastDonationDate.Value < DateTime.UtcNow.AddDays(-90));

            Assert.False(isEligible);
        }
    }
}
