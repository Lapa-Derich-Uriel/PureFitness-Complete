using System.ComponentModel.DataAnnotations;
using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            User = new User(); // Initialize User to avoid null reference
        }

        public User? User { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
