using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<ProductMaterial> ProductMaterials { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка составного ключа для ProductMaterial
            modelBuilder.Entity<ProductMaterial>()
                .HasKey(pm => new { pm.ProductId, pm.MaterialId });

            // Настройка связей
            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMaterials)
                .HasForeignKey(pm => pm.ProductId);

            modelBuilder.Entity<ProductMaterial>()
                .HasOne(pm => pm.Material)
                .WithMany(m => m.ProductMaterials)
                .HasForeignKey(pm => pm.MaterialId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Product)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(wo => wo.ProductId);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.ProductionLine)
                .WithMany(pl => pl.WorkOrders)
                .HasForeignKey(wo => wo.ProductionLineId);

            modelBuilder.Entity<ProductionLine>()
                .HasOne(pl => pl.CurrentWorkOrder)
                .WithOne()
                .HasForeignKey<ProductionLine>(pl => pl.CurrentWorkOrderId);

            base.OnModelCreating(modelBuilder);
        }
    }
}