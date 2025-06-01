using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/lines")]
    public class LinesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LinesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/lines?available=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionLine>>> GetLines([FromQuery] bool? available = null)
        {
            var query = _context.ProductionLines.AsQueryable();

            if (available == true)
            {
                query = query.Where(l => l.Status == "Active" && l.CurrentWorkOrderId == null);
            }

            return await query.Include(l => l.CurrentWorkOrder)
                           .ThenInclude(wo => wo.Product)
                           .ToListAsync();
        }

        // PUT /api/lines/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line == null)
            {
                return NotFound();
            }

            line.Status = dto.status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/lines/{id}/schedule
        [HttpGet("{id}/schedule")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedule(int id)
        {
            var workOrders = await _context.WorkOrders
                .Where(wo => wo.ProductionLineId == id && wo.Status != "Completed" && wo.Status != "Cancelled")
                .Include(wo => wo.Product)
                .OrderBy(wo => wo.StartDate)
                .Select(wo => new
                {
                    Id = wo.Id,
                    ProductName = wo.Product.Name,
                    Quantity = wo.Quantity,
                    StartDate = wo.StartDate,
                    EstimatedEndDate = wo.EstimatedEndDate,
                    Status = wo.Status,
                    Progress = wo.Progress
                })
                .ToListAsync();

            return Ok(workOrders);
        }
        // POST /api/lines
        [HttpPost]
        public async Task<ActionResult<ProductionLine>> PostLine([FromBody] CreateLineDto dto)
        {
            var line = new ProductionLine
            {
                Name = dto.name,
                EfficiencyFactor = dto.efficiencyFactor,
                Status = dto.status
            };

            _context.ProductionLines.Add(line);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLines), new { id = line.Id }, line);
        }

        // Добавьте DTO класс в конец файла
        public class CreateLineDto
        {
            public string name { get; set; }
            public float efficiencyFactor { get; set; }
            public string status { get; set; }
        }
    }

    public class UpdateStatusDto
    {
        public string status { get; set; }
    }
}
