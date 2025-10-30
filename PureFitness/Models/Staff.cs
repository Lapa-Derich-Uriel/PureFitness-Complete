using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string Name { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public int? UserId { get; set; }

    public string? AssignedTask { get; set; }

    public virtual ICollection<DietPlan> DietPlans { get; set; } = new List<DietPlan>();

    public virtual ICollection<EquipmentSchedule> EquipmentSchedules { get; set; } = new List<EquipmentSchedule>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }

    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
}
