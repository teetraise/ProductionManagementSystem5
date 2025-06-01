using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class WorkOrder
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int? ProductionLineId { get; set; }

        public int Quantity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EstimatedEndDate { get; set; }

        public string Status { get; set; } = "Pending"; // "Pending", "InProgress", "Completed", "Cancelled"

        public int Progress { get; set; } = 0; // Процент выполнения

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual ProductionLine? ProductionLine { get; set; }
    }
}