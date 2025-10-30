using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class UserViewModel
    {
        public User? user { get; set; }

        // UI-only flags
        public bool IsTemporaryPassword { get; set; } = false;

        // Plaintext temp password (only for display)
        public string? TempPassword { get; set; }
    }
}
