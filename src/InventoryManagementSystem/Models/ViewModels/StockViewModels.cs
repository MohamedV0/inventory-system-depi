using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Base class for stock movement operations
    /// </summary>
    public abstract class StockMovementViewModel
    {
        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [StringLength(100, ErrorMessage = "Reason cannot exceed 100 characters")]
        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Reference number cannot exceed 50 characters")]
        [Display(Name = "Reference Number")]
        public string? Reference { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// View model for adding stock to inventory
    /// </summary>
    public class AddStockViewModel : StockMovementViewModel
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity to Add")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0")]
        [DataType(DataType.Currency)]
        public decimal? UnitPrice { get; set; }
    }

    /// <summary>
    /// View model for removing stock from inventory
    /// </summary>
    public class RemoveStockViewModel : StockMovementViewModel
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        [Display(Name = "Quantity to Remove")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// View model for adjusting stock to match physical count
    /// </summary>
    public class AdjustStockViewModel : StockMovementViewModel
    {
        [Required(ErrorMessage = "New quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "New quantity must be greater than or equal to 0")]
        [Display(Name = "New Stock Level")]
        public int NewQuantity { get; set; }
    }

    /// <summary>
    /// View model for stock movement filters
    /// </summary>
    public class StockHistoryFilterViewModel
    {
        public int? ProductId { get; set; }
        
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }
        
        [Display(Name = "Type")]
        public string? MovementType { get; set; }
        
        public List<string> AvailableTypes { get; set; } = new List<string> { "All", "Stock In", "Stock Out", "Adjustment" };
    }

    /// <summary>
    /// View model for detailed stock history entry
    /// </summary>
    public class StockHistoryDetailViewModel : StockHistoryViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? SKU { get; set; }
        [Display(Name = "User")]
        public string? UserName { get; set; }
    }

    /// <summary>
    /// View model for displaying product-specific stock history
    /// </summary>
    public class ProductStockHistoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public int TargetStockLevel { get; set; }
        public DateTime? CreatedAt { get; set; }
        public IEnumerable<StockHistoryViewModel> StockHistory { get; set; } = new List<StockHistoryViewModel>();
    }

    /// <summary>
    /// View model for displaying low stock items
    /// </summary>
    public class LowStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? SupplierName { get; set; }
        public int StockLevel { get; set; }
        public int ReorderLevel { get; set; }
        public int TargetStockLevel { get; set; }
    }
} 