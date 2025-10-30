using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Payment
{
    public int ReceiptId { get; set; }

    public int? StaffId { get; set; }

    public int? MemberId { get; set; }

    public DateOnly? TransactionDate { get; set; }

    public string? PaymentType { get; set; }

    public int? InventoryId { get; set; }

    public string? Items { get; set; }

    public decimal? Cost { get; set; }

    public string? AccessType { get; set; }

    public decimal? TotalCost { get; set; }

    public decimal? Amount { get; set; }

    public decimal? Change { get; set; }

    public string? WalkInName { get; set; }

    public int? ItemQuantity { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Inventory? Inventory { get; set; }

    public virtual Member? Member { get; set; }

    public virtual Staff? Staff { get; set; }
}
