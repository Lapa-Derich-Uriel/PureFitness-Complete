using PureFitness.Models;

namespace PureFitness.ViewModels
{
    public class EquipmentLogsViewModel
    {
        public List<Equipment>? Equipments { get; set; }
        public List<EquipmentItem>? EquipmentItems { get; set; }
        public List<EquipmentLog>? EquipmentLogs { get; set; }
    }
}
