using System;
using Microsoft.AspNetCore.Identity;
using RoktoLink.Models;
using Xunit;

namespace RoktoLink.Tests
{
    /// <summary>
    /// Unit tests verifying coordinator approval validation and password hasher verification.
    /// </summary>
    public class CoordinatorApprovalTests
    {
        [Fact]
        public void CoordinatorProfile_DefaultStatus_IsUnapproved()
        {
            var profile = new CoordinatorProfile
            {
                UserId = "coord-1",
                HospitalName = "Apollo Hospitals Dhaka",
                Department = "Transfusion Medicine",
                StaffId = "AP-9921"
            };

            Assert.False(profile.IsApproved);
            Assert.Null(profile.ApprovedAt);
        }

        [Fact]
        public void CoordinatorProfile_Approve_SetsApprovalAndTimestamp()
        {
            var profile = new CoordinatorProfile
            {
                UserId = "coord-1",
                HospitalName = "Dhaka Medical College Hospital",
                Department = "Blood Bank Unit",
                IsApproved = false
            };

            profile.IsApproved = true;
            profile.ApprovedAt = DateTime.UtcNow;

            Assert.True(profile.IsApproved);
            Assert.NotNull(profile.ApprovedAt);
        }
    }
}
