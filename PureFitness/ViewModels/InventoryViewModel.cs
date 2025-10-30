using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class InventoryViewModel
    {
        public Inventory? inventory { get; set; }

        public Supplier? supplier { get; set; }

        public List<Inventory>? Inventories { get; set; }
        public List<Supplier>? Suppliers { get; set; }
    }
}
