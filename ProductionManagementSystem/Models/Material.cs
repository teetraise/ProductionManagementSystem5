using System.ComponentModel.DataAnnotations;

namespace ProductionManagementSystem.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal Quantity { get; set; }

        public string UnitOfMeasure { get; set; }

        public decimal MinimalStock { get; set; }

        // Navigation properties
        public virtual ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    }
}