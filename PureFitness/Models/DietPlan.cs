using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class DietPlan
{
    public int DietPlanId { get; set; }

    public int? MemberId { get; set; }

    public int? StaffId { get; set; }

    public DateTime? DateCreated { get; set; }

    public virtual ICollection<DietPlanItem> DietPlanItems { get; set; } = new List<DietPlanItem>();

    public virtual Member? Member { get; set; }

    public virtual Staff? Staff { get; set; }
}
