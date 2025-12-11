using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PureFitness.Models;
using PureFitness.ViewModels;
using PureFitness.Context;
using System.Security.Cryptography;
using System.Linq;

namespace PureFitness.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly MyDBContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public StaffController(MyDBContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // --------------------------
        // STAFF LIST
        // --------------------------
        public IActionResult StaffList()
        {
            var staff = _context.Staffs
                .Include(s => s.User)
                .Where(s => s.User != null && s.User.Role!.RoleName == "Staff")
                .ToList();

            return View(staff);
        }

        // --------------------------
        // ADD (GET)
        // --------------------------
        public IActionResult Add()
        {
            var viewModel = new StaffViewModel
            {
                staff = new Staff(),
                user = new User()
            };
            return View(viewModel);
        }

        // --------------------------
        // ADD (POST)
        // --------------------------
        [HttpPost]
        public IActionResult AddStaff(StaffViewModel StaffVM)
        {
            var email = StaffVM.user?.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("user.Email", "Email is required.");
                return View("Add", StaffVM);
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("user.Email", "This email is already registered.");
                return View("Add", StaffVM);
            }

            // Ensure "Staff" role exists
            var role = _context.Roles.FirstOrDefault(r => r.RoleName == "Staff");
            if (role == null)
            {
                role = new Role { RoleName = "Staff" };
                _context.Roles.Add(role);
                _context.SaveChanges();
            }

            // Generate temporary password (plaintext)
            string rawPassword = GenerateSecurePassword();
            StaffVM.PlainPasswordForDisplay = rawPassword;

            // Create new user with hashed password
            var newUser = new User
            {
                Username = email,
                Email = email,
                Password = _passwordHasher.HashPassword(null!, rawPassword),
                RoleId = role.RoleId
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // ✅ Assign UserId and AssignedTask
            StaffVM.staff!.UserId = newUser.UserId;
            StaffVM.staff.AssignedTask = StaffVM.staff.AssignedTask; // already bound from form
            _context.Staffs.Add(StaffVM.staff);
            _context.SaveChanges();

            // Pass plaintext password via TempData for one-time display
            TempData["TempPassword"] = rawPassword;
            TempData["StaffId"] = StaffVM.staff.StaffId;

            // Redirect to Edit so password field shows plaintext once
            return RedirectToAction("StaffList");
        }

        // --------------------------
        // EDIT (GET)
        // --------------------------
        public IActionResult Edit(int id)
        {
            var staff = _context.Staffs
                .Include(s => s.User)
                .FirstOrDefault(s => s.StaffId == id);

            if (staff == null)
                return NotFound();

            var viewModel = new StaffViewModel
            {
                staff = staff,
                user = staff.User ?? new User(),
                PlainPasswordForDisplay = TempData["TempPassword"]?.ToString() ?? staff.User?.Password ?? ""
            };

            return View(viewModel);
        }

        // --------------------------
        // EDIT (POST)
        // --------------------------
        [HttpPost]
        public IActionResult EditStaffMember(StaffViewModel StaffVM)
        {
            var staffFromDB = _context.Staffs.Include(s => s.User)
                .FirstOrDefault(s => s.StaffId == StaffVM.staff!.StaffId);

            if (staffFromDB == null)
                return NotFound();

            // ✅ Update staff info
            staffFromDB.Name = StaffVM.staff!.Name;
            staffFromDB.PhoneNumber = StaffVM.staff.PhoneNumber;
            staffFromDB.AssignedTask = StaffVM.staff.AssignedTask; // ✅ Save assigned task

            // ✅ Update user info
            if (staffFromDB.User != null)
            {
                staffFromDB.User.Email = StaffVM.user!.Email;
                staffFromDB.User.Username = StaffVM.user!.Username;

                // Only re-hash if user entered a new (non-hashed) password
                if (!string.IsNullOrEmpty(StaffVM.user.Password) &&
                    !StaffVM.user.Password.StartsWith("$2")) // bcrypt-style
                {
                    staffFromDB.User.Password = _passwordHasher.HashPassword(staffFromDB.User, StaffVM.user.Password);
                }
            }

            _context.SaveChanges();

            return RedirectToAction("StaffList");
        }

        // --------------------------
        // RESET PASSWORD
        // --------------------------
        [HttpPost]
        public IActionResult ResetPassword(int id)
        {
            var staff = _context.Staffs.Include(s => s.User).FirstOrDefault(s => s.StaffId == id);
            if (staff == null || staff.User == null)
            {
                return NotFound();
            }

            string rawPassword = GenerateSecurePassword();
            staff.User.Password = _passwordHasher.HashPassword(staff.User, rawPassword);
            _context.SaveChanges();

            TempData["TempPassword"] = rawPassword;
            return RedirectToAction("Edit", new { id = staff.StaffId });
        }

        // --------------------------
        // DELETE
        // --------------------------
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Load staff with identity user
            var staff = _context.Staffs
                .Include(s => s.User)
                .FirstOrDefault(s => s.StaffId == id);

            if (staff == null)
                return NotFound();

            // 🚫 Prevent deletion if staff is still assigned to Members
            bool hasAssignedMembers = _context.Members.Any(m => m.StaffId == id);
            if (hasAssignedMembers)
            {
                return BadRequest("Cannot delete staff while they are still assigned to members. Please reassign those members first.");
            }

            // 🔄 Nullify StaffId in related WorkoutPlans
            var relatedWorkoutPlans = _context.WorkoutPlans
                .Where(wp => wp.StaffId == id)
                .ToList();

            foreach (var wp in relatedWorkoutPlans)
                wp.StaffId = null;

            // 🔄 Nullify StaffId in related DietPlans
            var relatedDietPlans = _context.DietPlans
                .Where(dp => dp.StaffId == id)
                .ToList();

            foreach (var dp in relatedDietPlans)
                dp.StaffId = null;

            // 🔄 Nullify StaffId in related EquipmentSchedules
            var relatedEquipmentSchedules = _context.EquipmentSchedules
                .Where(es => es.StaffId == id)
                .ToList();

            foreach (var es in relatedEquipmentSchedules)
                es.StaffId = null;

            // ❌ Remove linked ASP.NET Identity user
            if (staff.User != null)
                _context.Users.Remove(staff.User);

            // ❌ Remove staff record
            _context.Staffs.Remove(staff);

            _context.SaveChanges();

            return RedirectToAction("StaffList");
        }

        // --------------------------
        // SEARCH (AJAX)
        // --------------------------
        [HttpPost]
        public async Task<IActionResult> Search(string lowersearch)
        {
            IQueryable<Staff> query = _context.Staffs.Include(s => s.User);

            if (!string.IsNullOrWhiteSpace(lowersearch))
            {
                string search = lowersearch.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    (s.User != null && s.User.Email!.ToLower().Contains(search)) ||
                    (s.PhoneNumber != null && s.PhoneNumber.ToLower().Contains(search)) ||
                    (s.AssignedTask != null && s.AssignedTask.ToLower().Contains(search)) // ✅ searchable by task
                );
            }

            var result = await query.ToListAsync();
            return PartialView("_StaffTableBody", result);
        }

        // --------------------------
        // HELPER: SECURE PASSWORD
        // --------------------------
        private string GenerateSecurePassword(int length = 12)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return new string(bytes.Select(b => valid[b % valid.Length]).ToArray());
        }
    }
}
