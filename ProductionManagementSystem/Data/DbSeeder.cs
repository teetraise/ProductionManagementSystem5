using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Проверяем, есть ли уже данные
            if (context.Materials.Any() || context.Products.Any() || context.ProductionLines.Any())
            {
                return; // База данных уже заполнена
            }

            // Добавляем материалы
            var aluminum = new Material
            {
                Name = "Алюминий",
                Quantity = 500,
                UnitOfMeasure = "кг",
                MinimalStock = 50
            };

            var steel = new Material
            {
                Name = "Сталь",
                Quantity = 300,
                UnitOfMeasure = "кг",
                MinimalStock = 30
            };

            var plastic = new Material
            {
                Name = "Пластик",
                Quantity = 200,
                UnitOfMeasure = "кг",
                MinimalStock = 25
            };

            var electronics = new Material
            {
                Name = "Электронные компоненты",
                Quantity = 1000,
                UnitOfMeasure = "шт",
                MinimalStock = 100
            };

            var paint = new Material
            {
                Name = "Краска",
                Quantity = 50,
                UnitOfMeasure = "литр",
                MinimalStock = 10
            };

            var rubber = new Material
            {
                Name = "Резина",
                Quantity = 80,
                UnitOfMeasure = "кг",
                MinimalStock = 15
            };

            var glass = new Material
            {
                Name = "Стекло",
                Quantity = 120,
                UnitOfMeasure = "кг",
                MinimalStock = 20
            };

            var materials = new List<Material> { aluminum, steel, plastic, electronics, paint, rubber, glass };
            context.Materials.AddRange(materials);
            await context.SaveChangesAsync();

            // Добавляем производственные линии
            var lineA = new ProductionLine
            {
                Name = "Линия сборки А",
                Status = "Active",
                EfficiencyFactor = 1.2f
            };

            var lineB = new ProductionLine
            {
                Name = "Линия сборки Б",
                Status = "Active",
                EfficiencyFactor = 1.0f
            };

            var linePaint = new ProductionLine
            {
                Name = "Линия покраски",
                Status = "Active",
                EfficiencyFactor = 0.8f
            };

            var linePacking = new ProductionLine
            {
                Name = "Линия упаковки",
                Status = "Stopped",
                EfficiencyFactor = 1.5f
            };

            var productionLines = new List<ProductionLine> { lineA, lineB, linePaint, linePacking };
            context.ProductionLines.AddRange(productionLines);
            await context.SaveChangesAsync();

            // Добавляем продукты
            var carPart = new Product
            {
                Name = "Автомобильная деталь А1",
                Description = "Основная деталь для автомобильной промышленности",
                Category = "Автомобильные",
                ProductionTimePerUnit = 45,
                MinimalStock = 10,
                Specifications = "{\"weight\": \"2.5kg\", \"material\": \"aluminum\", \"color\": \"silver\"}"
            };

            var electronicBlock = new Product
            {
                Name = "Электронный блок Е1",
                Description = "Управляющий электронный блок",
                Category = "Электроника",
                ProductionTimePerUnit = 120,
                MinimalStock = 5,
                Specifications = "{\"voltage\": \"12V\", \"power\": \"100W\", \"size\": \"10x15x5cm\"}"
            };

            var plasticCase = new Product
            {
                Name = "Пластиковый корпус П1",
                Description = "Защитный корпус из высокопрочного пластика",
                Category = "Пластиковые изделия",
                ProductionTimePerUnit = 30,
                MinimalStock = 20,
                Specifications = "{\"material\": \"ABS plastic\", \"color\": \"black\", \"weight\": \"0.5kg\"}"
            };

            var glassPanel = new Product
            {
                Name = "Стеклянная панель С1",
                Description = "Прозрачная защитная панель",
                Category = "Стеклянные изделия",
                ProductionTimePerUnit = 60,
                MinimalStock = 15,
                Specifications = "{\"thickness\": \"5mm\", \"type\": \"tempered glass\", \"transparency\": \"95%\"}"
            };

            var rubberSeal = new Product
            {
                Name = "Резиновое уплотнение Р1",
                Description = "Уплотнительное кольцо",
                Category = "Резиновые изделия",
                ProductionTimePerUnit = 15,
                MinimalStock = 50,
                Specifications = "{\"diameter\": \"50mm\", \"thickness\": \"3mm\", \"hardness\": \"70 Shore A\"}"
            };

            var products = new List<Product> { carPart, electronicBlock, plasticCase, glassPanel, rubberSeal };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Добавляем связи продуктов с материалами (используем реальные ID)
            var productMaterials = new List<ProductMaterial>
            {
                // Автомобильная деталь А1
                new ProductMaterial { ProductId = carPart.Id, MaterialId = aluminum.Id, QuantityNeeded = 2.5m },
                new ProductMaterial { ProductId = carPart.Id, MaterialId = paint.Id, QuantityNeeded = 0.1m },
                
                // Электронный блок Е1
                new ProductMaterial { ProductId = electronicBlock.Id, MaterialId = electronics.Id, QuantityNeeded = 15 },
                new ProductMaterial { ProductId = electronicBlock.Id, MaterialId = plastic.Id, QuantityNeeded = 0.3m },
                
                // Пластиковый корпус П1
                new ProductMaterial { ProductId = plasticCase.Id, MaterialId = plastic.Id, QuantityNeeded = 0.5m },
                new ProductMaterial { ProductId = plasticCase.Id, MaterialId = paint.Id, QuantityNeeded = 0.05m },
                
                // Стеклянная панель С1
                new ProductMaterial { ProductId = glassPanel.Id, MaterialId = glass.Id, QuantityNeeded = 1.2m },
                
                // Резиновое уплотнение Р1
                new ProductMaterial { ProductId = rubberSeal.Id, MaterialId = rubber.Id, QuantityNeeded = 0.1m }
            };

            context.ProductMaterials.AddRange(productMaterials);
            await context.SaveChangesAsync();

            // Добавляем несколько тестовых заказов
            var workOrders = new List<WorkOrder>
            {
                new WorkOrder
                {
                    ProductId = carPart.Id,
                    ProductionLineId = lineA.Id,
                    Quantity = 10,
                    StartDate = DateTime.UtcNow,
                    EstimatedEndDate = DateTime.UtcNow.AddHours(6),
                    Status = "Pending"
                },
                new WorkOrder
                {
                    ProductId = electronicBlock.Id,
                    ProductionLineId = lineB.Id,
                    Quantity = 5,
                    StartDate = DateTime.UtcNow.AddHours(1),
                    EstimatedEndDate = DateTime.UtcNow.AddHours(11),
                    Status = "Pending"
                },
                new WorkOrder
                {
                    ProductId = plasticCase.Id,
                    ProductionLineId = lineA.Id,
                    Quantity = 20,
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EstimatedEndDate = DateTime.UtcNow.AddDays(1).AddHours(10),
                    Status = "Pending"
                },
                new WorkOrder
                {
                    ProductId = rubberSeal.Id,
                    ProductionLineId = lineB.Id,
                    Quantity = 100,
                    StartDate = DateTime.UtcNow.AddHours(-2),
                    EstimatedEndDate = DateTime.UtcNow.AddHours(23),
                    Status = "InProgress",
                    Progress = 35
                }
            };

            context.WorkOrders.AddRange(workOrders);
            await context.SaveChangesAsync();
        }
    }
}