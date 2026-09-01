using RoktoLink.Services;
using Xunit;

namespace RoktoLink.Tests
{
    /// <summary>
    /// Unit tests verifying blood type compatibility rules.
    /// </summary>
    public class BloodCompatibilityServiceTests
    {
        private readonly BloodCompatibilityService _service;

        public BloodCompatibilityServiceTests()
        {
            _service = new BloodCompatibilityService();
        }

        [Fact]
        public void GetCompatibleDonorTypes_ABPositive_ReturnsAllEightTypes()
        {
            var result = _service.GetCompatibleDonorTypes("AB+");

            Assert.Equal(8, result.Count);
            Assert.Contains("A+", result);
            Assert.Contains("A-", result);
            Assert.Contains("B+", result);
            Assert.Contains("B-", result);
            Assert.Contains("AB+", result);
            Assert.Contains("AB-", result);
            Assert.Contains("O+", result);
            Assert.Contains("O-", result);
        }

        [Fact]
        public void GetCompatibleDonorTypes_ONegative_ReturnsOnlyONegative()
        {
            var result = _service.GetCompatibleDonorTypes("O-");

            Assert.Single(result);
            Assert.Contains("O-", result);
        }

        [Theory]
        [InlineData("A+", new[] { "A+", "A-", "O+", "O-" })]
        [InlineData("A-", new[] { "A-", "O-" })]
        [InlineData("B+", new[] { "B+", "B-", "O+", "O-" })]
        [InlineData("B-", new[] { "B-", "O-" })]
        [InlineData("AB-", new[] { "AB-", "A-", "B-", "O-" })]
        [InlineData("O+", new[] { "O+", "O-" })]
        public void GetCompatibleDonorTypes_StandardTypes_ReturnsCorrectCompatibleTypes(string recipientType, string[] expectedDonors)
        {
            var result = _service.GetCompatibleDonorTypes(recipientType);

            Assert.Equal(expectedDonors.Length, result.Count);
            foreach (var expected in expectedDonors)
            {
                Assert.Contains(expected, result);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("Invalid")]
        [InlineData("C+")]
        [InlineData("XYZ")]
        public void GetCompatibleDonorTypes_InvalidTypes_ReturnsEmptyList(string invalidType)
        {
            var result = _service.GetCompatibleDonorTypes(invalidType);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
