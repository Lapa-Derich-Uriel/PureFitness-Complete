using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int? ProductId { get; set; }

    public int? ProductQuantity { get; set; }

    public string? ProductStatus { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product? Product { get; set; }
}
