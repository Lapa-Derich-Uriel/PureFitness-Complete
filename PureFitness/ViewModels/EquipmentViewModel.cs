using System.ComponentModel.DataAnnotations.Schema;
using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class EquipmentViewModel
    {
        public Equipment? equipment { get; set; }

        public EquipmentItem? equipmentItem { get; set; }
        public List<Equipment>? Equipments { get; set; }
        public List<EquipmentItem>? EquipmentItems { get; set; }
        public List<EquipmentLog>? EquipmentLogs { get; set; }
        public List<Staff>? Staffs { get; set; }
        public EquipmentSchedule EquipmentSchedule { get; set; } = new();
        public List<EquipmentSchedule> EquipmentSchedules { get; set; } = new List<EquipmentSchedule>();

    }
}
