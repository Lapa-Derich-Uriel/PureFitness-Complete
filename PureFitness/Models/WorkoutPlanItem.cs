using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class WorkoutPlanItem
{
    public int WorkoutPlanItemId { get; set; }

    public int? WorkoutPlanId { get; set; }

    public int? EquipmentId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public int Sets { get; set; }

    public int Repetitions { get; set; }

    public string? Notes { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual WorkoutPlan? WorkoutPlan { get; set; }
}
