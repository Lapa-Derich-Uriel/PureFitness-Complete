using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class DietPlanItem
{
    public int DietPlanItemId { get; set; }

    public int? DietPlanId { get; set; }

    public int CalorieTarget { get; set; }

    public string MealName { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual DietPlan? DietPlan { get; set; }
}
