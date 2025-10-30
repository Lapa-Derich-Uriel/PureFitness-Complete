using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using PureFitness.Context;
using PureFitness.ViewModels;
using PureFitness.Models;
using System.Security.Claims;

namespace PureFitness.Controllers
{
    [Authorize]
    public class WorkoutAndDietPlanController : Controller
    {
        private readonly MyDBContext _context;
        public WorkoutAndDietPlanController(MyDBContext context)
        {
            _context = context;
        }

        // --------------------------
        // LIST VIEW
        // --------------------------
        public async Task<IActionResult> ExerciseAndDietPlanList(int? staffId)
        {
            var (role, username) = GetCurrentUser();

            IQueryable<Member> query = _context.Members
                .AsNoTracking()
                .Include(m => m.Staff)
                .Include(m => m.WorkoutPlans)
                    .ThenInclude(w => w.WorkoutPlanItems)
                        .ThenInclude(i => i.Equipment)
                .Include(m => m.DietPlans)
                    .ThenInclude(d => d.DietPlanItems);

            if (role == "Member")
            {
                var member = await _context.Members
                    .Include(m => m.Staff)
                    .Include(m => m.WorkoutPlans)
                        .ThenInclude(w => w.WorkoutPlanItems)
                            .ThenInclude(i => i.Equipment)
                    .Include(m => m.DietPlans)
                        .ThenInclude(d => d.DietPlanItems)
                    .FirstOrDefaultAsync(m => m.User!.Username == username);

                if (member == null)
                    return Unauthorized("Member not found.");

                var vm = new WorkoutAndDietPlanViewModel
                {
                    MemberId = member.MemberId,
                    Member = member,
                    StaffId = member.StaffId,
                    Staff = member.Staff,
                    WorkoutPlan = member.WorkoutPlans.OrderByDescending(w => w.DateCreated).FirstOrDefault(),
                    WorkoutPlanItems = member.WorkoutPlans.OrderByDescending(w => w.DateCreated).FirstOrDefault()?.WorkoutPlanItems.ToList(),
                    DietPlan = member.DietPlans.OrderByDescending(d => d.DateCreated).FirstOrDefault(),
                    DietPlanItems = member.DietPlans.OrderByDescending(d => d.DateCreated).FirstOrDefault()?.DietPlanItems.ToList()
                };

                return View(new List<WorkoutAndDietPlanViewModel> { vm });
            }

            if (role == "Staff")
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.User!.Username == username);
                if (staff == null) return Unauthorized("Staff account not found.");
                query = query.Where(m => m.StaffId == staff.StaffId);
            }
            else if (role == "Admin" && staffId.HasValue)
            {
                query = query.Where(m => m.StaffId == staffId.Value);
            }

            var members = await query.ToListAsync();

            var viewModels = members.Select(m => new WorkoutAndDietPlanViewModel
            {
                MemberId = m.MemberId,
                Member = m,
                StaffId = m.StaffId,
                Staff = m.Staff,
                WorkoutPlan = m.WorkoutPlans.OrderByDescending(w => w.DateCreated).FirstOrDefault(),
                WorkoutPlanItems = m.WorkoutPlans.OrderByDescending(w => w.DateCreated).FirstOrDefault()?.WorkoutPlanItems.ToList(),
                DietPlan = m.DietPlans.OrderByDescending(d => d.DateCreated).FirstOrDefault(),
                DietPlanItems = m.DietPlans.OrderByDescending(d => d.DateCreated).FirstOrDefault()?.DietPlanItems.ToList()
            }).ToList();

            ViewBag.StaffList = await _context.Staffs.ToListAsync();
            ViewBag.EquipmentList = await _context.Equipments.ToListAsync();

