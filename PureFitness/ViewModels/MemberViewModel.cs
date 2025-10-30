using System.ComponentModel.DataAnnotations.Schema;
using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class MemberViewModel
    {
        public Member? member { get; set; }

        public User? user { get; set; }

        // Add these to capture selected units from the UI
        public string? HeightUnit { get; set; }
        public string? WeightUnit { get; set; }

        // ✅ Temporary password to show once after member creation
        public string? TempPassword { get; set; }

        [NotMapped] // optional if this VM maps to an EF entity; usually VM is not persisted anyway
        public string? PlainPasswordForDisplay { get; set; }
    }
}
