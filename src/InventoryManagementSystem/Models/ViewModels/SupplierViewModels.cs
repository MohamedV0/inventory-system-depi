using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for creating a new supplier
    /// </summary>
    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;
    }

    /// <summary>
    /// View model for updating an existing supplier
    /// </summary>
    public class UpdateSupplierViewModel : CreateSupplierViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// View model for displaying supplier details
    /// </summary>
    public class SupplierDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Product Count")]
        public int ProductCount { get; set; }

        public List<SupplierProductViewModel> Products { get; set; } = new List<SupplierProductViewModel>();
    }

    /// <summary>
    /// View model for displaying supplier in a list
    /// </summary>
    public class SupplierListItemViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Supplier Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Product Count")]
        public int ProductCount { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Base view model for supplier entity
    /// </summary>
    public class SupplierViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }

        [StringLength(100, ErrorMessage = "Website cannot exceed 100 characters")]
        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? UpdatedAt { get; set; }
        
        public string? UpdatedBy { get; set; }
    }
} 