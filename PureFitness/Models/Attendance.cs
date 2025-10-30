using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int? MemberId { get; set; }

    public DateOnly? AttendanceDate { get; set; }

    public TimeOnly? TimeIn { get; set; }

    public TimeOnly? TimeOut { get; set; }

    public string? AttendanceStatus { get; set; }

    public string? Activity { get; set; }

    public int? ReceiptId { get; set; }

    public int? TimeSpent { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Payment? Receipt { get; set; }
}
