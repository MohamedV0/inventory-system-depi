using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels;

/// <summary>
/// Base view model for product-supplier relationship
/// </summary>
public class ProductSupplierViewModel
{
    public int ProductId { get; set; }
    
    public int SupplierId { get; set; }
    
    public string SupplierName { get; set; } = string.Empty;
    
    public string SupplierEmail { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0")]
    public decimal UnitPrice { get; set; }
    
    // Also include SupplierPrice for backward compatibility
    public decimal SupplierPrice => UnitPrice;
    
    public string SupplierSKU { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue, ErrorMessage = "Lead time days must be greater than or equal to 0")]
    public int LeadTimeDays { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Minimum order quantity must be greater than or equal to 0")]
    public int MinimumOrderQuantity { get; set; }
    
    public bool IsPreferredSupplier { get; set; }
    
    // Also include IsPreferred for backward compatibility
    public bool IsPreferred => IsPreferredSupplier;
    
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
    
    public DateTime? LastPurchaseDate { get; set; }
}

public class ProductSupplierListItemViewModel
{
    public int ProductId { get; set; }
    public int SupplierId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int LeadTimeDays { get; set; }
    public int MinimumOrderQuantity { get; set; }
    public bool IsPreferredSupplier { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}

public class ProductSupplierDetailsViewModel : ProductSupplierListItemViewModel
{
    public string ProductSku { get; set; } = string.Empty;
    public string SupplierContactName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
    
    // Reference to product and supplier objects
    public ProductBasicViewModel? Product { get; set; }
    public SupplierBasicViewModel? Supplier { get; set; }
}

public class CreateProductSupplierViewModel
{
    [Required(ErrorMessage = "Product ID is required")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Supplier ID is required")]
    public int SupplierId { get; set; }

    [Required(ErrorMessage = "Unit price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Lead time is required")]
    [Range(1, 365, ErrorMessage = "Lead time must be between 1 and 365 days")]
    public int LeadTimeDays { get; set; }

    [Required(ErrorMessage = "Minimum order quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Minimum order quantity must be greater than 0")]
    public int MinimumOrderQuantity { get; set; }

    public bool IsPreferredSupplier { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
    public string Notes { get; set; } = string.Empty;
}

public class UpdateProductSupplierViewModel : CreateProductSupplierViewModel
{
    // Inherits all properties from CreateProductSupplierViewModel
    // No need to redefine properties with the 'new' keyword
}

/// <summary>
/// Basic view model for product reference
/// </summary>
public class ProductBasicViewModel
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string SKU { get; set; } = string.Empty;
}

/// <summary>
/// Basic view model for supplier reference
/// </summary>
public class SupplierBasicViewModel
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
} 