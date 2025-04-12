using System;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models.ViewModels.Reports
{
    /// <summary>
    /// Data transfer object for report data
    /// </summary>
    public class ReportDTO
    {
        /// <summary>
        /// Unique identifier for the report
        /// </summary>
        public string ReportId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// When the report was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Type of report (e.g., "Inventory", "Product", "Supplier")
        /// </summary>
        public string ReportType { get; set; } = string.Empty;

        /// <summary>
        /// Start date of the report period
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of the report period
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Summary statistics for the report
        /// </summary>
        public ReportSummaryDTO Summary { get; set; } = new();

        /// <summary>
        /// Individual items in the report
        /// </summary>
        public IEnumerable<ReportItemDTO> Items { get; set; } = new List<ReportItemDTO>();
    }

    /// <summary>
    /// Summary statistics for a report
    /// </summary>
    public class ReportSummaryDTO
    {
        /// <summary>
        /// Total number of items in the report
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total value of all items
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// Number of items with stock below reorder level
        /// </summary>
        public int LowStockItems { get; set; }

        /// <summary>
        /// Number of items with zero stock
        /// </summary>
        public int OutOfStockItems { get; set; }

        /// <summary>
        /// Average value per item
        /// </summary>
        public decimal AverageValue => TotalItems > 0 ? TotalValue / TotalItems : 0;

        /// <summary>
        /// Additional metrics for the report summary (e.g., AverageStockTurnover, TotalStockouts)
        /// </summary>
        public Dictionary<string, decimal> AdditionalMetrics { get; set; } = new();
    }

    /// <summary>
    /// Individual item in a report
    /// </summary>
    public class ReportItemDTO
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Category of the item
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Current quantity in stock
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price of the item
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total value (Quantity * UnitPrice)
        /// </summary>
        public decimal TotalValue { get; set; }

        /// <summary>
        /// When the item was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Optional transaction history if included in report
        /// </summary>
        public IEnumerable<TransactionHistoryDTO>? TransactionHistory { get; set; }

        /// <summary>
        /// Additional metrics for the item (e.g., StockTurnover, StockoutFrequency)
        /// </summary>
        public Dictionary<string, decimal> Metrics { get; set; } = new();

        /// <summary>
        /// Additional fields for the item (e.g., SKU, Barcode, Description)
        /// </summary>
        public Dictionary<string, string> AdditionalFields { get; set; } = new();
    }

    /// <summary>
    /// Transaction history entry for an item
    /// </summary>
    public class TransactionHistoryDTO
    {
        /// <summary>
        /// Date of the transaction
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Type of transaction (e.g., "Purchase", "Sale", "Adjustment")
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;

        /// <summary>
        /// Quantity changed in the transaction
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at the time of transaction
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total value of the transaction
        /// </summary>
        public decimal TotalValue => Quantity * UnitPrice;

        /// <summary>
        /// Optional reference number (e.g., PO number, Invoice number)
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// User who performed the transaction
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;
    }
} 