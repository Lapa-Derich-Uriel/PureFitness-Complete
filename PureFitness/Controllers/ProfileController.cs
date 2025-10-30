using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureFitness.Context;
using PureFitness.Models;
using PureFitness.ViewModels;

namespace PureFitness.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MyDBContext _context;

        public ProfileController(MyDBContext context)
        {
            _context = context;
        }

        // --------------------------
        // VIEW PROFILE
        // --------------------------
        public IActionResult AccountProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("LoginView", "Login");

            // Include everything related
            var user = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Members)
                .Include(u => u.Staff)
                .FirstOrDefault(u => u.UserId == userId.Value);

            if (user == null)
                return NotFound();

            var role = user.Role?.RoleName ?? "Member";

            // Initialize ViewModel
            var vm = new ProfileViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                Password = string.Empty,
                Email = user.Email,
                RoleName = role
            };

            // MEMBER DETAILS
            if (role.Equals("Member", StringComparison.OrdinalIgnoreCase))
            {
                var m = _context.Members
                    .Include(m => m.User)
                    .FirstOrDefault(m => m.UserId == userId.Value);

                if (m != null)
                {
                    vm.Member = new Member
                    {
                        MemberId = m.MemberId,
                        Fname = m.Fname,
                        Lname = m.Lname,
                        Gender = m.Gender,
                        PhoneNumber = m.PhoneNumber,
                        Address = m.Address,
                        MembershipType = m.MembershipType,
                        MembershipPeriod = m.MembershipPeriod,
                        DueDate = m.DueDate,
                        AccessType = m.AccessType,
                        Weight = m.Weight,
                        Height = m.Height,
                        FitnessGoal = m.FitnessGoal,
                        Age = m.Age,
                        Bmi = m.Bmi,
                        DateJoined = m.DateJoined
                    };

                    // If email was stored under Member.User instead of User
                    if (string.IsNullOrEmpty(vm.Email))
                        vm.Email = m.User?.Email;
                }
            }

            // STAFF DETAILS
            else if (role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                var s = _context.Staffs
                    .Include(st => st.User)
                    .FirstOrDefault(st => st.UserId == userId.Value);

                if (s != null)
                {
                    vm.Staff = new Staff
                    {
                        StaffId = s.StaffId,
                        Name = s.Name,
                        PhoneNumber = s.PhoneNumber
                    };

                    if (string.IsNullOrEmpty(vm.Email))
                        vm.Email = s.User?.Email;
                }
            }

            return View(vm);
        }


        // --------------------------
        // UPDATE PROFILE
        // --------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View("AccountProfile", model);

            var user = _context.Users
                .Include(u => u.Members)
                .Include(u => u.Staff)
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserId == model.Id);

            if (user == null)
                return NotFound();

            // --- Common fields ---
            user.Username = model.Username;

            // 🧠 Password remains unchanged if left blank
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // In a real app, hash before storing:
                // user.Password = _passwordHasher.HashPassword(model.Password);
                user.Password = model.Password;
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
                user.Email = model.Email;

            // --- Role-specific updates ---
            if (model.RoleName.Equals("Member", StringComparison.OrdinalIgnoreCase) && user.Members.Any())
            {
                var m = user.Members.First();
                m.PhoneNumber = model.Member?.PhoneNumber ?? m.PhoneNumber;
            }
            else if (model.RoleName.Equals("Staff", StringComparison.OrdinalIgnoreCase) && user.Staff.Any())
            {
                var s = user.Staff.First();
                s.Name = model.Staff?.Name ?? s.Name;
                s.PhoneNumber = model.Staff?.PhoneNumber ?? s.PhoneNumber;
            }

            _context.SaveChanges();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("AccountProfile");
        }
    }
}
