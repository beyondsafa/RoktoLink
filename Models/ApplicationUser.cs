using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RoktoLink.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string District { get; set; } = string.Empty;
    }
}
