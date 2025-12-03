using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PureFitness.Models;
using PureFitness.ViewModels;
using PureFitness.Context;
using System;
using System.Linq;

namespace PureFitness.Controllers
{
    public class DashboardController : Controller
    {
        private readonly MyDBContext _context;

        public DashboardController(MyDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var username = HttpContext.Session.GetString("Username");

            // 🧠 ADMIN DASHBOARD
            if (role == "Admin")
            {
                // Total active members
                 var totalMembers = _context.Members.Count(m => m.Status == "Active");

                // New memberships (joined within the past Month)
                var monthlymembers = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
                var newMembersThisMonth = _context.Members.Count(m => m.DateJoined >= monthlymembers);

                // Active equipment
                var activeEquipmentItems = _context.EquipmentItems.Count(e => e.EquipmentItemStatus == "Available");

                // Under maintenance
                var equipmentsUnderMaintenance = _context.EquipmentItems.Count(e => e.EquipmentItemStatus == "Under Maintenance");

                // Total staff
                var totalStaff = _context.Staffs.Count();

                // Low stock inventory
                var lowStockItems = _context.Inventories
                    .Include(i => i.Product)
                    .Where(i => i.ProductQuantity <= 5)
                    .ToList();

                // Pass to View
                ViewBag.TotalMembers = totalMembers;
                ViewBag.NewMembersThisMonth = newMembersThisMonth;
                ViewBag.ActiveEquipmentItems = activeEquipmentItems;
                ViewBag.EquipmentsUnderMaintenance = equipmentsUnderMaintenance;
                ViewBag.TotalStaff = totalStaff;
                ViewBag.LowStockItems = lowStockItems;

                return View(new DashboardViewModel());
            }

            // 👷 STAFF DASHBOARD
            else if (role == "Staff")
            {
                var staff = _context.Staffs.FirstOrDefault(s => s.User!.Username == username);

                var schedules = _context.EquipmentSchedules
                    .Include(e => e.Equipment)
                    .Where(s => s.StaffId == staff!.StaffId)
                    .ToList();

                var viewModel = new DashboardViewModel
                {
                    Name = staff?.Name ?? username,
                    AssignedTask = schedules.Any() ? "Perform scheduled maintenance tasks." : "No tasks assigned.",
                    EquipmentSchedules = schedules
                };

                return View(viewModel);
            }

            // 💪 MEMBER DASHBOARD
            else if (role == "Member")
            {
                var member = _context.Members
                    .Include(m => m.User)
                    .FirstOrDefault(m => m.User!.Email == username);

                if (member != null)
                {
                    var viewModel = new DashboardViewModel
                    {
                        member = member,
                        WorkoutGoal = member.FitnessGoal ?? "No goal set."
                    };
                    return View(viewModel);
                }
            }

            // ❌ Fallback
            return View(new DashboardViewModel());
        }
    }
}
