using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? Specifications { get; set; } // JSON format

        public string? Category { get; set; }

        public int MinimalStock { get; set; }

        public int ProductionTimePerUnit { get; set; } // в минутах

        // Navigation properties
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
        public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}