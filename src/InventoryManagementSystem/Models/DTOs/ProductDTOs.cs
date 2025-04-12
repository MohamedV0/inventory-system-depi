using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.DTOs
{
    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class ProductCreateDto
    {
        /// <summary>
        /// Product SKU
        /// </summary>
        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50, ErrorMessage = "SKU cannot be longer than 50 characters")]
        [Display(Name = "SKU")]
        public required string SKU { get; set; }

        /// <summary>
        /// Product code
        /// </summary>
        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, ErrorMessage = "Code cannot be longer than 50 characters")]
        [Display(Name = "Product Code")]
        public required string Code { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        [Display(Name = "Product Name")]
        public required string Name { get; set; }

        /// <summary>
        /// Product description
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        [Display(Name = "Description")]
        public required string Description { get; set; }

        /// <summary>
        /// Product selling price
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 9999999.99, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Selling Price")]
        public decimal Price { get; set; }

        /// <summary>
        /// Product cost 
        /// </summary>
        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, 9999999.99, ErrorMessage = "Cost must be greater than 0")]
        [Display(Name = "Cost")]
        public decimal Cost { get; set; }

        /// <summary>
        /// Product category ID
        /// </summary>
        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Product current stock level
        /// </summary>
        [Required(ErrorMessage = "Current stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Current stock must be a positive number")]
        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        /// <summary>
        /// Product reorder level (stock threshold for reordering)
        /// </summary>
        [Required(ErrorMessage = "Reorder level is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Reorder level must be at least 1")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        /// <summary>
        /// Product minimum stock level
        /// </summary>
        [Required(ErrorMessage = "Minimum stock level is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level must be a positive number")]
        [Display(Name = "Minimum Stock Level")]
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Product maximum stock level
        /// </summary>
        [Required(ErrorMessage = "Maximum stock level is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum stock level must be greater than 0")]
        [Display(Name = "Maximum Stock Level")]
        public int MaximumStockLevel { get; set; }

        /// <summary>
        /// Unit of measurement (e.g., each, kg, liter)
        /// </summary>
        [Required(ErrorMessage = "Unit of measurement is required")]
        [StringLength(20, ErrorMessage = "Unit of measurement cannot be longer than 20 characters")]
        [Display(Name = "Unit of Measurement")]
        public required string UnitOfMeasurement { get; set; }

        /// <summary>
        /// Product barcode (optional)
        /// </summary>
        [StringLength(50, ErrorMessage = "Barcode cannot be longer than 50 characters")]
        [Display(Name = "Barcode")]
        public required string Barcode { get; set; }

        /// <summary>
        /// Product image path (optional)
        /// </summary>
        [StringLength(255, ErrorMessage = "Image path cannot be longer than 255 characters")]
        [Display(Name = "Image Path")]
        public required string ImagePath { get; set; }
    }
} 