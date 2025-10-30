using System.ComponentModel.DataAnnotations;
using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;   // "Member", "Staff", "Admin"

        // role-specific
        public Member? Member { get; set; }
        public Staff? Staff { get; set; }
    }
}
