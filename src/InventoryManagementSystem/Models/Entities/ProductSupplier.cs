using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Entities
{
    public class ProductSupplier : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Supplier price must be greater than or equal to 0")]
        public decimal UnitPrice { get; set; }

        [StringLength(50)]
        public string? SupplierSKU { get; set; }

        [Range(0, int.MaxValue)]
        public int LeadTimeDays { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumOrderQuantity { get; set; }

        public bool IsPreferredSupplier { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; } = null!;
        public virtual Supplier Supplier { get; set; } = null!;

        // Adding SupplierPrice for backward compatibility
        public decimal SupplierPrice
        {
            get => UnitPrice;
            set => UnitPrice = value;
        }

        // Adding IsPreferred for backward compatibility
        public bool IsPreferred
        {
            get => IsPreferredSupplier;
            set => IsPreferredSupplier = value;
        }
    }
} 