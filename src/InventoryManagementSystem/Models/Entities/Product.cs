using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.Models.Entities.Base;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Models.Entities;

public class Product : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal StockLevel { get; set; }

    [Required]
    public int MinimumStockLevel { get; set; }

    [Required]
    public int MaximumStockLevel { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();

    public virtual ICollection<StockHistory> StockHistory { get; set; } = new List<StockHistory>();

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string UnitOfMeasurement { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Cost must be greater than or equal to 0")]
    public decimal Cost { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be greater than or equal to 0")]
    public int ReorderLevel { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Current stock must be greater than or equal to 0")]
    public int CurrentStock { get; set; }

    [StringLength(255)]
    public string? ImagePath { get; set; }

    [StringLength(50)]
    public string? Barcode { get; set; }
}