using System;
using System.Collections.Generic;
using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class DashboardViewModel
    {
        // 🧍 Common Info
        public string Role { get; set; } = "";
        public string Username { get; set; } = "";

        // 🧠 For Member Dashboard
        public Member? member { get; set; }

        // 🧰 For Staff Dashboard
        public string? Name { get; set; }
        public string? AssignedTask { get; set; }
        public List<EquipmentSchedule> EquipmentSchedules { get; set; } = new();

        // 🧾 For Admin Dashboard
        public int TotalActiveMembers { get; set; }
        public int NewMembershipsThisWeek { get; set; }
        public int ActiveEquipments { get; set; }
        public int UnderMaintenance { get; set; }
        public int LowStockItems { get; set; }

        // 🏋️ Misc
        public string WorkoutGoal { get; set; } = "No goal set.";
        public List<string> Notifications { get; set; } = new List<string>
        {
            "Remember to hydrate!",
            "Don’t skip leg day!",
            "Stay consistent with your program."
        };
    }
}
