namespace Inventory_SalesManagement.Models
{
    public class DashboardViewModel
    {
        // Summary Cards
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }

        // For Doughnut Chart (Product Name, Qty Sold)
        public List<string> ProductLabels { get; set; }
        public List<int> ProductData { get; set; }

        // For Line Chart (Invoice #, Total Amount)
        public List<string> OrderLabels { get; set; }
        public List<decimal> OrderRevenueData { get; set; }

        // For "Low Stock" List
        public List<Entities.Product> LowStockProducts { get; set; }
    }
}