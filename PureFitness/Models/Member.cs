using System;
using System.Collections.Generic;

namespace PureFitness.Models;

public partial class Member
{
    public int MemberId { get; set; }

    public string? Fname { get; set; }

    public string? Lname { get; set; }

    public string? Gender { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? MembershipType { get; set; }

    public string? AccessType { get; set; }

    public string? MembershipPeriod { get; set; }

    public string? FitnessGoal { get; set; }

    public int Age { get; set; }

    public decimal Height { get; set; }

    public decimal Weight { get; set; }

    public decimal Bmi { get; set; }

    public DateOnly? DateJoined { get; set; }

    public DateOnly? DueDate { get; set; }

    public int? UserId { get; set; }

    public string? Status { get; set; }

    public string? PaidStatus { get; set; }

    public int? StaffId { get; set; }

    public DateOnly? CancellationDate { get; set; }

    public int? ActivityPoints { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<DietPlan> DietPlans { get; set; } = new List<DietPlan>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Staff? Staff { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
}
