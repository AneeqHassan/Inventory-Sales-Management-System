using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_SalesManagement.Models.Entities
{
    [Table("Supplier")]
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("supplier_id")]
        public int Id { get; set; }
        [Column("supplier_name")]
        public string? Name { get; set; }
        [Column("supplier_rmail")]
        public string? Email { get; set; }
        [Column("supplier_phone")]
        public string? Phone { get; set; }
        [Column("supplier_address")]
        public string? Address { get; set; }
    }
}

