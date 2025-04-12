using AutoMapper;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels.Reports;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using CsvHelper;
using InventoryManagementSystem.Extensions;
using InventoryManagementSystem.Data.Specifications;
using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Services
{
    /// <summary>
    /// Service for generating and exporting inventory reports
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly Data.Context.IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ReportService> _logger;
        private readonly IExportService _exportService;

        public ReportService(
            Data.Context.IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService,
            ILogger<ReportService> logger,
            IExportService exportService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        /// <inheritdoc />
        public async Task<Result<ReportDTO>> GenerateReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                return Result<ReportDTO>.ValidationError("Report parameters cannot be null");

            if (!parameters.ValidateDateRange())
                return Result<ReportDTO>.ValidationError("End date cannot be earlier than start date");

            try
            {
                // Try to get from cache first
                var cacheKey = GenerateCacheKey(parameters);
                var cachedReport = await _cacheService.GetOrCreateAsync(
                    cacheKey,
                    async ct => await GenerateReportInternalAsync(parameters, ct),
                    TimeSpan.FromHours(1),
                    cancellationToken);

                return Result<ReportDTO>.Success(cachedReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating {ReportType} report", parameters.ReportType);
                return Result<ReportDTO>.Failure($"Error generating report: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<byte[]>> ExportReportAsync(
            ReportParameters parameters,
            string format,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(format))
                return Result<byte[]>.ValidationError("Export format must be specified");

            try
            {
                var reportResult = await GenerateReportAsync(parameters, cancellationToken);
                if (!reportResult.IsSuccess)
                    return Result<byte[]>.Failure(reportResult.Message);

                return format.ToLowerInvariant() switch
                {
                    "pdf" => await _exportService.ExportToPdfAsync(reportResult.Value),
                    "excel" => await _exportService.ExportToExcelAsync(reportResult.Value),
                    "csv" => await _exportService.ExportToCsvAsync(reportResult.Value),
                    _ => Result<byte[]>.ValidationError($"Unsupported export format: {format}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting {ReportType} report to {Format}", 
                    parameters.ReportType, format);
                return Result<byte[]>.Failure($"Error exporting report: {ex.Message}");
            }
        }

        private async Task<ReportDTO> GenerateReportInternalAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            switch (parameters.ReportType)
            {
                case "StockLevels":
                    return await GenerateStockLevelsReportAsync(parameters, cancellationToken);
                case "Products":
                    return await GenerateProductsReportAsync(parameters, cancellationToken);
                case "Suppliers":
                    return await GenerateSuppliersReportAsync(parameters, cancellationToken);
                case "Inventory":
                    return await GenerateInventoryReportAsync(parameters, cancellationToken);
                case "StockMovement":
                    return await GenerateStockMovementReportAsync(parameters, cancellationToken);
                case "LowStock":
                    return await GenerateLowStockReportAsync(parameters, cancellationToken);
                case "Performance":
                    return await GeneratePerformanceReportAsync(parameters, cancellationToken);
                default:
                    throw new InvalidOperationException($"Unsupported report type: {parameters.ReportType}");
            }
        }

        private async Task<ReportDTO> GenerateStockLevelsReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Use BaseProductSpecification to ensure Category is included
            var specification = new BaseProductSpecification();
            var productsResult = await _unitOfWork.Products.FindAsync(specification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = products.Select(p => new ReportItemDTO
            {
                Name = p.Name,
                Category = p.Category?.Name ?? "Uncategorized",
                Quantity = p.CurrentStock,
                UnitPrice = p.Price,
                TotalValue = p.CurrentStock * p.Price,
                LastUpdated = p.UpdatedAt ?? p.CreatedAt
            }).ToList();

            return new ReportDTO
            {
                ReportType = "Stock Levels",
                GeneratedDate = DateTime.UtcNow,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Sum(i => i.Quantity),
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count(i => i.Quantity <= 10),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0)
                }
            };
        }

        private async Task<ReportDTO> GenerateProductsReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Use BaseProductSpecification to ensure Category is included
            var specification = new BaseProductSpecification();
            var productsResult = await _unitOfWork.Products.FindAsync(specification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = products.Select(p => new ReportItemDTO
            {
                Name = p.Name,
                Category = p.Category?.Name ?? "Uncategorized",
                Quantity = p.CurrentStock,
                UnitPrice = p.Price,
                TotalValue = p.CurrentStock * p.Price,
                LastUpdated = p.UpdatedAt ?? p.CreatedAt,
                AdditionalFields = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Code"] = p.Code,
                    ["Description"] = p.Description ?? "N/A",
                    ["UnitOfMeasurement"] = p.UnitOfMeasurement,
                    ["Barcode"] = p.Barcode ?? "N/A"
                },
                Metrics = new Dictionary<string, decimal>
                {
                    ["Cost"] = p.Cost,
                    ["ProfitMargin"] = p.Price > 0 ? ((p.Price - p.Cost) / p.Price) * 100 : 0,
                    ["MinimumStockLevel"] = p.MinimumStockLevel,
                    ["MaximumStockLevel"] = p.MaximumStockLevel,
                    ["ReorderLevel"] = p.ReorderLevel,
                    ["StockStatus"] = p.CurrentStock <= 0 ? 0 : 
                                    p.CurrentStock <= p.ReorderLevel ? 1 : 
                                    p.CurrentStock <= p.MinimumStockLevel ? 2 : 3
                },
                TransactionHistory = p.StockHistory?.Select(h => new TransactionHistoryDTO
                {
                    Date = h.Date,
                    TransactionType = h.Type.ToString(),
                    Quantity = h.QuantityChange,
                    UnitPrice = h.UnitPrice ?? 0
                }).ToList()
            }).ToList();

            // Calculate additional summary metrics
            var totalCost = items.Sum(i => i.Metrics["Cost"] * i.Quantity);
            var totalValue = items.Sum(i => i.TotalValue);
            var averageMargin = items.Any() ? items.Average(i => i.Metrics["ProfitMargin"]) : 0;

            // Set default period if not specified
            var startDate = parameters.StartDate ?? DateTime.UtcNow.Date.AddMonths(-1);
            var endDate = parameters.EndDate ?? DateTime.UtcNow.Date;

            return new ReportDTO
            {
                ReportType = "Products",
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Count(),
                    TotalValue = totalValue,
                    LowStockItems = items.Count(i => i.Metrics["StockStatus"] <= 2),
                    OutOfStockItems = items.Count(i => i.Metrics["StockStatus"] == 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["TotalCost"] = totalCost,
                        ["GrossProfit"] = totalValue - totalCost,
                        ["AverageProfitMargin"] = averageMargin,
                        ["ItemsBelowReorderLevel"] = items.Count(i => i.Quantity <= i.Metrics["ReorderLevel"]),
                        ["ItemsNearMinimumStock"] = items.Count(i => 
                            i.Quantity > i.Metrics["ReorderLevel"] && 
                            i.Quantity <= i.Metrics["MinimumStockLevel"])
                    }
                }
            };
        }

        private async Task<ReportDTO> GenerateSuppliersReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Create specification that includes all necessary related data
            var specification = new SupplierWithProductsSpecification();
            
            if (parameters.SupplierId.HasValue)
            {
                specification = new SupplierWithProductsSpecification(parameters.SupplierId.Value);
            }

            var suppliersResult = await _unitOfWork.Suppliers.FindAsync(
                specification,
                cancellationToken);

            if (!suppliersResult.IsSuccess)
                throw new InvalidOperationException(suppliersResult.Message);

            var suppliers = suppliersResult.Value;
            var items = suppliers.Select(s =>
            {
                // Calculate average unit price for supplier's products
                var avgUnitPrice = s.ProductSuppliers?
                    .Where(ps => ps.Product != null)
                    .Select(ps => ps.UnitPrice)
                    .DefaultIfEmpty(0m)
                    .Average() ?? 0m;

                // Calculate total stock value
                var totalStockValue = s.ProductSuppliers?
                    .Where(ps => ps.Product != null)
                    .Sum(ps => ps.Product.CurrentStock * ps.Product.Price) ?? 0m;

                return new ReportItemDTO
                {
                    Name = s.Name,
                    Category = "Supplier",
                    Quantity = s.ProductSuppliers?.Count ?? 0,
                    UnitPrice = avgUnitPrice, // Set the average unit price
                    TotalValue = totalStockValue,
                    LastUpdated = s.UpdatedAt ?? s.CreatedAt,
                    Metrics = new Dictionary<string, decimal>
                    {
                        ["TotalProducts"] = s.ProductSuppliers?.Count ?? 0,
                        ["AverageProductPrice"] = avgUnitPrice,
                        ["TotalStockValue"] = totalStockValue,
                        ["LowStockProducts"] = s.ProductSuppliers?
                            .Count(ps => ps.Product != null && ps.Product.CurrentStock <= ps.Product.ReorderLevel) ?? 0,
                        ["OutOfStockProducts"] = s.ProductSuppliers?
                            .Count(ps => ps.Product != null && ps.Product.CurrentStock <= 0) ?? 0,
                        ["MinPrice"] = s.ProductSuppliers?
                            .Where(ps => ps.Product != null)
                            .Min(ps => ps.Product.Price) ?? 0m,
                        ["MaxPrice"] = s.ProductSuppliers?
                            .Where(ps => ps.Product != null)
                            .Max(ps => ps.Product.Price) ?? 0m
                    }
                };
            }).ToList();

            // Calculate overall metrics
            var totalProducts = items.Sum(i => i.Quantity);
            var totalValue = items.Sum(i => i.TotalValue);
            var overallAvgPrice = items.Count > 0 
                ? Convert.ToDecimal(items.Average(i => i.UnitPrice))
                : 0m;

            return new ReportDTO
            {
                ReportType = "Suppliers",
                GeneratedDate = DateTime.UtcNow,
                StartDate = parameters.StartDate,
                EndDate = parameters.EndDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = totalProducts,
                    TotalValue = totalValue,
                    LowStockItems = (int)items.Sum(i => i.Metrics["LowStockProducts"]),
                    OutOfStockItems = (int)items.Sum(i => i.Metrics["OutOfStockProducts"]),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["TotalSuppliers"] = items.Count,
                        ["AverageProductsPerSupplier"] = items.Count > 0 
                            ? Convert.ToDecimal(items.Average(i => i.Quantity))
                            : 0m,
                        ["TotalStockValue"] = totalValue,
                        ["AverageProductPrice"] = overallAvgPrice,
                        ["MinProductPrice"] = items.Min(i => i.Metrics["MinPrice"]),
                        ["MaxProductPrice"] = items.Max(i => i.Metrics["MaxPrice"]),
                        ["PriceRange"] = items.Max(i => i.Metrics["MaxPrice"]) - items.Min(i => i.Metrics["MinPrice"])
                    }
                }
            };
        }

        private async Task<ReportDTO> GenerateInventoryReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Use BaseProductSpecification to ensure Category is included
            var specification = new BaseProductSpecification();
            var productsResult = await _unitOfWork.Products.FindAsync(specification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = new List<ReportItemDTO>();

            foreach (var product in products)
            {
                var stockHistory = await GetTransactionHistoryAsync(product.Id, parameters, cancellationToken);
                
                items.Add(new ReportItemDTO
                {
                    Name = product.Name,
                    Category = product.Category?.Name ?? "Uncategorized",
                    Quantity = product.CurrentStock,
                    UnitPrice = product.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? product.Price,
                    TotalValue = product.CurrentStock * (product.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? product.Price),
                    LastUpdated = product.UpdatedAt ?? product.CreatedAt,
                    TransactionHistory = stockHistory.ToList()
                });
            }

            return new ReportDTO
            {
                ReportType = "Inventory",
                GeneratedDate = DateTime.UtcNow,
                StartDate = parameters.StartDate,
                EndDate = parameters.EndDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Sum(i => i.Quantity),
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count(i => i.Quantity <= 10),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0)
                }
            };
        }

        private async Task<IEnumerable<TransactionHistoryDTO>> GetTransactionHistoryAsync(
            int productId,
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Use specification pattern for better filtering
            ISpecification<StockHistory> specification;
            if (parameters.StartDate.HasValue && parameters.EndDate.HasValue)
            {
                specification = new StockHistoryByDateRangeSpecification(parameters.StartDate.Value, parameters.EndDate.Value);
            }
            else
            {
                specification = new StockHistoryByProductSpecification(productId);
            }

            var historyResult = await _unitOfWork.StockHistories.FindAsync(
                specification.Criteria,
                cancellationToken: cancellationToken);

            if (!historyResult.IsSuccess)
                return Enumerable.Empty<TransactionHistoryDTO>();

            var filteredHistory = historyResult.Value
                .Where(h => h.ProductId == productId &&
                    (!parameters.StartDate.HasValue || h.Date >= parameters.StartDate) &&
                    (!parameters.EndDate.HasValue || h.Date <= parameters.EndDate))
                .Select(h => new TransactionHistoryDTO
                {
                    Date = h.Date,
                    TransactionType = h.Type.ToDisplayString(),
                    Quantity = h.QuantityChange,
                    UnitPrice = h.UnitPrice ?? 0m,
                    ReferenceNumber = h.ReferenceNumber,
                    CreatedBy = h.CreatedBy
                });

            return filteredHistory;
        }

        private async Task<ReportDTO> GenerateStockMovementReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            var stockHistoryResult = await _unitOfWork.StockHistories
                .FindAsync(new StockHistoryByDateRangeSpecification(
                    parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1),
                    parameters.EndDate ?? DateTime.UtcNow),
                    cancellationToken: cancellationToken);

            if (!stockHistoryResult.IsSuccess)
                throw new InvalidOperationException(stockHistoryResult.Message);

            var stockHistory = stockHistoryResult.Value;
            var items = stockHistory
                .GroupBy(h => h.Product)
                .Select(g => new ReportItemDTO
                {
                    Name = g.Key.Name,
                    Category = g.Key.Category?.Name ?? "Uncategorized",
                    Quantity = g.Key.CurrentStock,
                    UnitPrice = g.Key.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? g.Key.Price,
                    TotalValue = g.Key.CurrentStock * (g.Key.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? g.Key.Price),
                    LastUpdated = g.Key.UpdatedAt ?? g.Key.CreatedAt,
                    TransactionHistory = g.Select(h => new TransactionHistoryDTO
                    {
                        Date = h.Date,
                        TransactionType = h.Type.ToString(),
                        Quantity = h.QuantityChange,
                        UnitPrice = h.UnitPrice ?? 0,
                        ReferenceNumber = h.ReferenceNumber
                    }).OrderByDescending(h => h.Date).ToList(),
                    Metrics = new Dictionary<string, decimal>
                    {
                        ["StockTurnover"] = CalculateStockTurnover(g.ToList()),
                        ["StockoutFrequency"] = CalculateStockoutFrequency(g.ToList()),
                        ["AverageDailyUsage"] = CalculateAverageDailyUsage(g.ToList())
                    }
                }).ToList();

            return new ReportDTO
            {
                ReportType = "Stock Movement",
                GeneratedDate = DateTime.UtcNow,
                StartDate = parameters.StartDate,
                EndDate = parameters.EndDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Sum(i => i.Quantity),
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count(i => i.Quantity <= 10),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["AverageStockTurnover"] = items.Average(i => i.Metrics["StockTurnover"]),
                        ["TotalStockouts"] = items.Sum(i => i.Metrics["StockoutFrequency"])
                    }
                }
            };
        }

        private async Task<ReportDTO> GenerateLowStockReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            var productsResult = await _unitOfWork.Products
                .GetAllAsync(cancellationToken: cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = products
                .Where(p => p.CurrentStock <= p.ReorderLevel)
                .Select(p => new ReportItemDTO
                {
                    Name = p.Name,
                    Category = p.Category?.Name ?? "Uncategorized",
                    Quantity = p.CurrentStock,
                    UnitPrice = p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price,
                    TotalValue = p.CurrentStock * (p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price),
                    LastUpdated = p.UpdatedAt ?? p.CreatedAt,
                    Metrics = new Dictionary<string, decimal>
                    {
                        ["ReorderLevel"] = p.ReorderLevel,
                        ["ReorderQuantity"] = CalculateReorderQuantity(p),
                        ["DaysUntilStockout"] = CalculateDaysUntilStockout(p)
                    }
                }).ToList();

            return new ReportDTO
            {
                ReportType = "Low Stock Alert",
                GeneratedDate = DateTime.UtcNow,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Count,
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count,
                    OutOfStockItems = items.Count(i => i.Quantity <= 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["TotalReorderValue"] = items.Sum(i => i.Metrics["ReorderQuantity"] * i.UnitPrice),
                        ["AverageDaysUntilStockout"] = items.Average(i => i.Metrics["DaysUntilStockout"])
                    }
                }
            };
        }

        private async Task<ReportDTO> GeneratePerformanceReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            var productsResult = await _unitOfWork.Products
                .GetAllAsync(cancellationToken: cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = products.Select(p => new ReportItemDTO
            {
                Name = p.Name,
                Category = p.Category?.Name ?? "Uncategorized",
                Quantity = p.CurrentStock,
                UnitPrice = p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price,
                TotalValue = p.CurrentStock * (p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price),
                LastUpdated = p.UpdatedAt ?? p.CreatedAt,
                Metrics = new Dictionary<string, decimal>
                {
                    ["StockTurnover"] = CalculateStockTurnover(p.StockHistory?.ToList()),
                    ["InventoryEfficiency"] = CalculateInventoryEfficiency(p),
                    ["StockoutRate"] = CalculateStockoutRate(p),
                    ["AverageDailyDemand"] = CalculateAverageDailyDemand(p)
                }
            }).ToList();

            return new ReportDTO
            {
                ReportType = "Performance Metrics",
                GeneratedDate = DateTime.UtcNow,
                StartDate = parameters.StartDate,
                EndDate = parameters.EndDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Count,
                    TotalValue = items.Sum(i => i.TotalValue),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["AverageStockTurnover"] = items.Average(i => i.Metrics["StockTurnover"]),
                        ["AverageInventoryEfficiency"] = items.Average(i => i.Metrics["InventoryEfficiency"]),
                        ["OverallStockoutRate"] = items.Average(i => i.Metrics["StockoutRate"])
                    }
                }
            };
        }

        // Helper methods for calculating metrics
        private decimal CalculateStockTurnover(List<StockHistory> history)
        {
            if (history == null || !history.Any()) return 0m;
            
            var totalStockOut = Math.Abs(history.Where(h => h.QuantityChange < 0)
                .Sum(h => h.QuantityChange));
            var averageInventory = history.Average(h => h.NewStock);
            
            return averageInventory > 0 ? totalStockOut / (decimal)averageInventory : 0m;
        }

        private decimal CalculateStockoutFrequency(List<StockHistory> history)
        {
            if (history == null || !history.Any()) return 0m;
            
            return history.Count(h => h.NewStock <= 0);
        }

        private decimal CalculateAverageDailyUsage(List<StockHistory> history)
        {
            if (history == null || !history.Any()) return 0m;
            
            var totalOut = Math.Abs(history.Where(h => h.QuantityChange < 0)
                .Sum(h => h.QuantityChange));
            var days = (history.Max(h => h.Date) - history.Min(h => h.Date)).Days + 1;
            
            return days > 0 ? totalOut / days : 0m;
        }

        private decimal CalculateReorderQuantity(Product product)
        {
            // Simple EOQ formula: sqrt((2 * Annual Demand * Order Cost) / (Holding Cost per unit per year))
            const decimal orderCost = 50m; // Standard order cost
            const decimal holdingCostPercentage = 0.2m; // 20% of unit cost
            
            var annualDemand = CalculateAnnualDemand(product);
            var holdingCost = product.Price * holdingCostPercentage;
            
            return holdingCost > 0 
                ? (decimal)Math.Sqrt((double)((2m * annualDemand * orderCost) / holdingCost))
                : 0m;
        }

        private decimal CalculateDaysUntilStockout(Product product)
        {
            if (product.CurrentStock <= 0) return 0m;
            
            var dailyUsage = CalculateAverageDailyDemand(product);
            return dailyUsage > 0 ? product.CurrentStock / dailyUsage : 999m; // 999 indicates very low usage
        }

        private decimal CalculateInventoryEfficiency(Product product)
        {
            if (product.StockHistory == null || !product.StockHistory.Any()) return 0m;
            
            var averageInventory = product.StockHistory.Average(h => h.NewStock);
            var totalSales = Math.Abs(product.StockHistory
                .Where(h => h.Type == TransactionType.StockOut)
                .Sum(h => h.QuantityChange * (h.UnitPrice ?? 
                    (product.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? product.Price))));
                
            return averageInventory > 0 ? totalSales / (decimal)averageInventory : 0m;
        }

        private decimal CalculateStockoutRate(Product product)
        {
            if (product.StockHistory == null || !product.StockHistory.Any()) return 0m;
            
            var totalDays = (decimal)(product.StockHistory.Max(h => h.Date) - product.StockHistory.Min(h => h.Date)).Days + 1;
            var stockoutDays = (decimal)product.StockHistory.Count(h => h.NewStock <= 0);
            
            return totalDays > 0 ? stockoutDays / totalDays : 0m;
        }

        private decimal CalculateAverageDailyDemand(Product product)
        {
            if (product.StockHistory == null || !product.StockHistory.Any()) return 0m;
            
            var totalDemand = Math.Abs(product.StockHistory
                .Where(h => h.Type == TransactionType.StockOut)
                .Sum(h => h.QuantityChange));
            var days = (decimal)(product.StockHistory.Max(h => h.Date) - product.StockHistory.Min(h => h.Date)).Days + 1;
            
            return days > 0 ? totalDemand / days : 0m;
        }

        private decimal CalculateAnnualDemand(Product product)
        {
            var dailyDemand = CalculateAverageDailyDemand(product);
            return dailyDemand * 365m;
        }

        private string GenerateCacheKey(ReportParameters parameters)
        {
            return $"report_{parameters.ReportType}_{parameters.StartDate:yyyyMMdd}_{parameters.EndDate:yyyyMMdd}" +
                   $"_{parameters.CategoryId}_{parameters.SupplierId}_{parameters.IncludeInactive}" +
                   $"_{parameters.IncludeTransactionHistory}";
        }
    }
} 