using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var workOrders = await _context.WorkOrders
                .Include(w => w.Product)
                .Include(w => w.ProductionLine)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
            return View(workOrders);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name");
            ViewBag.ProductionLines = new SelectList(
                await _context.ProductionLines.Where(pl => pl.Status == "Active").ToListAsync(),
                "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkOrder workOrder)
        {
            if (ModelState.IsValid)
            {
                // Получаем информацию о продукте и линии для расчета времени
                var product = await _context.Products.FindAsync(workOrder.ProductId);
                var productionLine = await _context.ProductionLines.FindAsync(workOrder.ProductionLineId);

                if (product != null && productionLine != null)
                {
                    // Проверяем наличие материалов
                    var requiredMaterials = await _context.ProductMaterials
                        .Where(pm => pm.ProductId == workOrder.ProductId)
                        .Include(pm => pm.Material)
                        .ToListAsync();

                    bool hasEnoughMaterials = true;
                    foreach (var pm in requiredMaterials)
                    {
                        var totalNeeded = pm.QuantityNeeded * workOrder.Quantity;
                        if (pm.Material.Quantity < totalNeeded)
                        {
                            ModelState.AddModelError("", $"Недостаточно материала: {pm.Material.Name}. Требуется: {totalNeeded}, Доступно: {pm.Material.Quantity}");
                            hasEnoughMaterials = false;
                        }
                    }

                    if (hasEnoughMaterials)
                    {
                        // Расчет времени производства
                        var totalProductionTime = (product.ProductionTimePerUnit * workOrder.Quantity) / productionLine.EfficiencyFactor;
                        workOrder.EstimatedEndDate = workOrder.StartDate.AddMinutes(totalProductionTime);

                        _context.Add(workOrder);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            ViewBag.Products = new SelectList(await _context.Products.ToListAsync(), "Id", "Name", workOrder.ProductId);
            ViewBag.ProductionLines = new SelectList(
                await _context.ProductionLines.Where(pl => pl.Status == "Active").ToListAsync(),
                "Id", "Name", workOrder.ProductionLineId);
            return View(workOrder);
        }

        [HttpPost]
        public async Task<IActionResult> StartProduction(int id)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (workOrder != null && workOrder.Status == "Pending")
            {
                workOrder.Status = "InProgress";
                if (workOrder.ProductionLine != null)
                {
                    workOrder.ProductionLine.CurrentWorkOrderId = workOrder.Id;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.ProductionLine)
                .FirstOrDefaultAsync(wo => wo.Id == id);

            if (workOrder != null)
            {
                workOrder.Status = "Cancelled";
                if (workOrder.ProductionLine != null)
                {
                    workOrder.ProductionLine.CurrentWorkOrderId = null;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
