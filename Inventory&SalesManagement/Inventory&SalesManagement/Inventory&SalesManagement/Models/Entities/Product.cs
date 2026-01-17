using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_SalesManagement.Models.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("product_id")]
        public int Id { get; set; }
        [Column("product_name")]
        public string? Name { get; set; }

        // For your "Barcode input" feature
        [Column("product_barcode")]
        public string? Barcode { get; set; }
        [Column("product_price")]
        public decimal Price { get; set; }

        // For "Stock update" and "Low-stock alerts"
        [Column("product_qty")]
        public int StockQuantity { get; set; }

        // Relationship: A product belongs to a Supplier
        [Column("supplier_id")]
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
