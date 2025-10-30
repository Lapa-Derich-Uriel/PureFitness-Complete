using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? SupplierId { get; set; }

    public string? ProductName { get; set; }

    public decimal? ProductPrice { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Supplier? Supplier { get; set; }
}
