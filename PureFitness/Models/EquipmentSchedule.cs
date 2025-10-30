using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class EquipmentSchedule
{
    public int ScheduleId { get; set; }

    public int? EquipmentId { get; set; }

    public int? EquipmentItemId { get; set; }

    public DateOnly? MaintenanceDate { get; set; }

    public int? StaffId { get; set; }

    public string? Remarks { get; set; }

    public string? Frequency { get; set; }

    public DateOnly? NextMaintenanceDate { get; set; }

    public virtual Equipment? Equipment { get; set; }

    public virtual EquipmentItem? EquipmentItem { get; set; }

    public virtual ICollection<EquipmentLog> EquipmentLogs { get; set; } = new List<EquipmentLog>();

    public virtual Staff? Staff { get; set; }
}
