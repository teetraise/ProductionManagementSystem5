using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers.Api
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/orders?status=active&date=today
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? date = null)
        {
            var query = _context.WorkOrders
                .Include(wo => wo.Product)
                .Include(wo => wo.ProductionLine)
                .AsQueryable();

            if (status == "active")
            {
                query = query.Where(wo => wo.Status == "InProgress" || wo.Status == "Pending");
            }
            else if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(wo => wo.Status == status);
            }

            if (date == "today")
            {
                var today = DateTime.Today;
                query = query.Where(wo => wo.StartDate.Date == today);
            }

            var orders = await query.Select(wo => new
            {
                Id = wo.Id,
                ProductName = wo.Product.Name,
                Quantity = wo.Quantity,
                StartDate = wo.StartDate,
                EstimatedEndDate = wo.EstimatedEndDate,
                Status = wo.Status,
                Progress = wo.Progress,
                ProductionLineName = wo.ProductionLine != null ? wo.ProductionLine.Name : null
            }).ToListAsync();

            return Ok(orders);
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<WorkOrder>> PostOrder([FromBody] CreateOrderDto dto)
        {
            var product = await _context.Products.FindAsync(dto.product_id);
            if (product == null)
            {
                return BadRequest("Product not found");
            }

            var productionLine = await _context.ProductionLines.FindAsync(dto.line_id);
            if (productionLine == null)
            {
                return BadRequest("Production line not found");
            }

            // Проверка материалов
            var requiredMaterials = await _context.ProductMaterials
                .Where(pm => pm.ProductId == dto.product_id)
                .Include(pm => pm.Material)
                .ToListAsync();

            foreach (var pm in requiredMaterials)
            {
                var totalNeeded = pm.QuantityNeeded * dto.quantity;
                if (pm.Material.Quantity < totalNeeded)
                {
                    return BadRequest($"Insufficient material: {pm.Material.Name}. Required: {totalNeeded}, Available: {pm.Material.Quantity}");
                }
            }

            // Расчет времени производства
            var totalProductionTime = (product.ProductionTimePerUnit * dto.quantity) / productionLine.EfficiencyFactor;
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMinutes(totalProductionTime);

            var workOrder = new WorkOrder
            {
                ProductId = dto.product_id,
                ProductionLineId = dto.line_id,
                Quantity = dto.quantity,
                StartDate = startDate,
                EstimatedEndDate = endDate,
                Status = "Pending"
            };

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrders), new { id = workOrder.Id }, workOrder);
        }

        // PUT /api/orders/{id}/progress
        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressDto dto)
        {
            var order = await _context.WorkOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Progress = dto.percent;

            if (dto.percent >= 100)
            {
                order.Status = "Completed";
            }
            else if (dto.percent > 0 && order.Status == "Pending")
            {
                order.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET /api/orders/{id}/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<object>> GetOrderDetails(int id)
        {
            var order = await _context.WorkOrders
                .Where(wo => wo.Id == id)
                .Include(wo => wo.Product)
                    .ThenInclude(p => p.ProductMaterials)
                    .ThenInclude(pm => pm.Material)
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = new
            {
                Id = order.Id,
                Product = new
                {
                    Id = order.Product.Id,
                    Name = order.Product.Name,
                    ProductionTimePerUnit = order.Product.ProductionTimePerUnit,
                    Materials = order.Product.ProductMaterials.Select(pm => new
                    {
                        MaterialName = pm.Material.Name,
                        QuantityNeeded = pm.QuantityNeeded,
                        TotalNeeded = pm.QuantityNeeded * order.Quantity,
                        UnitOfMeasure = pm.Material.UnitOfMeasure
                    })
                },
                Quantity = order.Quantity,
                StartDate = order.StartDate,
                EstimatedEndDate = order.EstimatedEndDate,
                Status = order.Status,
                Progress = order.Progress,
                ProductionLine = order.ProductionLine != null ? new
                {
                    Id = order.ProductionLine.Id,
                    Name = order.ProductionLine.Name,
                    EfficiencyFactor = order.ProductionLine.EfficiencyFactor
                } : null
            };

            return Ok(orderDetails);
        }
    }

    public class CreateOrderDto
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
        public int line_id { get; set; }
    }

    public class UpdateProgressDto
    {
        public int percent { get; set; }
    }
}