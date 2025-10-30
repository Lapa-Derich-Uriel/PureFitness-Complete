using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureFitness.Context;
using PureFitness.Models;
using PureFitness.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PureFitness.Controllers
{
    public class PaymentController : Controller
    {
        private readonly MyDBContext _context;

        public PaymentController(MyDBContext context)
        {
            _context = context;
        }

        // GET: show payment page
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> PaymentList()
        {
            var vm = new PaymentViewModel
            {
                Members = await _context.Members
                    .AsNoTracking()
                    .OrderBy(m => m.Fname)
                    .ThenBy(m => m.Lname)
                    .ToListAsync(),

                Products = await _context.Inventories
                    .Include(p => p.Product)   // ✅ Add this line
                    .AsNoTracking()
                    .Where(p => p.ProductQuantity > 0)
                    .OrderBy(p => p.Product!.ProductName)
                    .ToListAsync(),


                payment = new Payment
                {
                    TransactionDate = DateOnly.FromDateTime(DateTime.Now)
                }
            };

            return View(vm);
        }

        // POST: create payment (AJAX)
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment([FromForm] Payment payment, string MemberName)
        {
            Console.WriteLine("➡️ Raw Items JSON: " + payment.Items);
            if (payment == null || string.IsNullOrWhiteSpace(payment.PaymentType))
                return BadRequest(new { success = false, message = "Invalid payment data." });

            decimal cost = 0m;
            decimal total = 0m;
            string items = "";
            string memberDisplayName = "";
            DateTime now = DateTime.Now;

            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        Console.WriteLine("Received PaymentType: " + payment?.PaymentType);
                        var paymentFor = (payment!.PaymentType ?? "").Trim().ToLower();

                        switch (paymentFor)
                        {
                            case "initial":
                                // 🟩 Initial payment: record and set Paid = "Paid"
                                cost = payment.Cost ?? 0m;
                                total = payment.TotalCost ?? cost;

                                var initMember = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == payment.MemberId);
                                if (initMember != null)
                                {
                                    initMember.PaidStatus = "Paid";
                                    await _context.SaveChangesAsync();
                                    memberDisplayName = $"{initMember.Fname} {initMember.Lname}";
                                }
                                else
                                {
                                    memberDisplayName = "Unknown Member";
                                }
                                break;

                            case "membership":
                                // 🟩 Membership renewal: extend due date & mark as Paid
                                cost = payment.Cost ?? 0m;
                                total = payment.TotalCost ?? cost;

                                var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberId == payment.MemberId);
                                if (member == null)
                                    throw new Exception("Member not found for renewal.");

                                memberDisplayName = $"{member.Fname} {member.Lname}";

                                // Determine membership period (in days)
                                string period = (Request.Form["MembershipPeriod"].ToString() ?? payment.Items ?? "").ToLower().Trim();
                                int daysToAdd = 0;

                                if (period.Contains("1month") || period.Contains("1 month")) daysToAdd = 30;
                                else if (period.Contains("3months") || period.Contains("3 months")) daysToAdd = 90;
                                else if (period.Contains("6months") || period.Contains("6 months")) daysToAdd = 180;
                                else if (period.Contains("1year") || period.Contains("12months") || period.Contains("1 year") || period.Contains("12 months")) daysToAdd = 365;

                                if (daysToAdd <= 0)
                                    throw new Exception("Invalid membership duration — please check MembershipPeriod or Items mapping.");

                                ExtendMembership(member, DateOnly.FromDateTime(now), daysToAdd);
                                member.PaidStatus = "Paid";

                                await _context.SaveChangesAsync();
                                Console.WriteLine($"✅ Extended membership for {memberDisplayName} by {daysToAdd} days. New due: {member.DueDate}");
                                break;

                            case "walkin":
                                // 🟩 Walk-in: just record payment and attendance
                                cost = payment.Cost ?? 0m;
                                total = payment.TotalCost ?? cost;
                                payment.WalkInName = MemberName ?? "Walk-in Customer";
                                memberDisplayName = payment.WalkInName;
                                break;

                            case "products":
                                // 🟩 Product purchase: update inventory
                                Console.WriteLine("🧩 Raw payment.Items content: " + payment.Items);
                                if (!string.IsNullOrWhiteSpace(payment.Items))
                                {
                                    var productList = System.Text.Json.JsonSerializer.Deserialize<List<ProductItemDto>>(payment.Items);
                                    Console.WriteLine("🧩 Raw payment.Items content: " + payment.Items);

                                    Console.WriteLine("➡️ Deserialized product count: " + (productList?.Count ?? 0));

                                    if (productList != null && productList.Count > 0)
                                    {
                                        cost = productList.Sum(p => p.subtotal);
                                        total = cost;
                                        items = string.Join(", ", productList.Select(p => $"{p.name} (x{p.qty})"));

                                        foreach (var p in productList)
                                        {
                                            var inv = await _context.Inventories
                                                .FirstOrDefaultAsync(i => i.InventoryId == p.InventoryId);

                                            if (inv == null)
                                                throw new Exception($"Inventory not found for product: {p.name}");

                                            if (inv.ProductQuantity == null || inv.ProductQuantity < p.qty)
                                                throw new Exception($"Not enough stock for {p.name}");

                                            inv.ProductQuantity -= p.qty;
                                            Console.WriteLine($"🧮 Deducted {p.qty} from {p.name}. Remaining: {inv.ProductQuantity}");
                                        }

                                        await _context.SaveChangesAsync();

                                        payment.ItemQuantity = productList.Sum(p => p.qty);
                                    }
                                }

                                memberDisplayName = MemberName ?? "Product Purchase";
                                break;

                            default:
                                throw new InvalidOperationException("Unsupported payment type.");
                        }

                        // ✅ Save payment record
                        var newPayment = new Payment
                        {
                            StaffId = payment.StaffId,
                            MemberId = payment.MemberId,
                            TransactionDate = DateOnly.FromDateTime(now),
                            PaymentType = payment.PaymentType,
                            Items = string.IsNullOrEmpty(items) ? payment.Items : items,
                            Cost = cost,
                            TotalCost = total,
                            Amount = payment.Amount,
                            Change = payment.Change,
                            WalkInName = payment.WalkInName
                        };

                        _context.Payments.Add(newPayment);
                        await _context.SaveChangesAsync();

                        // ✅ Record attendance for walk-in
                        if (paymentFor == "walkin")
                        {
                            var attendance = new Attendance
                            {
                                MemberId = null,
                                AttendanceDate = DateOnly.FromDateTime(now),
                                TimeIn = TimeOnly.FromDateTime(now),
                                AttendanceStatus = "Present",
                                Activity = "Walk-in Gym Use",
                                ReceiptId = newPayment.ReceiptId
                            };

                            _context.Attendances.Add(attendance);
                            await _context.SaveChangesAsync();

                            Console.WriteLine($"✅ Walk-in attendance recorded for {payment.WalkInName}");
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                // ✅ Return success JSON
                return Json(new
                {
                    success = true,
                    receipt = new
                    {
                        paymentType = payment.PaymentType,
                        memberName = memberDisplayName,
                        transactionDate = now.ToString("yyyy-MM-dd HH:mm:ss"),
                        cost,
                        totalCost = total,
                        amount = payment.Amount,
                        change = payment.Change
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Error saving payment: " + ex.Message });
            }
        }

        // DTO for product JSON
        public class ProductItemDto
        {
            public int InventoryId { get; set; }
            public string? name { get; set; }
            public int qty { get; set; }
            public decimal price { get; set; }
            public decimal subtotal { get; set; }
        }

        // Helper: extend/reset membership and mark as Paid
        private void ExtendMembership(Member member, DateOnly now, int daysToAdd)
        {
            DateOnly? dueDate = member.DueDate;

            if (dueDate.HasValue && dueDate.Value >= now)
            {
                // Still active → extend from current expiration
                member.DueDate = dueDate.Value.AddDays(daysToAdd);
            }
            else
            {
                // Expired → start new period from now
                member.DueDate = now.AddDays(daysToAdd);
            }

            member.PaidStatus = "Paid";
            _context.Members.Update(member);
        }
    }
}
