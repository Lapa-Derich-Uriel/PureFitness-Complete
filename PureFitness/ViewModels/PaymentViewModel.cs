using PureFitness.Models;
using System.Collections.Generic;

namespace PureFitness.ViewModels
{
    public class PaymentViewModel
    {
        public Payment payment { get; set; } = new Payment();

        public List<Payment> Payments { get; set; } = new List<Payment>();
        public List<Member> Members { get; set; } = new List<Member>();
        public List<Staff> Staffs { get; set; } = new List<Staff>();

        public List<Inventory> Products { get; set; } = new List<Inventory>();

        // Extra display fields
        public string? MemberName { get; set; }
        public string? MembershipType { get; set; }
        public string? AccessType { get; set; }
        public string? Items { get; set; }
        public string? LoggedInStaffName { get; set; }
        public int LoggedInStaffId { get; set; }
        public string? MembershipPeriod { get; set; }
    }
}
