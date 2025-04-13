using System;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models.ViewModels
{
    /// <summary>
    /// View model for the dashboard
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Total number of products in the system
        /// </summary>
        public int TotalProducts { get; set; }
        
        /// <summary>
        /// Total number of active suppliers
        /// </summary>
        public int TotalSuppliers { get; set; }
        
        /// <summary>
        /// Total number of categories
        /// </summary>
        public int TotalCategories { get; set; }
        
        /// <summary>
        /// Total value of inventory (cost * quantity)
        /// </summary>
        public decimal TotalInventoryValue { get; set; }
        
        /// <summary>
        /// Products that need to be reordered (stock level below reorder level)
        /// </summary>
        public List<ProductListItemViewModel> LowStockProducts { get; set; } = new List<ProductListItemViewModel>();
        
        /// <summary>
        /// Recent stock transaction history
        /// </summary>
        public List<StockHistoryViewModel> StockHistory { get; set; } = new List<StockHistoryViewModel>();
        
        /// <summary>
        /// Number of low stock products
        /// </summary>
        public int LowStockCount => LowStockProducts.Count;
        
        /// <summary>
        /// Whether there are any low stock products
        /// </summary>
        public bool HasLowStockProducts => LowStockCount > 0;

        /// <summary>
        /// Daily inventory values for the past week
        /// </summary>
        public Dictionary<DateTime, decimal> WeeklyInventoryValues { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// Weekly inventory values for the past month
        /// </summary>
        public Dictionary<DateTime, decimal> MonthlyInventoryValues { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// Monthly inventory values for the past quarter
        /// </summary>
        public Dictionary<DateTime, decimal> QuarterlyInventoryValues { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// Weekly stock movement data
        /// </summary>
        public List<StockMovementData> WeeklyStockMovements { get; set; } = new List<StockMovementData>();

        /// <summary>
        /// Category-wise inventory value distribution
        /// </summary>
        public Dictionary<string, decimal> CategoryValues { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Supplier performance metrics
        /// </summary>
        public List<SupplierPerformance> SupplierPerformances { get; set; } = new List<SupplierPerformance>();

        /// <summary>
        /// Percentage change in total products compared to previous period
        /// </summary>
        public decimal ProductsTrend { get; set; }

        /// <summary>
        /// Percentage change in inventory value compared to previous period
        /// </summary>
        public decimal InventoryValueTrend { get; set; }

        /// <summary>
        /// Number of products with critically low stock (below 25% of reorder level)
        /// </summary>
        public int CriticalStockCount { get; set; }

        /// <summary>
        /// Number of products with warning stock level (below 50% of reorder level)
        /// </summary>
        public int WarningStockCount { get; set; }

        /// <summary>
        /// Number of inactive suppliers (no orders in last 30 days)
        /// </summary>
        public int InactiveSuppliers { get; set; }

        /// <summary>
        /// Predicted inventory value for next week based on current trends
        /// </summary>
        public decimal PredictedInventoryValue { get; set; }

        /// <summary>
        /// Optimal inventory value range
        /// </summary>
        public (decimal Min, decimal Max) OptimalInventoryRange { get; set; }

        /// <summary>
        /// Average daily stock movement
        /// </summary>
        public int AverageDailyMovement { get; set; }

        /// <summary>
        /// Peak movement hours (24-hour format)
        /// </summary>
        public List<int> PeakMovementHours { get; set; } = new List<int>();

        /// <summary>
        /// Overall stock health score (0-100)
        /// </summary>
        public int StockHealthScore { get; set; }

        /// <summary>
        /// Inventory turnover rate
        /// </summary>
        public decimal TurnoverRate { get; set; }

        /// <summary>
        /// Percentage of dead stock (no movement in 90 days)
        /// </summary>
        public decimal DeadStockPercentage { get; set; }

        /// <summary>
        /// List of critical alerts for immediate attention
        /// </summary>
        public List<DashboardAlert> CriticalAlerts { get; set; } = new List<DashboardAlert>();

        /// <summary>
        /// List of recommended actions based on current metrics
        /// </summary>
        public List<DashboardRecommendation> Recommendations { get; set; } = new List<DashboardRecommendation>();

        /// <summary>
        /// Number of active suppliers (with orders in last 30 days)
        /// </summary>
        public int ActiveSuppliers { get; set; }

        /// <summary>
        /// Stock movement efficiency percentage
        /// </summary>
        public decimal MovementEfficiency { get; set; }

        /// <summary>
        /// CSS class for stock health indicator
        /// </summary>
        public string StockHealthClass => StockHealthScore switch
        {
            >= 80 => "excellent",
            >= 60 => "good",
            >= 40 => "fair",
            _ => "poor"
        };

        /// <summary>
        /// Stock accuracy percentage
        /// </summary>
        public decimal StockAccuracy { get; set; }

        /// <summary>
        /// Reorder rate percentage
        /// </summary>
        public decimal ReorderRate { get; set; }

        /// <summary>
        /// Product trend percentage
        /// </summary>
        public decimal ProductTrend { get; set; }

        /// <summary>
        /// List of top performing products
        /// </summary>
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();

        /// <summary>
        /// List of top performing suppliers
        /// </summary>
        public List<TopSupplierViewModel> TopSuppliers { get; set; } = new List<TopSupplierViewModel>();
    }

    public class StockMovementData
    {
        public DateTime Date { get; set; }
        public int StockIn { get; set; }
        public int StockOut { get; set; }
    }

    public class SupplierPerformance
    {
        public string SupplierName { get; set; } = string.Empty;
        public int LeadTimeDays { get; set; }
        public decimal OrderCompletionRate { get; set; }
    }

    public class DashboardAlert
    {
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionIcon { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string SeverityClass { get; set; } = string.Empty;
    }

    public class DashboardRecommendation
    {
        public string Action { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class TopProductViewModel
    {
        public string Name { get; set; } = string.Empty;
        public bool NeedsReorder { get; set; }
        public int StockIn { get; set; }
        public int StockOut { get; set; }
        public decimal TurnoverRate { get; set; }
        public string TurnoverClass { get; set; } = string.Empty;
        public string TrendClass { get; set; } = string.Empty;
        public string TrendDirection { get; set; } = string.Empty;
        public decimal TrendPercentage { get; set; }
    }

    public class TopSupplierViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int LeadTimeDays { get; set; }
        public string LeadTimeClass { get; set; } = string.Empty;
        public decimal CompletionRate { get; set; }
        public string CompletionClass { get; set; } = string.Empty;
        public int QualityRating { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RiskClass { get; set; } = string.Empty;
    }
} 