            return View(viewModels);
        }

        // --------------------------
        // RETURN PLANS (only latest plan's items)
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetPlans(int memberId)
        {
            var member = await _context.Members
                .Include(m => m.Staff)
                .FirstOrDefaultAsync(m => m.MemberId == memberId);

            if (member == null)
                return NotFound();

            // Load latest workout plan only
            var workoutPlan = await _context.WorkoutPlans
                .AsNoTracking()
                .Include(w => w.WorkoutPlanItems)
                    .ThenInclude(i => i.Equipment)
                .Where(w => w.MemberId == memberId)
                .OrderByDescending(w => w.DateCreated)
                .FirstOrDefaultAsync();

            // Load latest diet plan only
            var dietPlan = await _context.DietPlans
                .AsNoTracking()
                .Include(d => d.DietPlanItems)
                .Where(d => d.MemberId == memberId)
                .OrderByDescending(d => d.DateCreated)
                .FirstOrDefaultAsync();

            var workouts = (workoutPlan?.WorkoutPlanItems?
                .Select(i => (object)new
                {
                    workoutPlanItemId = i.WorkoutPlanItemId,
                    exerciseName = i.ExerciseName,
                    equipmentId = i.EquipmentId,
                    equipmentName = i.Equipment != null ? i.Equipment.EquipmentName : "",
                    sets = i.Sets,
                    repetitions = i.Repetitions,
                    notes = i.Notes
                })
                .ToList()) ?? new List<object>();

            var diets = (dietPlan?.DietPlanItems?
                .Select(i => (object)new
                {
                    dietPlanItemId = i.DietPlanItemId,
                    mealName = i.MealName,
                    calorieTarget = i.CalorieTarget,
                    notes = i.Notes
                })
                .ToList()) ?? new List<object>();

            return Json(new
            {
                staffId = member.StaffId,
                staffName = member.Staff?.Name,
                workouts,
                diets
            });
        }

        // --------------------------
        // SAVE (updates latest plan; prevents duplicates)
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost("WorkoutAndDietPlan/SaveWorkoutAndDietPlan")]
        public async Task<IActionResult> SaveWorkoutAndDietPlan([FromBody] SaveWorkoutAndDietPlanRequest request)
        {
            if (request == null || request.MemberId == 0)
                return BadRequest("Invalid request data.");

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == request.MemberId);

            if (member == null)
                return NotFound("Member not found.");

            // --- WORKOUT PLAN: load latest plan from DB (deterministic)
            var workoutPlan = await _context.WorkoutPlans
                .Include(p => p.WorkoutPlanItems)
                .Where(p => p.MemberId == member.MemberId)
                .OrderByDescending(p => p.DateCreated)
                .FirstOrDefaultAsync();

            if (workoutPlan == null)
            {
                workoutPlan = new WorkoutPlan
                {
                    MemberId = member.MemberId,
                    StaffId = member.StaffId,
                    DateCreated = DateTime.UtcNow
                };
                _context.WorkoutPlans.Add(workoutPlan);
            }
            else
            {
                // Remove existing items and clear in-memory list before re-adding
                if (workoutPlan.WorkoutPlanItems?.Any() == true)
                {
                    _context.WorkoutPlanItems.RemoveRange(workoutPlan.WorkoutPlanItems);
                    workoutPlan.WorkoutPlanItems.Clear();
                }
            }

            if (request.Workouts?.Any() == true)
            {
                workoutPlan.WorkoutPlanItems = new List<WorkoutPlanItem>();
                foreach (var item in request.Workouts)
                {
                    workoutPlan.WorkoutPlanItems.Add(new WorkoutPlanItem
                    {
                        ExerciseName = item.ExerciseName,
                        EquipmentId = item.EquipmentId == 0 ? null : item.EquipmentId,
                        Sets = item.Sets,
                        Repetitions = item.Repetitions,
                        Notes = item.Notes
                    });
                }
            }

            // --- DIET PLAN: load latest plan from DB (deterministic)
            var dietPlan = await _context.DietPlans
                .Include(p => p.DietPlanItems)
                .Where(p => p.MemberId == member.MemberId)
                .OrderByDescending(p => p.DateCreated)
                .FirstOrDefaultAsync();

            if (dietPlan == null)
            {
                dietPlan = new DietPlan
                {
                    MemberId = member.MemberId,
                    StaffId = member.StaffId,
                    DateCreated = DateTime.UtcNow
                };
                _context.DietPlans.Add(dietPlan);
            }
            else
            {
                if (dietPlan.DietPlanItems?.Any() == true)
                {
                    _context.DietPlanItems.RemoveRange(dietPlan.DietPlanItems);
                    dietPlan.DietPlanItems.Clear();
                }
            }

            if (request.Diets?.Any() == true)
            {
                dietPlan.DietPlanItems = new List<DietPlanItem>();
                foreach (var item in request.Diets)
                {
                    dietPlan.DietPlanItems.Add(new DietPlanItem
                    {
                        MealName = item.MealName,
                        CalorieTarget = item.CalorieTarget,
                        Notes = item.Notes
                    });
                }
            }

            // Persist once
            await _context.SaveChangesAsync();

            // Return canonical/latest data for UI re-render
            return await GetPlans(member.MemberId);
        }

        // --------------------------
        // DELETE ITEMS
        // --------------------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("DeleteWorkoutItem/{id}")]
        public async Task<IActionResult> DeleteWorkoutItem(int id)
        {
            var item = await _context.WorkoutPlanItems.FindAsync(id);
            if (item == null) return NotFound();
            _context.WorkoutPlanItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("DeleteDietItem/{id}")]
        public async Task<IActionResult> DeleteDietItem(int id)
        {
            var item = await _context.DietPlanItems.FindAsync(id);
            if (item == null) return NotFound();
            _context.DietPlanItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // --------------------------
        // Request DTOs
        // --------------------------
        public class SaveWorkoutAndDietPlanRequest
        {
            public int MemberId { get; set; }
            public List<WorkoutItemDto>? Workouts { get; set; }
            public List<DietItemDto>? Diets { get; set; }
        }

        public class WorkoutItemDto
        {
            public string ExerciseName { get; set; } = string.Empty;
            public int EquipmentId { get; set; }
            public int Sets { get; set; }
            public int Repetitions { get; set; }
            public string? Notes { get; set; }
        }

        public class DietItemDto
        {
            public string MealName { get; set; } = string.Empty;
            public int CalorieTarget { get; set; }
            public string? Notes { get; set; }
        }

        // --------------------------
        // Helper
        // --------------------------
        private (string Role, string Username) GetCurrentUser()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
            var username = User.Identity?.Name ?? string.Empty;
            return (role, username);
        }
    }
}
