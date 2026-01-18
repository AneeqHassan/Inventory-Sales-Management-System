using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory_SalesManagement.Models;
using Inventory_SalesManagement.Models.Entities;

public class HomeController : Controller
{
    private readonly DatabaseContext _context;

    public HomeController(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // --- 1. SECURITY CHECK (Add this block) ---
        // If the user is not signed in, kick them to the Login page immediately.
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Auth");
        }

        // --- 2. DASHBOARD LOGIC (Keep existing code) ---
        var products = await _context.Products.ToListAsync();
        var sales = await _context.SalesOrderManagements.Include(s => s.Product).ToListAsync();

        var model = new Dashboard
        {
            TotalProducts = products.Count,
            LowStockCount = products.Count(p => p.StockQuantity < 5),
            TotalOrders = sales.Select(s => s.OrderNumber).Distinct().Count(),
            TotalRevenue = sales.Sum(s => s.TotalAmount),

            // Low Stock List
            LowStockProducts = products
                .Where(p => p.StockQuantity < 5)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToList()
        };

        // Prepare Charts
        var topProducts = sales
            .GroupBy(s => s.Product?.Name ?? "Unknown")
            .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToList();

        model.ProductLabels = topProducts.Select(x => x.Name).ToList();
        model.ProductData = topProducts.Select(x => x.Qty).ToList();

        var recentOrders = sales
            .GroupBy(s => s.OrderNumber)
            .Select(g => new
            {
                Invoice = g.Key,
                Total = g.Sum(x => x.TotalAmount),
                Date = g.First().OrderDate
            })
            .OrderByDescending(x => x.Date)
            .Take(10)
            .OrderBy(x => x.Date)
            .ToList();

        model.OrderLabels = recentOrders.Select(x => x.Invoice).ToList();
        model.OrderRevenueData = recentOrders.Select(x => x.Total).ToList();

        return View(model);
    }
}