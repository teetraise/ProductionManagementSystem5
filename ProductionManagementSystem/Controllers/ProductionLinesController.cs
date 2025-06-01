using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;

namespace ProductionManagementSystem.Controllers
{
    public class ProductionLinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductionLinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productionLines = await _context.ProductionLines
                .Include(pl => pl.CurrentWorkOrder)
                    .ThenInclude(wo => wo.Product)
                .ToListAsync();
            return View(productionLines);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionLine productionLine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productionLine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productionLine);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line != null)
            {
                line.Status = line.Status == "Active" ? "Stopped" : "Active";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEfficiency(int id, float efficiency)
        {
            var line = await _context.ProductionLines.FindAsync(id);
            if (line != null && efficiency >= 0.5f && efficiency <= 2.0f)
            {
                line.EfficiencyFactor = efficiency;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}