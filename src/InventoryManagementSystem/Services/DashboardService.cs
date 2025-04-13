using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Services
{
    /// <summary>
    /// Service for dashboard operations
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IProductService productService,
            IStockService stockService,
            ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _stockService = stockService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the dashboard data
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard data</returns>
        public async Task<Result<DashboardViewModel>> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var dashboardViewModel = new DashboardViewModel();
                var now = DateTime.UtcNow;
                var lastMonth = now.AddMonths(-1);

                // Get current period metrics
                var currentProducts = await GetProductMetrics(now.AddDays(-30), now);
                var previousProducts = await GetProductMetrics(now.AddDays(-60), now.AddDays(-30));

                // Calculate trends
                dashboardViewModel.ProductTrend = CalculateTrendPercentage(currentProducts.TotalCount, previousProducts.TotalCount);
                dashboardViewModel.InventoryValueTrend = CalculateTrendPercentage(currentProducts.TotalValue, previousProducts.TotalValue);

                // Set basic metrics
                dashboardViewModel.TotalProducts = currentProducts.TotalCount;
                dashboardViewModel.TotalInventoryValue = currentProducts.TotalValue;
                dashboardViewModel.TotalSuppliers = await GetTotalSuppliersAsync();
                dashboardViewModel.ActiveSuppliers = await GetActiveSuppliers(now.AddDays(-30));
                
                // Calculate stock levels
                var stockMetrics = await GetStockMetrics();
                dashboardViewModel.LowStockProducts = stockMetrics.LowStockProducts;
                dashboardViewModel.CriticalStockCount = stockMetrics.CriticalCount;
                dashboardViewModel.WarningStockCount = stockMetrics.WarningCount;

                // Calculate health metrics
                var healthMetrics = await CalculateStockHealthMetrics(now.AddDays(-90), now);
                dashboardViewModel.StockHealthScore = healthMetrics.HealthScore;
                dashboardViewModel.TurnoverRate = healthMetrics.TurnoverRate;
                dashboardViewModel.DeadStockPercentage = healthMetrics.DeadStockPercentage;
                dashboardViewModel.StockAccuracy = healthMetrics.StockAccuracy;
                dashboardViewModel.ReorderRate = healthMetrics.ReorderRate;

                // Get movement metrics
                var movementMetrics = await GetMovementMetrics(now.AddDays(-30), now);
                dashboardViewModel.AverageDailyMovement = movementMetrics.AverageDaily;
                dashboardViewModel.MovementEfficiency = movementMetrics.Efficiency;
                dashboardViewModel.PeakMovementHours = movementMetrics.PeakHours;

                // Calculate predictions and optimal ranges
                dashboardViewModel.PredictedInventoryValue = await CalculatePredictedInventoryValue(currentProducts.TotalValue, dashboardViewModel.InventoryValueTrend);
                dashboardViewModel.OptimalInventoryRange = await CalculateOptimalInventoryRange(currentProducts.TotalValue);

                // Get top performers
                dashboardViewModel.TopProducts = await GetTopProducts(now.AddDays(-30), now);
                dashboardViewModel.TopSuppliers = await GetTopSuppliers(now.AddDays(-30), now);

                // Generate alerts and recommendations
                dashboardViewModel.CriticalAlerts = GenerateCriticalAlerts(dashboardViewModel);
                dashboardViewModel.Recommendations = GenerateRecommendations(dashboardViewModel);

                // Get historical data for charts
                await PopulateInventoryValuesAsync(dashboardViewModel, cancellationToken);
                await PopulateStockMovementsAsync(dashboardViewModel, cancellationToken);
                await PopulateCategoryValuesAsync(dashboardViewModel, cancellationToken);
                await PopulateSupplierPerformanceAsync(dashboardViewModel, cancellationToken);

                return Result<DashboardViewModel>.Success(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard data");
                return Result<DashboardViewModel>.Failure("Error generating dashboard data");
            }
        }

        private decimal CalculateTrendPercentage(decimal current, decimal previous)
        {
            if (previous == 0) return 0;
            return ((current - previous) / previous) * 100;
        }

        private async Task<(int TotalCount, decimal TotalValue)> GetProductMetrics(DateTime start, DateTime end)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            if (!products.IsSuccess) return (0, 0);

            var activeProducts = products.Value.Where(p => p.CreatedAt <= end && p.IsActive);
            return (
                activeProducts.Count(),
                activeProducts.Sum(p => p.CurrentStock * p.Price)
            );
        }

        private async Task<int> GetTotalSuppliersAsync()
        {
            var result = await _unitOfWork.Suppliers.CountAsync(s => s.IsActive);
            return result.IsSuccess ? result.Value : 0;
        }

        private async Task<int> GetActiveSuppliers(DateTime since)
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            if (!suppliers.IsSuccess) return 0;

            return suppliers.Value.Count(s => s.IsActive && 
                s.ProductSuppliers.Any(ps => ps.Product.StockHistory.Any(sh => sh.Date >= since)));
        }

        private async Task<(List<ProductListItemViewModel> LowStockProducts, int CriticalCount, int WarningCount)> GetStockMetrics()
        {
            var lowStockResult = await _productService.GetProductsNeedingReorderAsync();
            if (!lowStockResult.IsSuccess) return (new List<ProductListItemViewModel>(), 0, 0);

            var products = lowStockResult.Value;
            return (
                products.ToList(),
                products.Count(p => p.CurrentStock <= p.ReorderLevel * 0.25m),
                products.Count(p => p.CurrentStock <= p.ReorderLevel * 0.5m)
            );
        }

        private async Task<(int HealthScore, decimal TurnoverRate, decimal DeadStockPercentage, decimal StockAccuracy, decimal ReorderRate)> 
        CalculateStockHealthMetrics(DateTime start, DateTime end)
        {
            try
            {
            var products = await _unitOfWork.Products.GetAllAsync();
            if (!products.IsSuccess) return (0, 0, 0, 0, 0);

            var activeProducts = products.Value.Where(p => p.IsActive).ToList();
            if (!activeProducts.Any()) return (0, 0, 0, 0, 0);

                // Get all history within the date range
                var allHistories = await _unitOfWork.StockHistories.GetStockHistoryByDateRangeAsync(start, end);
                if (!allHistories.IsSuccess) 
                {
                    return (0, 0, 0, 0, 0);
                }

                // Group history by product
                var productHistoryMap = allHistories.Value
                    .GroupBy(h => h.ProductId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Count products with meaningful transactions (StockIn/StockOut with non-zero changes)
                var productsWithMovement = activeProducts.Count(p => 
                    productHistoryMap.ContainsKey(p.Id) && 
                    productHistoryMap[p.Id].Any(h => 
                        (h.Type == TransactionType.StockIn || h.Type == TransactionType.StockOut) && 
                        h.QuantityChange != 0)
                );

            // Calculate turnover rate
                decimal turnoverRate = 0;
                int productsWithTurnover = 0;

                foreach (var product in activeProducts)
                {
                    if (productHistoryMap.ContainsKey(product.Id))
                    {
                        var history = productHistoryMap[product.Id];
                        var stockOut = history.Where(h => h.Type == TransactionType.StockOut).ToList();
                        
                        if (stockOut.Any() && product.CurrentStock > 0)
                        {
                            var totalOut = Math.Abs(stockOut.Sum(h => h.QuantityChange));
                            var avgInventory = product.CurrentStock;
                            
                            if (avgInventory > 0)
                            {
                                turnoverRate += (totalOut / avgInventory);
                                productsWithTurnover++;
                            }
                        }
                    }
                }

                // Calculate average turnover rate
                turnoverRate = productsWithTurnover > 0 
                    ? turnoverRate / productsWithTurnover 
                    : 0;

                // Calculate dead stock percentage (products with no StockIn/StockOut movement)
                var deadStockPercentage = activeProducts.Any() 
                    ? (decimal)(activeProducts.Count - productsWithMovement) / activeProducts.Count * 100
                    : 0;

                // Calculate stock accuracy
                var stockAccuracy = await CalculateStockAccuracyAsync(start, end);

            // Calculate reorder rate
                var reorderRate = activeProducts.Any()
                    ? (decimal)activeProducts.Count(p => p.CurrentStock <= p.ReorderLevel) / activeProducts.Count * 100
                    : 0;

            // Calculate overall health score
            var healthScore = (int)((100 - deadStockPercentage) * 0.3m + 
                                  stockAccuracy * 0.3m + 
                                  (turnoverRate > 1 ? 100 : turnoverRate * 100) * 0.2m + 
                                  (100 - reorderRate) * 0.2m);

            return (healthScore, turnoverRate, deadStockPercentage, stockAccuracy, reorderRate);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating stock health metrics");
                return (0, 0, 0, 0, 0);
            }
        }

        private async Task<(int AverageDaily, decimal Efficiency, List<int> PeakHours)> GetMovementMetrics(DateTime start, DateTime end)
        {
            var history = await _unitOfWork.StockHistories.GetStockHistoryByDateRangeAsync(start, end);
            if (!history.IsSuccess) return (0, 0, new List<int>());

            var movements = history.Value;
            var dailyMovements = movements.GroupBy(m => m.Date.Date)
                                        .Select(g => Math.Abs(g.Sum(m => m.QuantityChange)))
                                        .ToList();

            var averageDaily = dailyMovements.Any() ? (int)dailyMovements.Average() : 0;
            
            // Calculate efficiency (successful movements vs total movements)
            var successfulMovements = movements.Count(m => m.QuantityChange != 0);
            var totalMovements = movements.Count();
            var efficiency = totalMovements > 0 ? (decimal)successfulMovements / totalMovements * 100 : 0;

            // Get peak hours (hours with most movement)
            var peakHours = movements.GroupBy(m => m.Date.Hour)
                                   .OrderByDescending(g => g.Count())
                                   .Take(3)
                                   .Select(g => g.Key)
                                   .ToList();

            return (averageDaily, efficiency, peakHours);
        }

        private async Task<decimal> CalculatePredictedInventoryValue(decimal currentValue, decimal trend)
        {
            // Simple linear prediction based on current trend
            return currentValue * (1 + (trend / 100));
        }

        private async Task<(decimal Min, decimal Max)> CalculateOptimalInventoryRange(decimal currentValue)
        {
            // Calculate optimal range based on current value (example logic)
            var min = currentValue * 0.8m;
            var max = currentValue * 1.2m;
            return (min, max);
        }

        private async Task<List<TopProductViewModel>> GetTopProducts(DateTime start, DateTime end)
        {
            try 
            {
                // Get all products
            var products = await _unitOfWork.Products.GetAllAsync();
                if (!products.IsSuccess) 
                    return new List<TopProductViewModel>();

                // Get all stock history for the date range
                var allHistory = await _unitOfWork.StockHistories.GetStockHistoryByDateRangeAsync(start, end);
                if (!allHistory.IsSuccess) 
                    return new List<TopProductViewModel>();

                // Group history by product
                var productHistoryMap = allHistory.Value
                    .GroupBy(h => h.ProductId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var result = products.Value
                .Where(p => p.IsActive)
                    .Select(p => 
                    {
                        // Get history for this product if available
                        var productHistory = productHistoryMap.ContainsKey(p.Id) 
                            ? productHistoryMap[p.Id] 
                            : new List<StockHistory>();
                        
                        // Calculate stock in/out using actual data
                        int stockIn = productHistory
                            .Where(h => h.Type == TransactionType.StockIn)
                            .Sum(h => h.QuantityChange);
                        
                        int stockOut = Math.Abs(productHistory
                            .Where(h => h.Type == TransactionType.StockOut)
                            .Sum(h => h.QuantityChange));

                        // Calculate turnover rate based on actual data
                        decimal turnoverRate = CalculateProductTurnoverRate(productHistory);
                        
                        // Calculate trend based on actual data
                        var trendPercentage = CalculateProductTrendPercentage(productHistory);
                        var trendDirection = trendPercentage >= 0 ? "up" : "down";
                        var trendClass = trendPercentage > 10 ? "text-success" : 
                                        trendPercentage < -10 ? "text-danger" : "text-warning";
                        
                        return new TopProductViewModel
                {
                    Name = p.Name,
                    NeedsReorder = p.CurrentStock <= p.ReorderLevel,
                            StockIn = stockIn,
                            StockOut = stockOut,
                            TurnoverRate = Math.Min(100, (int)(turnoverRate * 100)), // Cap at 100% for display
                    TurnoverClass = GetTurnoverClass(p.CurrentStock, p.ReorderLevel),
                            TrendClass = trendClass,
                            TrendDirection = trendDirection,
                            TrendPercentage = Math.Abs(trendPercentage)
                        };
                    })
                    .Where(p => p.StockIn > 0 || p.StockOut > 0) // Only show products with movement
                    .OrderByDescending(p => p.StockIn + p.StockOut) // Order by total movement
                    .Take(10) // Increased from 5 to 10
                .ToList();
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating top products using actual data");
                return new List<TopProductViewModel>();
            }
        }
        
        private async Task PopulateSupplierPerformanceAsync(DashboardViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                // Get suppliers with their product associations
                var suppliers = await _unitOfWork.Suppliers
                    .GetPagedAsync(
                        page: 1,
                        pageSize: int.MaxValue,
                        filter: s => s.IsActive,
                        includeProperties: "ProductSuppliers",
                        options: new QueryOptions { TrackingEnabled = false },
                        cancellationToken: cancellationToken);

                if (!suppliers.IsSuccess || !suppliers.Value.Any()) 
                {
                    model.SupplierPerformances = new List<SupplierPerformance>();
                    return;
                }

                // Get all stock history for the last quarter
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-3);
                var stockHistory = await _unitOfWork.StockHistories.GetStockHistoryByDateRangeAsync(startDate, endDate);
                if (!stockHistory.IsSuccess) 
                {
                    model.SupplierPerformances = new List<SupplierPerformance>();
                    return;
                }
                
                var historyData = stockHistory.Value;
                
                // Get all products for reference
                var products = await _unitOfWork.Products.GetAllAsync();
                if (!products.IsSuccess) 
                {
                    model.SupplierPerformances = new List<SupplierPerformance>();
                    return;
                }
                
                var productDict = products.Value.ToDictionary(p => p.Id, p => p);
                
                // Calculate supplier performance metrics based on real data
                var supplierPerformances = suppliers.Value
                    .Select(s => 
                    {
                        // Get all product IDs for this supplier
                        var supplierProductIds = s.ProductSuppliers?.Select(ps => ps.ProductId).ToList() ?? new List<int>();
                        if (!supplierProductIds.Any()) return null;
                        
                        // Get all stock history related to this supplier's products
                        var supplierHistory = historyData
                            .Where(h => supplierProductIds.Contains(h.ProductId))
                            .ToList();
                        
                        // Calculate supplier metrics
                        var stockInTransactions = supplierHistory
                            .Where(h => h.Type == TransactionType.StockIn)
                            .ToList();
                        
                        // Calculate lead time in days
                        int leadTimeDays = CalculateLeadTimeDays(stockInTransactions);
                        
                        // Calculate completion rate
                        decimal completionRate = CalculateActualCompletionRate(supplierHistory);
                        
                        // Skip suppliers with no meaningful data
                        if (leadTimeDays == 0 && completionRate == 0) return null;
                        
                        return new SupplierPerformance
                        {
                            SupplierName = s.Name,
                            LeadTimeDays = leadTimeDays,
                            OrderCompletionRate = completionRate
                        };
                    })
                    .Where(sp => sp != null)
                    .OrderByDescending(sp => sp.OrderCompletionRate)
                    .Take(10)
                    .ToList();
                    
                model.SupplierPerformances = supplierPerformances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating supplier performance with actual data");
                model.SupplierPerformances = new List<SupplierPerformance>();
            }
        }

        private string GetTurnoverClass(int currentStock, int reorderLevel)
        {
            var ratio = (decimal)currentStock / reorderLevel;
            return ratio switch
            {
                <= 0.25m => "bg-danger",
                <= 0.5m => "bg-warning",
                <= 0.75m => "bg-info",
                _ => "bg-success"
            };
        }

        private async Task<List<TopSupplierViewModel>> GetTopSuppliers(DateTime start, DateTime end)
        {
            try
            {
                // Get suppliers with all related data
                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                if (!suppliers.IsSuccess) 
                    return new List<TopSupplierViewModel>();

                // Get all stock history for the date range to avoid repetitive database queries
                var stockHistory = await _unitOfWork.StockHistories.GetStockHistoryByDateRangeAsync(start, end);
                if (!stockHistory.IsSuccess)
                    return new List<TopSupplierViewModel>();

                var historyData = stockHistory.Value;

                var result = suppliers.Value
                    .Where(s => s.IsActive)
                    .Select(s => 
                    {
                        // Get all stock history related to this supplier's products
                        var supplierProducts = s.ProductSuppliers?.Select(ps => ps.ProductId) ?? new List<int>();
                        var supplierHistory = historyData.Where(h => supplierProducts.Contains(h.ProductId)).ToList();
                        
                        // Calculate metrics based on actual data
                        var stockInCount = supplierHistory.Count(h => h.Type == TransactionType.StockIn);
                        var adjustmentCount = supplierHistory.Count(h => h.Type == TransactionType.Adjustment);
                        var totalTransactions = supplierHistory.Count();
                        
                        // Lead time - In a real system, this would be calculated from order date to delivery date
                        // For now, we'll use the number of days between first and last stock-in transaction
                        var stockInTransactions = supplierHistory.Where(h => h.Type == TransactionType.StockIn).ToList();
                        var leadTimeDays = CalculateLeadTimeDays(stockInTransactions);
                        
                        // Completion rate - calculated as successfully delivered items / total ordered
                        // Without order data, we'll use successful stock-ins vs total expected
                        var completionRate = CalculateActualCompletionRate(supplierHistory);
                        
                        // Quality rating - based on adjustment percentage (fewer adjustments = higher quality)
                        var qualityRating = CalculateActualQualityRating(supplierHistory);
                        
                        // Risk level - derived from other metrics
                        var riskLevel = DetermineRiskLevel(leadTimeDays, completionRate, qualityRating);
                        
                        return new TopSupplierViewModel
                        {
                            Name = s.Name,
                            LeadTimeDays = leadTimeDays,
                            LeadTimeClass = GetLeadTimeClass(leadTimeDays),
                            CompletionRate = completionRate,
                            CompletionClass = GetCompletionClass(completionRate),
                            QualityRating = qualityRating,
                            RiskLevel = riskLevel,
                            RiskClass = GetRiskClass(riskLevel)
                        };
                    })
                    .Where(s => s.CompletionRate > 0) // Only include suppliers with actual data
                    .OrderByDescending(s => s.CompletionRate)
                    .Take(10)
                    .ToList();
                    
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating top suppliers using actual data");
                return new List<TopSupplierViewModel>();
            }
        }
        
        // Calculate lead time based on actual stock-in transactions
        private int CalculateLeadTimeDays(List<StockHistory> stockInTransactions)
        {
            if (stockInTransactions.Count < 2)
                return 0;
            
            var firstTransaction = stockInTransactions.OrderBy(h => h.Date).First();
            var lastTransaction = stockInTransactions.OrderBy(h => h.Date).Last();
            
            // Lead time is days between first and last transaction divided by number of deliveries
            var totalDays = (lastTransaction.Date - firstTransaction.Date).TotalDays;
            var numberOfDeliveries = stockInTransactions.Select(h => h.Date.Date).Distinct().Count();
            
            // Avoid division by zero
            if (numberOfDeliveries <= 1)
                return (int)totalDays;
            
            return (int)(totalDays / (numberOfDeliveries - 1));
        }
        
        // Calculate completion rate based on actual transactions
        private decimal CalculateActualCompletionRate(List<StockHistory> supplierHistory)
        {
            if (!supplierHistory.Any())
                return 0;
            
            var stockInTransactions = supplierHistory.Where(h => h.Type == TransactionType.StockIn).ToList();
            if (!stockInTransactions.Any())
                return 0;
            
            // Total quantity actually received
            var receivedQuantity = stockInTransactions.Sum(h => h.QuantityChange);
            
            // Estimate expected quantity based on average stock levels and reorder points of products
            var productIds = supplierHistory.Select(h => h.ProductId).Distinct().ToList();
            var productsTask = _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
            var products = productsTask.Result;
            
            if (!products.IsSuccess || !products.Value.Any())
                return 0;
            
            // Calculate expected order quantity based on reorder levels
            var expectedQuantity = products.Value.Sum(p => p.ReorderLevel);
            
            if (expectedQuantity == 0)
                return receivedQuantity > 0 ? 100 : 0;
            
            // Calculate completion percentage
            return Math.Min(100, Math.Round((decimal)receivedQuantity / expectedQuantity * 100, 1));
        }
        
        // Calculate quality rating based on adjustment frequency
        private int CalculateActualQualityRating(List<StockHistory> supplierHistory)
        {
            if (!supplierHistory.Any())
                return 0;
            
            var totalTransactions = supplierHistory.Count();
            var adjustments = supplierHistory.Count(h => h.Type == TransactionType.Adjustment);
            
            // Fewer adjustments = higher quality
            var adjustmentRatio = totalTransactions > 0 ? (decimal)adjustments / totalTransactions : 0;
            
            // Convert to 1-5 scale (lower ratio = higher rating)
            if (adjustmentRatio == 0) return 5;
            else if (adjustmentRatio < 0.05m) return 4;
            else if (adjustmentRatio < 0.1m) return 3;
            else if (adjustmentRatio < 0.2m) return 2;
            else return 1;
        }
        
        // Determine risk level based on actual metrics
        private string DetermineRiskLevel(int leadTimeDays, decimal completionRate, int qualityRating)
        {
            // Calculate a risk score based on the metrics
            var riskScore = 0;
            
            // Lead time risk
            if (leadTimeDays > 7) riskScore += 3;
            else if (leadTimeDays > 5) riskScore += 2;
            else if (leadTimeDays > 3) riskScore += 1;
            
            // Completion rate risk
            if (completionRate < 80) riskScore += 3;
            else if (completionRate < 90) riskScore += 2;
            else if (completionRate < 95) riskScore += 1;
            
            // Quality rating risk
            if (qualityRating <= 2) riskScore += 3;
            else if (qualityRating == 3) riskScore += 2;
            else if (qualityRating == 4) riskScore += 1;
            
            // Determine risk level
            if (riskScore >= 7) return "High";
            else if (riskScore >= 4) return "Medium";
            else return "Low";
        }
        
        // Get lead time CSS class based on actual lead time
        private string GetLeadTimeClass(int leadTimeDays)
        {
            return leadTimeDays switch
            {
                <= 3 => "text-success",
                <= 5 => "text-warning",
                _ => "text-danger"
            };
        }

        // Get completion CSS class based on actual completion rate
        private string GetCompletionClass(decimal completionRate)
        {
            return completionRate switch
            {
                >= 90 => "bg-success",
                >= 75 => "bg-warning",
                _ => "bg-danger"
            };
        }

        // Get risk CSS class based on actual risk level
        private string GetRiskClass(string riskLevel)
        {
            return riskLevel switch
            {
                "Low" => "bg-success",
                "Medium" => "bg-warning",
                _ => "bg-danger"
            };
        }

        private List<DashboardAlert> GenerateCriticalAlerts(DashboardViewModel dashboard)
        {
            var alerts = new List<DashboardAlert>();

            // Add critical stock alerts
            if (dashboard.CriticalStockCount > 0)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Critical Stock Levels",
                    Description = $"{dashboard.CriticalStockCount} products below 25% of reorder level",
                    Severity = "Critical",
                    Icon = "fas fa-exclamation-circle",
                    SeverityClass = "alert-danger",
                    ActionUrl = "/Stock/LowStock",
                    ActionIcon = "fas fa-arrow-right"
                });
            }

            // Add dead stock alerts
            if (dashboard.DeadStockPercentage > 20)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "High Dead Stock",
                    Description = $"{dashboard.DeadStockPercentage:N1}% of inventory showing no movement",
                    Severity = "Warning",
                    Icon = "fas fa-box",
                    SeverityClass = "alert-warning",
                    ActionUrl = "/Stock/DeadStock",
                    ActionIcon = "fas fa-arrow-right"
                });
            }

            // Add stock accuracy alerts
            if (dashboard.StockAccuracy < 95)
            {
                alerts.Add(new DashboardAlert
                {
                    Title = "Low Stock Accuracy",
                    Description = $"Stock accuracy at {dashboard.StockAccuracy:N1}%",
                    Severity = "Warning",
                    Icon = "fas fa-balance-scale",
                    SeverityClass = "alert-warning",
                    ActionUrl = "/Stock/Audit",
                    ActionIcon = "fas fa-arrow-right"
                });
            }

            return alerts;
        }

        private List<DashboardRecommendation> GenerateRecommendations(DashboardViewModel dashboard)
        {
            var recommendations = new List<DashboardRecommendation>();

            // Reorder recommendations
            if (dashboard.CriticalStockCount > 0)
            {
                recommendations.Add(new DashboardRecommendation
                {
                    Title = "Add Stock",
                    Description = $"There are {dashboard.CriticalStockCount} items with low stock",
                    Priority = "High",
                    Icon = "fas fa-shopping-cart", 
                    ActionUrl = "/Stock/AddStock",
                    ActionText = "AddStock"
                });
            }

            return recommendations;
        }

        private async Task PopulateInventoryValuesAsync(DashboardViewModel model, CancellationToken cancellationToken)
        {
            // Get stock history for the past quarter
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-3);

            var stockHistory = await _unitOfWork.StockHistories
                .GetStockHistoryByDateRangeAsync(startDate, endDate);

            if (!stockHistory.IsSuccess) return;

            var historyItems = stockHistory.Value
                .OrderBy(h => h.Date)
                .ToList();

            // Calculate daily inventory values
            var dailyValues = historyItems
                .GroupBy(h => h.Date.Date)
                .Select(g => new { Date = g.Key, Value = g.Sum(h => h.NewStock * (h.UnitPrice ?? h.Product.Price)) })
                .ToDictionary(x => x.Date, x => x.Value);

            // Populate weekly values (last 7 days)
            model.WeeklyInventoryValues = dailyValues
                .Where(kv => kv.Key >= endDate.AddDays(-7))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Populate monthly values (last 4 weeks)
            model.MonthlyInventoryValues = dailyValues
                .Where(kv => kv.Key >= endDate.AddDays(-28))
                .GroupBy(kv => new DateTime(kv.Key.Year, kv.Key.Month, ((kv.Key.Day - 1) / 7) * 7 + 1))
                .ToDictionary(g => g.Key, g => g.Average(kv => kv.Value));

            // Populate quarterly values (last 3 months)
            model.QuarterlyInventoryValues = dailyValues
                .GroupBy(kv => new DateTime(kv.Key.Year, kv.Key.Month, 1))
                .ToDictionary(g => g.Key, g => g.Average(kv => kv.Value));
        }

        private async Task PopulateStockMovementsAsync(DashboardViewModel model, CancellationToken cancellationToken)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-28); // Last 4 weeks

            var stockHistory = await _unitOfWork.StockHistories
                .GetStockHistoryByDateRangeAsync(startDate, endDate);

            if (!stockHistory.IsSuccess) return;

            var movements = stockHistory.Value
                .GroupBy(h => h.Date.Date)
                .Select(g => new StockMovementData
                {
                    Date = g.Key,
                    StockIn = g.Where(h => h.QuantityChange > 0).Sum(h => h.QuantityChange),
                    StockOut = Math.Abs(g.Where(h => h.QuantityChange < 0).Sum(h => h.QuantityChange))
                })
                .OrderBy(m => m.Date)
                .ToList();

            model.WeeklyStockMovements = movements;
        }

        private async Task PopulateCategoryValuesAsync(DashboardViewModel model, CancellationToken cancellationToken)
        {
            var products = await _unitOfWork.Products
                .GetPagedAsync(
                    page: 1,
                    pageSize: int.MaxValue,
                    filter: p => p.IsActive,
                    includeProperties: "Category",
                    options: new QueryOptions { TrackingEnabled = false },
                    cancellationToken: cancellationToken);

            if (!products.IsSuccess) return;

            model.CategoryValues = products.Value
                .GroupBy(p => p.Category?.Name ?? "Uncategorized")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(p => p.CurrentStock * p.Price)
                );
        }

        // New method to calculate stock accuracy based on actual inventory adjustments
        private async Task<decimal> CalculateStockAccuracyAsync(DateTime start, DateTime end)
        {
            // Get all stock adjustment history entries
            var adjustments = await _unitOfWork.StockHistories.FindAsync(
                sh => sh.Date >= start && sh.Date <= end && 
                      sh.Type == Models.Common.TransactionType.Adjustment);
            
            if (!adjustments.IsSuccess || !adjustments.Value.Any())
                return 100m; // If no adjustments were made, assume 100% accuracy
            
            // Get all inventory count records
            var allInventoryCounts = await _unitOfWork.StockHistories.CountAsync(
                sh => sh.Date >= start && sh.Date <= end && 
                      (sh.Type == Models.Common.TransactionType.StockIn || 
                       sh.Type == Models.Common.TransactionType.StockOut ||
                       sh.Type == Models.Common.TransactionType.Adjustment));
            
            if (!allInventoryCounts.IsSuccess || allInventoryCounts.Value == 0)
                return 100m;
                
            // Calculate the number of accurate counts (counts that didn't need adjustment)
            var totalCounts = allInventoryCounts.Value;
            var adjustmentCounts = adjustments.Value.Count();
            
            // Lower adjustment count means higher accuracy
            return ((decimal)(totalCounts - adjustmentCounts) / totalCounts) * 100;
        }

        // Add back the turnover rate calculation method for product listings
        private decimal CalculateProductTurnoverRate(IEnumerable<StockHistory> history)
        {
            if (!history.Any()) return 0m;

            // Look specifically for StockOut transactions
            var stockOut = history.Where(h => h.Type == TransactionType.StockOut).ToList();
            if (!stockOut.Any()) return 0m;

            // Calculate total quantity moved out
            var totalOut = Math.Abs(stockOut.Sum(h => h.QuantityChange));
            
            // Get all stock levels from history to calculate average inventory
            var stockLevels = history.Select(h => h.NewStock).ToList();
            if (!stockLevels.Any() || stockLevels.All(s => s == 0)) return 0m;
            
            // Calculate average inventory, filtering out zero values
            var nonZeroLevels = stockLevels.Where(s => s > 0).ToList();
            var avgInventory = nonZeroLevels.Any() ? nonZeroLevels.Average() : stockLevels.Average();
            var avgInventoryDecimal = Convert.ToDecimal(avgInventory);
            
            // Safeguard against division by zero
            if (avgInventoryDecimal == 0m) return 0m;
            
            // Calculate turnover as ratio of outgoing stock to average inventory
            return totalOut / avgInventoryDecimal;
        }

        // Calculate product trend based on stock history
        private decimal CalculateProductTrendPercentage(IEnumerable<StockHistory> history)
        {
            if (!history.Any()) return 0;

            var orderedHistory = history.OrderBy(h => h.Date).ToList();
            
            // Split history into first half and second half
            var firstHalf = orderedHistory.Take(orderedHistory.Count / 2);
            var secondHalf = orderedHistory.Skip(orderedHistory.Count / 2);

            // Compare average stock levels between the two periods
            var firstHalfAvg = firstHalf.Any() ? (decimal)firstHalf.Average(h => h.NewStock) : 0;
            var secondHalfAvg = secondHalf.Any() ? (decimal)secondHalf.Average(h => h.NewStock) : 0;

            // Calculate percentage change, avoiding division by zero
            if (firstHalfAvg > 0)
                return Math.Round(((secondHalfAvg - firstHalfAvg) / firstHalfAvg) * 100, 1);
            else if (secondHalfAvg > 0) 
                return 100; // If we went from 0 to something, that's a 100% increase
            else
                return 0;
        }
    }
} 