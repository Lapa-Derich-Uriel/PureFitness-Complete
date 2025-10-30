using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class AttendanceViewModel
    {
        public Attendance? attendance { get; set; }

        public List<Member>? Members { get; set; }
    }
}
