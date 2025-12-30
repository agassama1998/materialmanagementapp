using MaterialManagement.Data;
using MaterialManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MaterialManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard statistics
            ViewBag.TotalMaterials = await _context.Materials.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.LowStockItems = await _context.Materials
                .Where(m => m.Quantity <= m.MinimumQuantity)
                .CountAsync();
            ViewBag.TotalValue = await _context.Materials
                .SumAsync(m => m.Quantity * m.UnitPrice);

            // Recent materials
            var recentMaterials = await _context.Materials
                .Include(m => m.Category)
                .OrderByDescending(m => m.CreatedDate)
                .Take(5)
                .ToListAsync();

            return View(recentMaterials);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
