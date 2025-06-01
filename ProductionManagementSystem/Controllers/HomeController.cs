using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductionManagementSystem.Data;
using ProductionManagementSystem.Models;
using System.Diagnostics;

namespace ProductionManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalMaterials = await _context.Materials.CountAsync(),
                ActiveOrders = await _context.WorkOrders.CountAsync(wo => wo.Status == "InProgress" || wo.Status == "Pending"),
                LowStockMaterials = await _context.Materials.CountAsync(m => m.Quantity <= m.MinimalStock),
                RecentOrders = await _context.WorkOrders
                    .Include(wo => wo.Product)
                    .Include(wo => wo.ProductionLine)
                    .OrderByDescending(wo => wo.StartDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboardData);
        }

        public IActionResult Materials()
        {
            return View();
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Orders()
        {
            return View();
        }

        public IActionResult ProductionLines()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalMaterials { get; set; }
        public int ActiveOrders { get; set; }
        public int LowStockMaterials { get; set; }
        public List<WorkOrder> RecentOrders { get; set; } = new();
    }
}
