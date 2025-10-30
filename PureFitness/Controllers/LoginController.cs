using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PureFitness.Context;
using PureFitness.Models;
using PureFitness.ViewModels;
using PureFitness.Services; // ✅ Added for EmailService

namespace PureFitness.Controllers
{
    public class LoginController : Controller
    {
        private readonly MyDBContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly EmailService _emailService;

        private static Dictionary<string, (int Attempts, DateTime LockoutEnd)> _lockouts = new();
        private static Dictionary<string, string> _resetCodes = new(); // ✅ Store email-verification codes temporarily

        public LoginController(MyDBContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public IActionResult LoginView() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginViewModel LoginVM)
        {
            if (!ModelState.IsValid)
                return View("LoginView", LoginVM);

            var username = LoginVM.Username!.Trim();
            var password = LoginVM.Password!.Trim();

            // Lockout check
            if (_lockouts.TryGetValue(username, out var lockout) && lockout.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Account is temporarily locked. Try again later.");
                return View("LoginView", LoginVM);
            }

            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                RegisterFailedAttempt(username);
                ModelState.AddModelError("", "Invalid login");
                return View("LoginView", LoginVM);
            }

            bool isValid = false;

            if (!string.IsNullOrEmpty(user.Password))
            {
                if (user.Password.StartsWith("AQAAAA")) // hashed
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
                    isValid = result != PasswordVerificationResult.Failed;
                }
                else
                {
                    isValid = password == user.Password;
                }
            }

            if (!isValid)
            {
                RegisterFailedAttempt(username);
                ModelState.AddModelError("", "Invalid login");
                return View("LoginView", LoginVM);
            }

            if (_lockouts.ContainsKey(username))
                _lockouts.Remove(username);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username!),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "Member")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                });

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role?.RoleName ?? "Member");
            HttpContext.Session.SetString("Username", user.Username!);

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("LoginView", "Login");
        }

        // =============================
        // 🔹 FORGOT PASSWORD FLOW
        // =============================
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (user == null)
            {
                vm.ErrorMessage = "Email not found.";
                return View(vm);
            }

            // Validate password match
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                vm.ErrorMessage = "Passwords do not match.";
                return View(vm);
            }

            // ✅ Hash and update the password
            user.Password = _passwordHasher.HashPassword(user, vm.NewPassword!);
            await _context.SaveChangesAsync();

            // ✅ Send email notification with the new password
            string subject = "PureFitness Password Reset Successful";
            string message = $@"
        <p>Hi {user.Username},</p>
        <p>Your password has been successfully reset.</p>
        <p><strong>New Password:</strong> {vm.NewPassword}</p>
        <p>You can now log in to your account using this password.</p>
        <br/>
        <p>– PureFitness Team</p>";

            await _emailService.SendEmailAsync(user.Email!, subject, message);
             
            // ✅ Redirect to LoginView after success
            TempData["ResetSuccess"] = "Password has been reset successfully. Please check your email for details.";
            return RedirectToAction("LoginView", "Login");
        }

        private void RegisterFailedAttempt(string username)
        {
            if (!_lockouts.ContainsKey(username))
            {
                _lockouts[username] = (1, DateTime.MinValue);
                return;
            }

            var (attempts, lockoutEnd) = _lockouts[username];
            attempts++;

            if (attempts >= 5)
            {
                _lockouts[username] = (0, DateTime.UtcNow.AddMinutes(5));
            }
            else
            {
                _lockouts[username] = (attempts, DateTime.MinValue);
            }
        }
    }
}
