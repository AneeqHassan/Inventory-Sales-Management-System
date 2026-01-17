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
        // 1. Fetch raw data needed
        var products = await _context.Products.ToListAsync();
        var sales = await _context.SalesOrderManagements.Include(s => s.Product).ToListAsync();

        // 2. Calculate Summary Metrics
        var model = new DashboardViewModel
        {
            TotalProducts = products.Count,
            LowStockCount = products.Count(p => p.StockQuantity < 5),
            TotalOrders = sales.Select(s => s.OrderNumber).Distinct().Count(), // Count unique invoices
            TotalRevenue = sales.Sum(s => s.TotalAmount),

            // 3. Low Stock List (Top 5 items running low)
            LowStockProducts = products
                .Where(p => p.StockQuantity < 5)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToList()
        };

        // 4. PREPARE DOUGHNUT CHART (Top 5 Selling Products by Qty)
        // Group sales by Product Name -> Sum the Quantity
        var topProducts = sales
            .GroupBy(s => s.Product?.Name ?? "Unknown")
            .Select(g => new { Name = g.Key, Qty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToList();

        model.ProductLabels = topProducts.Select(x => x.Name).ToList();
        model.ProductData = topProducts.Select(x => x.Qty).ToList();

        // 5. PREPARE LINE CHART (Revenue per Order - Last 10 Orders)
        // Group by OrderNumber (Invoice) -> Sum the TotalAmount
        var recentOrders = sales
            .GroupBy(s => s.OrderNumber)
            .Select(g => new {
                Invoice = g.Key,
                Total = g.Sum(x => x.TotalAmount),
                Date = g.First().OrderDate
            })
            .OrderByDescending(x => x.Date) // Newest first
            .Take(10)
            .OrderBy(x => x.Date) // Re-sort for the chart (Oldest -> Newest left to right)
            .ToList();

        model.OrderLabels = recentOrders.Select(x => x.Invoice).ToList();
        model.OrderRevenueData = recentOrders.Select(x => x.Total).ToList();

        return View(model);
    }
}