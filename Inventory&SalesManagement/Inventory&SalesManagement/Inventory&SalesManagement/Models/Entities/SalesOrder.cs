namespace Inventory_SalesManagement.Models.Entities
{
    public class SalesOrder
    {
        public List<SalesOrderManagement> Items { get; set; } = new List<SalesOrderManagement>();
    }
}
