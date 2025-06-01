using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class ProductionLine
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Status { get; set; } = "Stopped"; // "Active" or "Stopped"

        public float EfficiencyFactor { get; set; } = 1.0f; // 0.5-2.0

        public int? CurrentWorkOrderId { get; set; }

        // Navigation properties
        public virtual WorkOrder? CurrentWorkOrder { get; set; }
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}