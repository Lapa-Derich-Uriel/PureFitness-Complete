using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureFitness.Context;
using PureFitness.Models;
using PureFitness.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PureFitness.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class InventoryController : Controller
    {
        private readonly MyDBContext _context;

        public InventoryController(MyDBContext context)
        {
            _context = context;
        }

        // -----------------------------
        // LIST VIEW
        // -----------------------------
        public async Task<IActionResult> InventoryList()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Product)
                .AsNoTracking()
                .ToListAsync();

            var suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();

            foreach (var item in inventories)
            {
                item.ProductStatus = CalculateStatus(item.ProductQuantity ?? 0);
            }

            var viewModel = new InventoryViewModel
            {
                Inventories = inventories,
                Suppliers = suppliers
            };

            return View(viewModel);
        }

        // -----------------------------
        // ADD PRODUCT
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, int ProductQuantity, int? SupplierId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(product.ProductName))
                return BadRequest("Product name is required.");

            if (product.ProductPrice < 0)
                return BadRequest("Price cannot be negative.");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var inventory = new Inventory
            {
                ProductId = product.ProductId,
                ProductQuantity = ProductQuantity,
                ProductStatus = CalculateStatus(ProductQuantity),
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Optional: store supplier relationship
            if (SupplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(SupplierId);
                if (supplier != null)
                {
                    // You can extend this part to save supplier info in a linking table
                }
            }

            return RedirectToAction(nameof(InventoryList));
        }

        // -----------------------------
        // EDIT PRODUCT
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product updatedProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(updatedProduct.ProductId);
            if (product == null)
                return NotFound();

            product.ProductName = updatedProduct.ProductName;
            product.ProductPrice = updatedProduct.ProductPrice;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(InventoryList));
        }

        // -----------------------------
        // ADD QUANTITY
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuantity(int inventoryId, int quantity, int? supplierId)
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than zero.");

            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

            if (inventory == null)
                return NotFound();

            inventory.ProductQuantity = (inventory.ProductQuantity ?? 0) + quantity;
            inventory.ProductStatus = CalculateStatus(inventory.ProductQuantity.Value);

            // Optional: track supplier info here (future extension)
            if (supplierId.HasValue)
            {
                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier != null)
                {
                    // You can log restocks or link to supplier
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(InventoryList));
        }

        // -----------------------------
        // DELETE PRODUCT
        // -----------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == id);

            if (inventory == null)
                return NotFound();

            if (inventory.Product != null)
                _context.Products.Remove(inventory.Product);

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(InventoryList));
        }

        // -----------------------------
        // SUPPLIER MANAGEMENT
        // -----------------------------
        public async Task<IActionResult> SupplierList()
        {
            var suppliers = await _context.Suppliers
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSupplier(Supplier supplier)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(InventoryList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupplier(Supplier updatedSupplier)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = await _context.Suppliers.FindAsync(updatedSupplier.SupplierId);
            if (supplier == null)
                return NotFound();

            supplier.SupplierName = updatedSupplier.SupplierName;
            supplier.PhoneNumber = updatedSupplier.PhoneNumber;
            supplier.Email = updatedSupplier.Email;
            supplier.Address = updatedSupplier.Address;

            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(InventoryList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(InventoryList));
        }

        // -----------------------------
        // SEARCH FUNCTION
        // -----------------------------
        [HttpPost]
        public async Task<IActionResult> Search(string search, string statusFilter)
        {
            var query = _context.Inventories
                .Include(p => p.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Product!.ProductName!.Contains(search));

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                if (statusFilter.Equals("available", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.ProductStatus == "Available");
                else if (statusFilter.Equals("low", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.ProductStatus == "Low Stock");
                else if (statusFilter.Equals("out", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(p => p.ProductStatus == "Out of Stock");
            }

            var products = await query.OrderBy(p => p.InventoryId).ToListAsync();
            foreach (var p in products)
                p.ProductStatus = CalculateStatus(p.ProductQuantity ?? 0);

            return PartialView("_InventoryTablePartial", products);
        }

        // -----------------------------
        // HELPER
        // -----------------------------
        private static string CalculateStatus(int qty)
        {
            if (qty == 0) return "Out of Stock";
            if (qty <= 10) return "Low Stock";
            return "Available";
        }
    }
}
