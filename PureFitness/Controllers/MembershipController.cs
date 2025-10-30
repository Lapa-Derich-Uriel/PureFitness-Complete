using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PureFitness.Models;
using PureFitness.ViewModels;
using PureFitness.Context;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;

namespace PureFitness.Controllers
{
    [Authorize]
    public class MembershipController : Controller
    {
        private readonly MyDBContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public MembershipController(MyDBContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // ---------------- Member List ----------------
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult MemberList()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var members = _context.Members.Include(m => m.User).ToList();

            foreach (var member in members)
            {
                if (member.Status == "Canceled")
                    continue;

                var newStatus = member.DueDate < today ? "Inactive" : "Active";
                if (member.Status != newStatus)
                    member.Status = newStatus;
            }

            if (_context.ChangeTracker.HasChanges())
                _context.SaveChanges();

            return View(members);
        }

        // ---------------- Add Member (GET) ----------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new MemberViewModel
            {
                member = new Member(),
                user = new User()
            };
            return View(viewModel);
        }

        // ---------------- Add Member (POST) ----------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public IActionResult AddMember(MemberViewModel MemberVM)
        {
            // Basic validation
            if (!Regex.IsMatch(MemberVM?.user?.Email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("user.Email", "Invalid email format.");
                return View("Add", MemberVM);
            }

            if (_context.Members.Any(m => m.User!.Email == MemberVM!.user!.Email))
            {
                ModelState.AddModelError("user.Email", "This Member already exists!");
                return View("Add", MemberVM);
            }

            var role = _context.Roles.FirstOrDefault(r => r.RoleName == "Member");
            if (role == null)
            {
                role = new Role { RoleName = "Member" };
                _context.Roles.Add(role);
                _context.SaveChanges();
            }

            // Generate secure temporary password
            string rawPassword = GenerateSecurePassword();
            MemberVM!.PlainPasswordForDisplay = rawPassword;

            var newUser = new User
            {
                Username = MemberVM?.user!.Email,
                Password = _passwordHasher.HashPassword(null!, rawPassword),
                RoleId = role.RoleId,
                Email = MemberVM?.user!.Email
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            // Convert height/weight + BMI
            decimal heightInMeters = ConvertHeight(MemberVM!.member?.Height ?? 0, MemberVM!.HeightUnit ?? "cm");
            decimal weightInKg = ConvertWeight(MemberVM.member?.Weight ?? 0, MemberVM!.WeightUnit ?? "kg");
            decimal? bmi = (heightInMeters > 0) ? weightInKg / (heightInMeters * heightInMeters) : null;

            if (!Regex.IsMatch(MemberVM?.member?.PhoneNumber ?? "", @"^\d{11}$"))
            {
                ModelState.AddModelError("member.PhoneNumber", "Phone number must be exactly 11 digits.");
                return View("Add", MemberVM);
            }

            DateOnly? StartDate = MemberVM!.member!.DateJoined != default
                    ? MemberVM.member.DateJoined
                    : DateOnly.FromDateTime(DateTime.Now);

            MemberVM.member.DateJoined = StartDate;

            // ✅ Calculate DueDate based on DateJoined
            string membershipType, accessType;
            DateOnly? dueDate = CalculateDueDate(MemberVM.member.MembershipPeriod ?? "1 Month",
                                               MemberVM.member.AccessType,
                                               StartDate,
                                               out membershipType, out accessType);

            // Set member properties
            MemberVM.member.DueDate = dueDate;
            MemberVM.member.MembershipType = membershipType;
            MemberVM.member.AccessType = accessType;
            MemberVM.member.UserId = newUser.UserId;
            MemberVM.member.Height = Math.Round(heightInMeters, 2);
            MemberVM.member.Weight = Math.Round(weightInKg, 2);
            MemberVM.member.Bmi = bmi.HasValue ? Math.Round(bmi.Value, 2) : 0;
            MemberVM.member.Status = "Active";
            MemberVM.member.PaidStatus = "Pending";
            MemberVM.member.FitnessGoal = MemberVM.member.FitnessGoal ?? "General Fitness";
            MemberVM.member.MembershipPeriod = MemberVM.member.MembershipPeriod ?? "1 Month";

            if (!_context.Members.Any())
                _context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Members', RESEED, 0)");

            _context.Members.Add(MemberVM.member);
            _context.SaveChanges();

            TempData["TempPassword"] = rawPassword;
            TempData["NewMemberId"] = MemberVM.member.MemberId;
            TempData["TempEmail"] = MemberVM.user!.Email;
            TempData.Keep();

            return RedirectToAction("MemberList");
        }

        // ---------------- Edit (GET) ----------------
        [Authorize(Roles = "Admin,Staff")]
        public IActionResult Edit(int id)
        {
            var member = _context.Members.Include(u => u.User).FirstOrDefault(i => i.MemberId == id);
            if (member == null) return NotFound();

            var viewModel = new MemberViewModel
            {
                member = member,
                user = member.User,
                PlainPasswordForDisplay = TempData["TempPassword"]?.ToString() ?? ""
            };

            return View(viewModel);
        }

        // ---------------- EditMember (POST) ----------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public IActionResult EditMember(MemberViewModel MemberVM)
        {
            if (!Regex.IsMatch(MemberVM.member!.PhoneNumber ?? "", @"^[0-9]{11}$"))
            {
                ModelState.AddModelError("member.PhoneNumber", "Phone number must be exactly 11 digits.");
                return View("Edit", MemberVM);
            }

            var memberfromDB = _context.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.MemberId == MemberVM.member.MemberId);

            if (memberfromDB == null)
            {
                ModelState.AddModelError("", "Member not found");
                return View("Edit", MemberVM);
            }

            // Update member fields
            memberfromDB.Fname = MemberVM.member.Fname;
            memberfromDB.Lname = MemberVM.member.Lname;
            memberfromDB.Gender = MemberVM.member.Gender;
            memberfromDB.Age = MemberVM.member.Age;
            memberfromDB.PhoneNumber = MemberVM.member.PhoneNumber;
            memberfromDB.FitnessGoal = MemberVM.member.FitnessGoal ?? memberfromDB.FitnessGoal;
            memberfromDB.MembershipPeriod = MemberVM.member.MembershipPeriod ?? memberfromDB.MembershipPeriod;
            memberfromDB.Height = MemberVM.member.Height;
            memberfromDB.Weight = MemberVM.member.Weight;
            memberfromDB.Address = MemberVM.member.Address;

            // Update user fields
            if (memberfromDB.User != null)
            {
                memberfromDB.User.Email = MemberVM.user!.Email;
                memberfromDB.User.Username = MemberVM.user.Username;

                // Update password only if new one provided
                if (!string.IsNullOrEmpty(MemberVM.user?.Password) && MemberVM.user.Password.Length < 60)
                {
                    memberfromDB.User.Password = _passwordHasher.HashPassword(null!, MemberVM.user.Password);
                }
            }

            _context.SaveChanges();
            return RedirectToAction("MemberList", "Membership");
        }

        // ---------------- Delete Member ----------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var member = _context.Members.Include(m => m.User).FirstOrDefault(m => m.MemberId == id);
            if (member == null)
                return NotFound();

            var payments = _context.Payments.Where(p => p.MemberId == id).ToList();
            if (payments.Any()) _context.Payments.RemoveRange(payments);

            var attendances = _context.Attendances.Where(a => a.MemberId == id).ToList();
            if (attendances.Any()) _context.Attendances.RemoveRange(attendances);

            var workoutPlans = _context.WorkoutPlans.Include(w => w.WorkoutPlanItems)
                .Where(w => w.MemberId == id).ToList();
            foreach (var wp in workoutPlans)
                if (wp.WorkoutPlanItems.Any()) _context.WorkoutPlanItems.RemoveRange(wp.WorkoutPlanItems);
            if (workoutPlans.Any()) _context.WorkoutPlans.RemoveRange(workoutPlans);

            var dietPlans = _context.DietPlans.Include(d => d.DietPlanItems)
                .Where(d => d.MemberId == id).ToList();
            foreach (var dp in dietPlans)
                if (dp.DietPlanItems.Any()) _context.DietPlanItems.RemoveRange(dp.DietPlanItems);
            if (dietPlans.Any()) _context.DietPlans.RemoveRange(dietPlans);

            _context.Members.Remove(member);

            if (member.User != null)
            {
                bool otherMemberExists = _context.Members
                    .Any(m => m.UserId == member.User.UserId && m.MemberId != member.MemberId);

                if (!otherMemberExists)
                    _context.Users.Remove(member.User);
            }

            _context.SaveChanges();
            return RedirectToAction("MemberList");
        }

