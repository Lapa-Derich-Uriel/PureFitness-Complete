using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string? EquipmentName { get; set; }

    public string? EquipmentCategory { get; set; }

    public int? Quantity { get; set; }

    public int? Capacity { get; set; }

    public string? Program { get; set; }

    public virtual ICollection<EquipmentItem> EquipmentItems { get; set; } = new List<EquipmentItem>();

    public virtual ICollection<EquipmentSchedule> EquipmentSchedules { get; set; } = new List<EquipmentSchedule>();

    public virtual ICollection<WorkoutPlanItem> WorkoutPlanItems { get; set; } = new List<WorkoutPlanItem>();
}
