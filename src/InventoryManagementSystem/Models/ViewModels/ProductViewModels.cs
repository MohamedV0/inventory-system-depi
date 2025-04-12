using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for creating a new product
    /// </summary>
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit of measurement is required")]
        [StringLength(20, ErrorMessage = "Unit of measurement cannot exceed 20 characters")]
        [Display(Name = "Unit of Measurement")]
        public string UnitOfMeasurement { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be greater than or equal to 0")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Cost")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Reorder level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be greater than or equal to 0")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        [Required(ErrorMessage = "Current stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Current stock must be greater than or equal to 0")]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        
        // New properties for supplier assignment
        [Display(Name = "Primary Supplier")]
        public int? PrimarySupplierID { get; set; }
        
        [Display(Name = "Supplier Unit Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Supplier price must be greater than or equal to 0")]
        public decimal? SupplierUnitPrice { get; set; }
        
        [Display(Name = "Lead Time (Days)")]
        [Range(0, 365, ErrorMessage = "Lead time must be between 0 and 365 days")]
        public int? LeadTimeDays { get; set; }
        
        [Display(Name = "Minimum Order Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum order quantity must be greater than or equal to 0")]
        public int? MinimumOrderQuantity { get; set; }
        
        [Display(Name = "Is Preferred Supplier")]
        public bool IsPreferredSupplier { get; set; } = true;
    }

    /// <summary>
    /// View model for updating an existing product
    /// </summary>
    public class UpdateProductViewModel : CreateProductViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// View model for displaying product details
    /// </summary>
    public class ProductDetailsViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string SKU { get; set; } = string.Empty;
        
        public string UnitOfMeasurement { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public decimal Cost { get; set; }
        
        public int CurrentStock { get; set; }
        
        public int ReorderLevel { get; set; }
        
        public bool IsActive { get; set; }
        
        public int CategoryId { get; set; }
        
        public string CategoryName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        public List<ProductSupplierViewModel> Suppliers { get; set; } = new List<ProductSupplierViewModel>();
        
        public List<StockHistoryViewModel> StockHistory { get; set; } = new List<StockHistoryViewModel>();
    }

    /// <summary>
    /// View model for displaying stock history entries
    /// </summary>
    public class StockHistoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; } = string.Empty;

        public int ProductId { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Quantity Change")]
        public int QuantityChange { get; set; }

        // For backward compatibility
        public int Quantity => QuantityChange;

        [Display(Name = "Previous Stock")]
        public int PreviousStock { get; set; }

        [Display(Name = "New Stock")]
        public int NewStock { get; set; }

        [Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Reference")]
        public string? Reference { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
        
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "User")]
        public string? UpdatedBy { get; set; }

        // For backward compatibility
        public string CreatedBy => UpdatedBy ?? string.Empty;

        [Display(Name = "Type")]
        public TransactionType Type { get; set; }
        
        [Display(Name = "Type")]
        public string TypeString 
        { 
            get
            {
                return Type switch
                {
                    TransactionType.StockIn => "Stock In",
                    TransactionType.StockOut => "Stock Out",
                    TransactionType.Adjustment => "Adjustment",
                    TransactionType.Initial => "Initial Stock",
                    TransactionType.Transfer => "Transfer",
                    _ => Type.ToString()
                };
            }
        }
    }

    /// <summary>
    /// View model for displaying product in a list
    /// </summary>
    public class ProductListItemViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string SKU { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public int CurrentStock { get; set; }
        
        public int ReorderLevel { get; set; }
        
        public bool IsActive { get; set; }
        
        public int CategoryId { get; set; }
        
        public string CategoryName { get; set; } = string.Empty;
        
        public bool NeedsReorder { get; set; }
        
        public string? PrimarySupplierName { get; set; }
        
        public bool HasSupplier => !string.IsNullOrEmpty(PrimarySupplierName);
        
        public StockStatus StockStatus
        {
            get
            {
                if (CurrentStock <= ReorderLevel)
                    return StockStatus.Low;
                // Assuming MaximumStockLevel is typically 2x ReorderLevel
                else if (CurrentStock >= ReorderLevel * 2)
                    return StockStatus.High;
                else
                    return StockStatus.Normal;
            }
        }
    }
    
    // ProductSupplierViewModel is now defined in ProductSupplierViewModels.cs
    
    public class SupplierProductViewModel
    {
        public int ProductId { get; set; }
        
        public int SupplierId { get; set; }
        
        public string ProductName { get; set; } = string.Empty;
        
        public string ProductSKU { get; set; } = string.Empty;
        
        public decimal UnitPrice { get; set; }
        
        // For backward compatibility
        public decimal SupplierPrice => UnitPrice;
        
        public int LeadTimeDays { get; set; }
        
        public int MinimumOrderQuantity { get; set; }
        
        public bool IsPreferredSupplier { get; set; }
        
        // For backward compatibility
        public bool IsPreferred => IsPreferredSupplier;
        
        public DateTime? LastPurchaseDate { get; set; }
    }
    
    /// <summary>
    /// Base view model for product entity
    /// </summary>
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit of measurement is required")]
        [StringLength(20, ErrorMessage = "Unit of measurement cannot exceed 20 characters")]
        public string UnitOfMeasurement { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost must be greater than or equal to 0")]
        public decimal Cost { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be greater than or equal to 0")]
        public int ReorderLevel { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
} 