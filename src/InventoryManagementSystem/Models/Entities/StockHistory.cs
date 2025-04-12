using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.Models.Entities.Base;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Extensions;

namespace InventoryManagementSystem.Models.Entities
{
    /// <summary>
    /// Entity for tracking stock level changes
    /// </summary>
    public class StockHistory : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int QuantityChange { get; set; }

        [Required]
        public int PreviousStock { get; set; }

        [Required]
        public int NewStock { get; set; }

        [Required]
        [StringLength(100)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? ReferenceNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public TransactionType Type { get; set; } = TransactionType.StockIn;
        
        // String representation of the transaction type (for legacy code)
        [NotMapped]
        public string TypeString
        {
            get => Type.ToDisplayString();
            set => Type = value.ToTransactionType();
        }

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
} 