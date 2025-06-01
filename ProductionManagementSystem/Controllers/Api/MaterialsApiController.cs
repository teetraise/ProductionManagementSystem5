using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/materials")]
    public class MaterialsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaterialsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/materials?low_stock=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetMaterials([FromQuery] bool? low_stock = null)
        {
            var query = _context.Materials.AsQueryable();

            if (low_stock == true)
            {
                query = query.Where(m => m.Quantity <= m.MinimalStock);
            }

            return await query.ToListAsync();
        }

        // POST /api/materials
        [HttpPost]
        public async Task<ActionResult<Material>> PostMaterial([FromBody] CreateMaterialDto dto)
        {
            var material = new Material
            {
                Name = dto.name,
                Quantity = dto.quantity,
                UnitOfMeasure = dto.unit,
                MinimalStock = dto.min_stock
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMaterials), new { id = material.Id }, material);
        }

        // PUT /api/materials/{id}/stock
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            material.Quantity = dto.amount;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CreateMaterialDto
    {
        public string name { get; set; }
        public decimal quantity { get; set; }
        public string unit { get; set; }
        public decimal min_stock { get; set; }
    }

    public class UpdateStockDto
    {
        public decimal amount { get; set; }
    }
}