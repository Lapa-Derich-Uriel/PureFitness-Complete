using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class EquipmentScheduleViewModel
    {
        public EquipmentSchedule? EquipmentSchedule { get; set; }  // for create/edit

        public List<EquipmentItem> EquipmentItems { get; set; } = new List<EquipmentItem>();
        public List<Staff> Staffs { get; set; } = new List<Staff>();
        public List<EquipmentSchedule> EquipmentSchedules { get; set; } = new List<EquipmentSchedule>();
        public List<EquipmentSchedule> ItemSchedules { get; internal set; } = new List<EquipmentSchedule>();
    }
}
