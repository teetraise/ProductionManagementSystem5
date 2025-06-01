using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;

namespace ProductionManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/calculate")]
    public class CalculateApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CalculateApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST /api/calculate/production
        [HttpPost("production")]
        public async Task<ActionResult<object>> CalculateProduction([FromBody] CalculateProductionDto dto)
        {
            var product = await _context.Products.FindAsync(dto.product_id);
            if (product == null)
            {
                return BadRequest("Product not found");
            }

            // Получаем доступные линии
            var availableLines = await _context.ProductionLines
                .Where(l => l.Status == "Active" && l.CurrentWorkOrderId == null)
                .ToListAsync();

            var calculations = new List<object>();

            foreach (var line in availableLines)
            {
                var totalMinutes = (product.ProductionTimePerUnit * dto.quantity) / line.EfficiencyFactor;
                calculations.Add(new
                {
                    LineId = line.Id,
                    LineName = line.Name,
                    EfficiencyFactor = line.EfficiencyFactor,
                    TotalMinutes = Math.Round(totalMinutes, 2),
                    TotalHours = Math.Round(totalMinutes / 60, 2),
                    EstimatedEndDate = DateTime.UtcNow.AddMinutes(totalMinutes)
                });
            }

            return Ok(new
            {
                ProductId = dto.product_id,
                ProductName = product.Name,
                Quantity = dto.quantity,
                ProductionTimePerUnit = product.ProductionTimePerUnit,
                LineCalculations = calculations
            });
        }
    }

    public class CalculateProductionDto
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
    }
}