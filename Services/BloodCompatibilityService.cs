namespace RoktoLink.Services
{
    public class BloodCompatibilityService
    {
        /// <summary>
        /// Gets a list of compatible donor blood types for a given recipient blood type.
        /// </summary>
        /// <param name="recipientBloodType">The blood type of the recipient.</param>
        /// <returns>A list of compatible blood types.</returns>
        public List<string> GetCompatibleDonorTypes(string recipientBloodType)
        {
            return recipientBloodType switch
            {
                "A+" => new List<string> { "A+", "A-", "O+", "O-" },
                "A-" => new List<string> { "A-", "O-" },
                "B+" => new List<string> { "B+", "B-", "O+", "O-" },
                "B-" => new List<string> { "B-", "O-" },
                "AB+" => new List<string> { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" },
                "AB-" => new List<string> { "AB-", "A-", "B-", "O-" },
                "O+" => new List<string> { "O+", "O-" },
                "O-" => new List<string> { "O-" },
                _ => new List<string>()
            };
        }
    }
}
