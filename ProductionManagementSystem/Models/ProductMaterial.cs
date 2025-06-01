namespace ProductionManagementSystem.Models
{
    public class ProductMaterial
    {
        public int ProductId { get; set; }
        public int MaterialId { get; set; }
        public decimal QuantityNeeded { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual Material Material { get; set; }
    }
}