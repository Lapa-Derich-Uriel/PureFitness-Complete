using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureFitness.Models;
using PureFitness.ViewModels;
using PureFitness.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PureFitness.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly MyDBContext _context;

        public EquipmentController(MyDBContext context)
        {
            _context = context;
        }

        // --------------------------
        // LIST VIEW (Dashboard)
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> EquipmentList()
        {
            // --- STEP 1: Auto-create missing EquipmentItems ---
            var equipmentsToSeed = await _context.Equipments
                .Where(e => e.Quantity > 0 &&
                            !string.IsNullOrWhiteSpace(e.EquipmentName) &&
                            !_context.EquipmentItems.Any(i => i.EquipmentId == e.EquipmentId))
                .ToListAsync();

            if (equipmentsToSeed.Any())
            {
                foreach (var eq in equipmentsToSeed)
                {
                    var newItems = new List<EquipmentItem>();
                    for (int i = 1; i <= eq.Quantity!.Value; i++)
                    {
                        newItems.Add(new EquipmentItem
                        {
                            EquipmentId = eq.EquipmentId,
                            EquipmentItemName = $"{eq.EquipmentName} #{i}",
                            EquipmentItemStatus = "Available"
                        });
                    }

                    _context.EquipmentItems.AddRange(newItems);
                }

                await _context.SaveChangesAsync();
            }

            // --- STEP 2: Load all related data ---
            var vm = new EquipmentViewModel
            {
                // 🔹 Equipments and their items
                Equipments = await _context.Equipments
                    .Include(e => e.EquipmentItems)
                    .AsNoTracking()
                    .OrderBy(e => e.EquipmentName)
                    .ToListAsync(),

                // 🔹 Equipment items and parent equipment
                EquipmentItems = await _context.EquipmentItems
                    .Include(i => i.Equipment)
                    .AsNoTracking()
                    .ToListAsync(),

                // 🔹 Equipment schedules — now includes BOTH Equipment and EquipmentItem.Equipment
                EquipmentSchedules = await _context.EquipmentSchedules
                    .Include(s => s.Equipment) // ✅ fixes missing Category/Equipment for general schedules
                    .Include(s => s.EquipmentItem)
                        .ThenInclude(i => i!.Equipment)
                    .Include(s => s.Staff)
                    .AsNoTracking()
                    .OrderByDescending(s => s.MaintenanceDate)
                    .ToListAsync(),

                // 🔹 Logs including their schedule, item, and staff
                EquipmentLogs = await _context.EquipmentLogs
                    .Include(l => l.Schedule)
                        .ThenInclude(s => s!.Equipment)
                    .Include(l => l.Schedule)
                        .ThenInclude(s => s!.EquipmentItem)
                            .ThenInclude(i => i!.Equipment)
                    .Include(l => l.Schedule)
                        .ThenInclude(s => s!.Staff)
                    .AsNoTracking()
                    .OrderByDescending(l => l.Schedule!.MaintenanceDate)
                    .ToListAsync(),

                // 🔹 Staffs
                Staffs = await _context.Staffs
                    .AsNoTracking()
                    .OrderBy(s => s.Name)
                    .ToListAsync()
            };

            // --- STEP 3: Return to view ---
            return View(vm);
        }

        // --------------------------
        // EQUIPMENT TABLE (Partial Refresh)
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> EquipmentTable()
        {
            var list = await _context.Equipments
                .Include(e => e.EquipmentItems)
                .AsNoTracking()
                .OrderBy(e => e.EquipmentName)
                .ToListAsync();

            return PartialView("_EquipmentTable", list);
        }

        // --------------------------
        // ADD EQUIPMENT
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public IActionResult AddEquipment()
        {
            return PartialView("_EquipmentForm", new Equipment());
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEquipment(Equipment model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input");

            if (string.IsNullOrWhiteSpace(model.EquipmentName))
                return BadRequest("Equipment name is required");

            model.Quantity ??= 0;
            model.Capacity ??= 1; // Default to 1 if not provided
            model.Program ??= "N/A"; // Default if blank

            _context.Equipments.Add(model);
            await _context.SaveChangesAsync();

            // Auto-create EquipmentItems based on quantity
            var items = new List<EquipmentItem>();
            for (int i = 1; i <= model.Quantity; i++)
            {
                items.Add(new EquipmentItem
                {
                    EquipmentId = model.EquipmentId,
                    EquipmentItemName = $"{model.EquipmentName} Item {i}",
                    EquipmentItemStatus = "Available"
                });
            }

            if (items.Count > 0)
            {
                _context.EquipmentItems.AddRange(items);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // --------------------------
        // EDIT EQUIPMENT
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> EditEquipment(int id)
        {
            var entity = await _context.Equipments.FindAsync(id);
            if (entity == null) return NotFound();

            return PartialView("_EquipmentForm", entity);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEquipment(Equipment model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input");

            var entity = await _context.Equipments.FindAsync(model.EquipmentId);
            if (entity == null) return NotFound();

            entity.EquipmentName = model.EquipmentName;
            entity.EquipmentCategory = model.EquipmentCategory;
            entity.Quantity = model.Quantity ?? 0;
            entity.Capacity = model.Capacity ?? 1;
            entity.Program = model.Program ?? "N/A";

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --------------------------
        // DELETE EQUIPMENT
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Equipments.FindAsync(id);
            if (entity == null) return NotFound();

            var items = await _context.EquipmentItems.Where(i => i.EquipmentId == id).ToListAsync();
            if (items.Any())
                _context.EquipmentItems.RemoveRange(items);

            _context.Equipments.Remove(entity);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // --------------------------
        // EQUIPMENT ITEMS
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> EquipmentItemList(int equipmentId)
        {
            var equipment = await _context.Equipments.FindAsync(equipmentId);
            if (equipment == null) return NotFound();

            var items = await _context.EquipmentItems
                .Where(i => i.EquipmentId == equipmentId)
                .AsNoTracking()
                .ToListAsync();

            items = items.OrderBy(i => ExtractNumber(i.EquipmentItemName!)).ToList();

            ViewBag.EquipmentName = equipment.EquipmentName;
            ViewBag.EquipmentId = equipmentId;

            return PartialView("_EquipmentItemList", items);
        }

        public class ItemStatusDto
        {
            public int EquipmentItemId { get; set; }
            public string? Status { get; set; }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateItems([FromBody] List<ItemStatusDto> updates)
        {
            if (updates == null || updates.Count == 0)
                return BadRequest("No updates provided");

            var ids = updates.Select(u => u.EquipmentItemId).ToList();
            var items = await _context.EquipmentItems.Where(i => ids.Contains(i.EquipmentItemId)).ToListAsync();

            foreach (var item in items)
            {
                var dto = updates.First(u => u.EquipmentItemId == item.EquipmentItemId);
                item.EquipmentItemStatus = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --------------------------
        // MAINTENANCE SCHEDULES (fixed)
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Schedules()
        {
            var viewModel = new EquipmentScheduleViewModel
            {
                EquipmentSchedules = await _context.EquipmentSchedules
                    .Include(s => s.Equipment)
                    .Where(s => s.EquipmentItemId == null)
                    .OrderByDescending(s => s.NextMaintenanceDate)
                    .ToListAsync(),

                ItemSchedules = await _context.EquipmentSchedules
                    .Include(s => s.Equipment)
                    .Include(s => s.EquipmentItem)
                    .Where(s => s.EquipmentItemId != null)
                    .OrderByDescending(s => s.NextMaintenanceDate)
                    .ToListAsync()
            };

            // Return partial so AJAX gets only the HTML (no layout)
            return PartialView("_SchedulesContainer", viewModel);
        }



        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> AddSchedule(string type)
        {
            try
            {
                // Load Staff list
                ViewBag.Staffs = await _context.Staffs
                    .AsNoTracking()
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                // Handle ITEM schedule
                if (string.Equals(type, "item", StringComparison.OrdinalIgnoreCase))
                {
                    var items = await _context.EquipmentItems
                        .Include(i => i.Equipment)
                        .Where(i => i.EquipmentItemStatus == "Under Maintenance" ||
                                    i.EquipmentItemStatus == "UnderMaintenance")
                        .OrderBy(i => i.EquipmentItemName)
                        .ToListAsync();

                    ViewBag.EquipmentItems = items;
                    return PartialView("_ScheduleForm", new EquipmentSchedule());
                }

                // Handle GENERAL EQUIPMENT schedule
                var equipments = await _context.Equipments
                    .AsNoTracking()
                    .OrderBy(e => e.EquipmentName)
                    .ToListAsync();

                ViewBag.Equipments = equipments;
                return PartialView("_AddEquipmentSchedule", new EquipmentSchedule());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error loading schedule form: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(EquipmentSchedule schedule)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input. Please check all required fields.");

            try
            {
                bool isEquipmentSchedule = schedule.EquipmentId.HasValue && schedule.EquipmentId > 0;
                bool isItemSchedule = schedule.EquipmentItemId.HasValue && schedule.EquipmentItemId > 0;

                // Default date if missing
                schedule.MaintenanceDate ??= DateOnly.FromDateTime(DateTime.Today);

                // Compute NextMaintenanceDate if applicable
                if (isEquipmentSchedule && !string.IsNullOrWhiteSpace(schedule.Frequency))
                {
                    var baseDate = schedule.MaintenanceDate.Value.ToDateTime(TimeOnly.MinValue);
                    string freq = schedule.Frequency.Trim().ToLower();

                    schedule.NextMaintenanceDate = freq switch
                    {
                        "daily" => DateOnly.FromDateTime(baseDate.AddDays(1)),
                        "weekly" => DateOnly.FromDateTime(baseDate.AddDays(7)),
                        "biweekly" => DateOnly.FromDateTime(baseDate.AddDays(14)),
                        "monthly" => DateOnly.FromDateTime(baseDate.AddMonths(1)),
                        "quarterly" => DateOnly.FromDateTime(baseDate.AddMonths(3)),
                        "semi-annual" or "semiannual" => DateOnly.FromDateTime(baseDate.AddMonths(6)),
                        "annual" or "yearly" => DateOnly.FromDateTime(baseDate.AddYears(1)),
                        _ => null
                    };
                }

                // Save schedule first
                _context.EquipmentSchedules.Add(schedule);
                await _context.SaveChangesAsync();

                // Keep the user’s input remarks
                schedule.Remarks ??= isItemSchedule ? "Item Maintenance Scheduled" : "General Maintenance Scheduled";


                // ✅ Create a corresponding EquipmentLog entry
                var log = new EquipmentLog
                {
                    ScheduleId = schedule.ScheduleId,
                    DateCreated = DateOnly.FromDateTime(DateTime.Now),
                };

                _context.EquipmentLogs.Add(log);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating schedule: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var sched = await _context.EquipmentSchedules
                .Include(s => s.EquipmentLogs) // if you have nav property
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (sched == null)
                return Json(new { success = false, message = "Schedule not found." });

            // Manually delete dependent logs first
            var relatedLogs = await _context.EquipmentLogs
                .Where(l => l.ScheduleId == id)
                .ToListAsync();

            if (relatedLogs.Any())
                _context.EquipmentLogs.RemoveRange(relatedLogs);

            _context.EquipmentSchedules.Remove(sched);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }



        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> LogsTable()
        {
            var logs = _context.EquipmentLogs
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.EquipmentItem)
                .Include(l => l.Schedule!.Staff)
                .Where(l => l.Schedule!.EquipmentItemId != null) // ✅ Only logs tied to specific items
                .OrderByDescending(l => l.DateCreated)
                .ToList();


            return PartialView("_EquipmentLogs", logs);
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> SchedulesDebug()
        {
            var schedules = await _context.EquipmentSchedules
                .Include(s => s.EquipmentItem)
                    .ThenInclude(i => i!.Equipment)
                .Include(s => s.Equipment)
                .Include(s => s.Staff)
                .AsNoTracking()
                .OrderByDescending(s => s.MaintenanceDate)
                .ToListAsync();

            return Json(new
            {
                Total = schedules.Count,
                EquipmentSchedules = schedules.Where(s => s.EquipmentItemId == null)
                    .Select(s => new
                    {
                        s.ScheduleId,
                        s.EquipmentId,
                        s.EquipmentItemId,
                        s.MaintenanceDate,
                        s.NextMaintenanceDate,
                        s.Frequency,
                        s.Remarks
                    }),
                ItemSchedules = schedules.Where(s => s.EquipmentItemId != null)
                    .Select(s => new
                    {
                        s.ScheduleId,
                        s.EquipmentId,
                        s.EquipmentItemId,
                        s.MaintenanceDate,
                        s.NextMaintenanceDate,
                        s.Frequency,
                        s.Remarks
                    })
            });
        }


        private int ExtractNumber(string name)
        {
            var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }
    }
}