        // ---------------- Cancel Membership ----------------
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var member = _context.Members.FirstOrDefault(m => m.MemberId == id);
            if (member == null)
                return NotFound();

            if (member.Status == "Canceled")
            {
                TempData["SuccessMessage"] = "This membership is already canceled.";
                return RedirectToAction("MemberList");
            }

            member.Status = "Canceled";
            member.PaidStatus = "Unpaid";
            member.CancellationDate = DateOnly.FromDateTime(DateTime.Now);

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Membership for {member.Fname} {member.Lname} has been canceled on {member.CancellationDate}.";
            return RedirectToAction("MemberList");
        }

        // ---------------- Helpers ----------------
        private DateOnly CalculateDueDate(string membershipPeriod, string? selectedAccessType, DateOnly? startDate, out string membershipType, out string accessType)
        {
            DateOnly baseDate = startDate ?? DateOnly.FromDateTime(DateTime.Now); // ✅ Use Start Date (DateJoined)

            switch (membershipPeriod)
            {
                case "1 Month":
                    membershipType = "Regular";
                    accessType = selectedAccessType ?? "Weights";
                    return baseDate.AddMonths(1);

                case "3 Months":
                    membershipType = "Premium";
                    accessType = "Weights and Cardio";
                    return baseDate.AddMonths(3);

                case "1 Year":
                    membershipType = "Premium";
                    accessType = "Weights and Cardio";
                    return baseDate.AddYears(1);

                default:
                    membershipType = "Regular";
                    accessType = selectedAccessType ?? "Weights";
                    return baseDate;
            }
        }

        private string GenerateSecurePassword(int length = 12)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return new string(bytes.Select(b => valid[b % valid.Length]).ToArray());
        }

        private decimal ConvertHeight(decimal value, string unit)
        {
            return unit switch
            {
                "cm" => value / 100m,
                "ft" => value * 0.3048m,
                "in" => value * 0.0254m,
                _ => value
            };
        }

        private decimal ConvertWeight(decimal value, string unit)
        {
            return unit switch
            {
                "lbs" => value * 0.453592m,
                "g" => value / 1000m,
                _ => value
            };
        }
    }
}
