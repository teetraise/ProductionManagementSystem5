using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/products?category={id}
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] string? category = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            return await query.ToListAsync();
        }

        // GET /api/products/{id}/materials
        [HttpGet("{id}/materials")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductMaterials(int id)
        {
            var productMaterials = await _context.ProductMaterials
                .Where(pm => pm.ProductId == id)
                .Include(pm => pm.Material)
                .Select(pm => new
                {
                    MaterialId = pm.MaterialId,
                    MaterialName = pm.Material.Name,
                    QuantityNeeded = pm.QuantityNeeded,
                    UnitOfMeasure = pm.Material.UnitOfMeasure,
                    AvailableQuantity = pm.Material.Quantity
                })
                .ToListAsync();

            return Ok(productMaterials);
        }

        // POST /api/products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromBody] CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.name,
                ProductionTimePerUnit = dto.prod_time,
                Category = dto.category
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
    }

    public class CreateProductDto
    {
        public string name { get; set; }
        public int prod_time { get; set; }
        public string category { get; set; }
    }
}
