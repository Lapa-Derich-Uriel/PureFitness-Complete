using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class WorkoutPlan
{
    public int WorkoutPlanId { get; set; }

    public int? MemberId { get; set; }

    public int? StaffId { get; set; }

    public DateTime? DateCreated { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual ICollection<WorkoutPlanItem> WorkoutPlanItems { get; set; } = new List<WorkoutPlanItem>();
}
