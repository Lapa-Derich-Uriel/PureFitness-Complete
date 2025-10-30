using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class EquipmentItem
{
    public int EquipmentItemId { get; set; }

    public string? EquipmentItemName { get; set; }

    public string? EquipmentItemStatus { get; set; }

    public int? EquipmentId { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual ICollection<EquipmentSchedule> EquipmentSchedules { get; set; } = new List<EquipmentSchedule>();
}
