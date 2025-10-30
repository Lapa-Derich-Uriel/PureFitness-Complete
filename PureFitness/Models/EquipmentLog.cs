using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class EquipmentLog
{
    public int LogId { get; set; }

    public int? ScheduleId { get; set; }

    public DateOnly? DateCreated { get; set; }

    public virtual EquipmentSchedule? Schedule { get; set; }
}
