using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_SalesManagement.Models.Entities
{
    [Table("Orders")]
    public class SalesOrderManagement
    {
        [Key]
        public int Id { get; set; }

        // --- Order Info (Formerly SalesOrder) ---
        [Display(Name = "Invoice Number")]
        public string? OrderNumber { get; set; } // e.g., INV-001

        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Sold By")]
        public string? SalesPersonId { get; set; } // The logged-in user

        // --- Product Info (Formerly OrderItem) ---
        // We link to the Product table so we know what was sold
        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        // We keep the navigation property so we can access Product.Name later
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
    }
}
