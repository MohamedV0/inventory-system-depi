using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.ViewModels.Reports
{
    /// <summary>
    /// Parameters for generating inventory reports
    /// </summary>
    public class ReportParameters
    {
        /// <summary>
        /// Start date for the report period
        /// </summary>
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the report period
        /// </summary>
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Type of report to generate (e.g., "Inventory", "Product", "Supplier")
        /// </summary>
        [Required(ErrorMessage = "Report type is required")]
        [Display(Name = "Report Type")]
        public string ReportType { get; set; } = "Inventory";

        /// <summary>
        /// Optional category ID to filter the report
        /// </summary>
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Optional supplier ID to filter the report
        /// </summary>
        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        /// <summary>
        /// Whether to include inactive items in the report
        /// </summary>
        [Display(Name = "Include Inactive Items")]
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Whether to include detailed transaction history
        /// </summary>
        [Display(Name = "Include Transaction History")]
        public bool IncludeTransactionHistory { get; set; }

        /// <summary>
        /// Validates that EndDate is not earlier than StartDate
        /// </summary>
        public bool ValidateDateRange()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                return EndDate.Value.Date >= StartDate.Value.Date;
            }
            return true;
        }
    }
} 