using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PureFitness.Context;
using PureFitness.Models;
using PureFitness.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PureFitness.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly MyDBContext _context;

        public AttendanceController(MyDBContext context)
        {
            _context = context;
        }

        // --------------------------
        // LIST VIEW
        // --------------------------
        public async Task<IActionResult> AttendanceList()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";

            IQueryable<Attendance> query = _context.Attendances
                .Include(a => a.Member)
                .Include(a => a.Receipt); // ✅ include payment to access WalkInName

            if (role == "Member")
            {
                // Get current member info
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.User!.Username == username);

                if (member == null)
                    return Unauthorized("Member account not found.");

                // Show only that member's attendance
                query = query.Where(a => a.MemberId == member.MemberId);
            }
            else
            {
                // --------------------------
                // Auto-generate "Absent" for active members
                // --------------------------
                var activeMembers = await _context.Members
                    .Where(m => m.Status == "Active")
                    .ToListAsync();

                foreach (var member in activeMembers)
                {
                    bool hasAttendance = await _context.Attendances
                        .AnyAsync(a => a.MemberId == member.MemberId && a.AttendanceDate == today);

                    if (!hasAttendance)
                    {
                        _context.Attendances.Add(new Attendance
                        {
                            MemberId = member.MemberId,
                            AttendanceDate = today,
                            AttendanceStatus = "Absent"
                        });
                    }
                }

                // --------------------------
                // Add Walk-In attendance records (based on today's payments)
                // --------------------------
                var walkInsToday = await _context.Payments
                    .Where(p => p.PaymentType == "Walk-In" &&
                                p.TransactionDate == DateOnly.FromDateTime(DateTime.Now.Date))
                    .ToListAsync();

                foreach (var pay in walkInsToday)
                {
                    bool hasWalkInAttendance = await _context.Attendances
                        .AnyAsync(a => a.ReceiptId == pay.ReceiptId && a.AttendanceDate == today);

                    if (!hasWalkInAttendance)
                    {
                        _context.Attendances.Add(new Attendance
                        {
                            ReceiptId = pay.ReceiptId,       // link to Payment
                            AttendanceDate = today,
                            AttendanceStatus = "Present",
                            TimeIn = TimeOnly.FromDateTime(DateTime.Now)
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // Show today’s attendance for all members & walk-ins
                query = query.Where(a => a.AttendanceDate == today);
            }

            var attendanceList = await query
                .OrderByDescending(a => a.AttendanceId)
                .ToListAsync();

            ViewBag.Role = role;
            return View(attendanceList);
        }

        // --------------------------
        // TIME IN
        // --------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimeIn(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Member)
                .FirstOrDefaultAsync(a => a.AttendanceId == id);

            if (attendance != null && attendance.TimeIn == null)
            {
                attendance.TimeIn = TimeOnly.FromDateTime(DateTime.Now);
                attendance.AttendanceStatus = "Present";

                // ✅ Add Activity Points when member clocks in
                if (attendance.Member != null)
                {
                    const int POINTS_PER_ATTENDANCE = 10; // <-- adjust as needed
                    attendance.Member.ActivityPoints += POINTS_PER_ATTENDANCE;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AttendanceList));
        }

        // --------------------------
        // TIME OUT
        // --------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimeOut(int id)
        {
            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.AttendanceId == id);
            if (attendance != null && attendance.TimeOut == null)
            {
                var now = DateTime.Now;

                // ✅ Ensure AttendanceDate is always set
                if (!attendance.AttendanceDate.HasValue)
                    attendance.AttendanceDate = DateOnly.FromDateTime(now);

                attendance.TimeOut = TimeOnly.FromDateTime(now);

                // ✅ Build DateTimes safely
                DateTime startDt = attendance.AttendanceDate.Value.ToDateTime(
                    attendance.TimeIn ?? TimeOnly.FromDateTime(now)
                );
                DateTime endDt = attendance.AttendanceDate.Value.ToDateTime(attendance.TimeOut.Value);

                if (endDt < startDt)
                    endDt = endDt.AddDays(1);

                var minutes = (int)Math.Round((endDt - startDt).TotalMinutes);
                attendance.TimeSpent = Math.Max(0, minutes);

                attendance.AttendanceStatus = "Present";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(AttendanceList));
        }

    }
}
