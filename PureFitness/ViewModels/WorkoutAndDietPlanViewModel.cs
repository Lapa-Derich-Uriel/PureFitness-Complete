using PureFitness.Models;
namespace PureFitness.ViewModels
{
    public class WorkoutAndDietPlanViewModel
    {
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        // Assigned trainer (from Member.StaffId)
        public int? StaffId { get; set; }
        public Staff? Staff { get; set; }

        public WorkoutPlan? WorkoutPlan { get; set; }
        public DietPlan? DietPlan { get; set; }

        // Collections only — remove the duplicates
        public List<WorkoutPlanItem>? WorkoutPlanItems { get; set; }
        public List<DietPlanItem>? DietPlanItems { get; set; }
    }
}
