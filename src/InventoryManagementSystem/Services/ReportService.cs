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
            var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = parameters.EndDate ?? DateTime.UtcNow;

            // Create base specification for active products
            var specification = new BaseProductSpecification();
            
            // Create a list of specifications to combine
            var specifications = new List<BaseProductSpecification>();
            
            // Add category filter if specified
            if (parameters.CategoryId.HasValue)
            {
                specifications.Add(new ProductsByCategorySpecification(parameters.CategoryId.Value));
            }
            
            // Add supplier filter if specified
            if (parameters.SupplierId.HasValue)
            {
                specifications.Add(new ProductBySupplierSpecification(parameters.SupplierId.Value));
            }
            
            // Add active/inactive filter
            if (!parameters.IncludeInactive)
            {
                specifications.Add(new ActiveProductsSpecification());
            }
            
            // Combine all specifications
            var combinedSpecification = specification;
            
            // Only combine if we have additional specifications
            if (specifications.Any())
            {
                combinedSpecification = specifications.Aggregate((current, next) => 
                    new CombinedProductSpecification(current, next));
            }

            var productsResult = await _unitOfWork.Products.FindAsync(combinedSpecification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;

            // Get stock history for analysis
            var stockHistorySpec = new StockHistoryByDateRangeSpecification(startDate, endDate);
            var stockHistoryResult = await _unitOfWork.StockHistories.FindAsync(stockHistorySpec, cancellationToken);

            if (!stockHistoryResult.IsSuccess)
                throw new InvalidOperationException(stockHistoryResult.Message);

            var stockHistory = stockHistoryResult.Value;

            // Data validation and normalization
            var items = products.Select(p =>
            {
                // Get product's stock history
                var productHistory = stockHistory.Where(h => h.ProductId == p.Id).ToList();
                
                // Calculate stock metrics
                var stockIn = productHistory.Where(h => h.Type == TransactionType.StockIn)
                    .Sum(h => h.QuantityChange);
                var stockOut = Math.Abs(productHistory.Where(h => h.Type == TransactionType.StockOut)
                    .Sum(h => h.QuantityChange));
                var avgStock = productHistory.Any() 
                    ? (decimal)productHistory.Average(h => h.NewStock)
                    : p.CurrentStock;
                
                // Calculate stock velocity (units per day)
                var periodDays = (decimal)(endDate - startDate).TotalDays;
                var stockVelocity = periodDays > 0 ? stockOut / periodDays : 0;
                
                // Calculate days of inventory
                var daysOfInventory = stockVelocity > 0 
                    ? p.CurrentStock / stockVelocity
                    : (p.CurrentStock > 0 ? 999m : 0m);
                
                // Calculate inventory efficiency
                var inventoryEfficiency = CalculateInventoryEfficiency(p);
                
                // Calculate stock turnover
                var turnoverRate = avgStock > 0 ? stockOut / avgStock : 0;
                
                // Validate and normalize price
                var normalizedPrice = ValidatePrice(p.Price);
                var normalizedCost = ValidatePrice(p.Cost);
                
                return new ReportItemDTO
                {
                    Name = p.Name,
                    Category = p.Category?.Name ?? "Uncategorized",
                    Quantity = p.CurrentStock,
                    UnitPrice = normalizedPrice,
                    TotalValue = p.CurrentStock * normalizedPrice,
                    LastUpdated = p.UpdatedAt ?? p.CreatedAt,
                    AdditionalFields = new Dictionary<string, string>
                    {
                        ["SKU"] = p.SKU,
                        ["Status"] = GetStockStatus(p),
                        ["Supplier"] = p.ProductSuppliers?.FirstOrDefault()?.Supplier?.Name ?? "Not Specified",
                        ["LastRestockDate"] = productHistory
                            .Where(h => h.Type == TransactionType.StockIn)
                            .OrderByDescending(h => h.Date)
                            .FirstOrDefault()?.Date.ToString("yyyy-MM-dd") ?? "N/A"
                    },
                    Metrics = new Dictionary<string, decimal>
                    {
                        ["StockIn"] = stockIn,
                        ["StockOut"] = stockOut,
                        ["AverageStock"] = avgStock,
                        ["StockVelocity"] = stockVelocity,
                        ["DaysOfInventory"] = daysOfInventory,
                        ["InventoryEfficiency"] = inventoryEfficiency,
                        ["TurnoverRate"] = turnoverRate,
                        ["StockoutRate"] = CalculateStockoutRate(p),
                        ["ProfitMargin"] = normalizedPrice > 0 
                            ? ((normalizedPrice - normalizedCost) / normalizedPrice) * 100 
                            : 0,
                        ["InventoryValue"] = p.CurrentStock * normalizedPrice,
                        ["StockHealthScore"] = CalculateStockHealthScore(p, stockVelocity, daysOfInventory)
                    }
                };
            }).ToList();

            // Calculate summary metrics
            var totalInventoryValue = items.Sum(i => i.Metrics["InventoryValue"]);
            var averageStockHealth = items.Any() ? items.Average(i => i.Metrics["StockHealthScore"]) : 0;
            var averageTurnover = items.Any() ? items.Average(i => i.Metrics["TurnoverRate"]) : 0;

            return new ReportDTO
            {
                ReportType = "Inventory Overview",
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Sum(i => i.Quantity),
                    TotalValue = totalInventoryValue,
                    LowStockItems = items.Count(i => i.AdditionalFields["Status"].Contains("Low")),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["AverageStockHealthScore"] = averageStockHealth,
                        ["AverageTurnoverRate"] = averageTurnover,
                        ["TotalStockIn"] = items.Sum(i => i.Metrics["StockIn"]),
                        ["TotalStockOut"] = items.Sum(i => i.Metrics["StockOut"]),
                        ["AverageInventoryEfficiency"] = items.Any() ? items.Average(i => i.Metrics["InventoryEfficiency"]) : 0,
                        ["AverageDaysOfInventory"] = items.Any() ? items.Average(i => i.Metrics["DaysOfInventory"]) : 0,
                        ["OverallStockoutRate"] = items.Any() ? items.Average(i => i.Metrics["StockoutRate"]) : 0,
                        ["AverageProfitMargin"] = items.Any() ? items.Average(i => i.Metrics["ProfitMargin"]) : 0
                    }
                }
            };
        }

        private decimal CalculateStockHealthScore(Product product, decimal stockVelocity, decimal daysOfInventory)
        {
            // Stock health score (0-100) based on multiple factors
            decimal score = 100;

            // Deduct points for being below reorder level
            if (product.CurrentStock <= product.ReorderLevel)
                score -= 30;

            // Deduct points for being below minimum stock
            if (product.CurrentStock <= product.MinimumStockLevel)
                score -= 20;

            // Deduct points for being out of stock
            if (product.CurrentStock <= 0)
                score -= 50;

            // Deduct points for being overstocked
            if (product.CurrentStock >= product.MaximumStockLevel)
                score -= 15;

            // Add points for good stock velocity
            if (stockVelocity > 0)
                score += 10;

            // Add points for healthy days of inventory (between 15 and 60 days)
            if (daysOfInventory >= 15 && daysOfInventory <= 60)
                score += 10;

            // Ensure score stays within 0-100 range
            return Math.Max(0, Math.Min(100, score));
        }

        private decimal ValidatePrice(decimal price)
        {
            // Maximum reasonable price threshold (configurable)
            const decimal maxReasonablePrice = 10000m;
            
            if (price < 0)
                return 0;
            
            if (price > maxReasonablePrice)
            {
                _logger.LogWarning("Unusually high price detected: {Price}", price);
                // Could implement more sophisticated price validation here
                // For now, cap at maximum reasonable price
                return maxReasonablePrice;
            }
            
            return price;
        }

        private string GetStockStatus(Product product)
        {
            if (product.CurrentStock <= 0)
                return "Out of Stock";
            if (product.CurrentStock <= product.MinimumStockLevel)
                return "Critically Low";
            if (product.CurrentStock <= product.ReorderLevel)
                return "Low Stock";
            if (product.CurrentStock >= product.MaximumStockLevel)
                return "Overstocked";
            if (product.CurrentStock >= product.ReorderLevel * 2)
                return "Well Stocked";
            return "Adequate";
        }

        private decimal CalculateMedianPrice(List<ReportItemDTO> items)
        {
            if (!items.Any())
                return 0;

            var sortedPrices = items.Select(i => i.UnitPrice).OrderBy(p => p).ToList();
            var mid = sortedPrices.Count / 2;

            if (sortedPrices.Count % 2 == 0)
                return (sortedPrices[mid - 1] + sortedPrices[mid]) / 2;
            
            return sortedPrices[mid];
        }

        private async Task<ReportDTO> GenerateProductsReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Create base specification for active products
            var specification = new BaseProductSpecification();
            
            // Create a list of specifications to combine
            var specifications = new List<BaseProductSpecification>();
            
            // Add category filter if specified
            if (parameters.CategoryId.HasValue)
            {
                specifications.Add(new ProductsByCategorySpecification(parameters.CategoryId.Value));
            }
            
            // Add supplier filter if specified
            if (parameters.SupplierId.HasValue)
            {
                specifications.Add(new ProductBySupplierSpecification(parameters.SupplierId.Value));
            }
            
            // Add active/inactive filter
            if (!parameters.IncludeInactive)
            {
                specifications.Add(new ActiveProductsSpecification());
            }
            
            // Combine all specifications
            var combinedSpecification = specification;
            
            // Only combine if we have additional specifications
            if (specifications.Any())
            {
                combinedSpecification = specifications.Aggregate((current, next) => 
                    new CombinedProductSpecification(current, next));
            }

            var productsResult = await _unitOfWork.Products.FindAsync(combinedSpecification, cancellationToken);

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
            var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = parameters.EndDate ?? DateTime.UtcNow;

            // Get stock history for the date range
            var stockHistorySpec = new StockHistoryByDateRangeSpecification(startDate, endDate);
            var stockHistoryResult = await _unitOfWork.StockHistories.FindAsync(stockHistorySpec, cancellationToken);

            if (!stockHistoryResult.IsSuccess)
                throw new InvalidOperationException(stockHistoryResult.Message);

            var stockHistory = stockHistoryResult.Value;

            // Get products that had transactions in this period
            var productIds = stockHistory.Select(sh => sh.ProductId).Distinct().ToList();
            
            // Create a new specification for products with transactions
            var specification = new BaseProductSpecification();
            var productSpec = new ProductByIdsSpecification(productIds);
            
            var productsResult = await _unitOfWork.Products.FindAsync(productSpec, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;
            var items = new List<ReportItemDTO>();

            foreach (var product in products)
            {
                var productHistory = stockHistory.Where(h => h.ProductId == product.Id).ToList();
                
                // Calculate metrics first
                var stockIn = productHistory.Where(h => h.Type == TransactionType.StockIn).Sum(h => h.QuantityChange);
                var stockOut = Math.Abs(productHistory.Where(h => h.Type == TransactionType.StockOut).Sum(h => h.QuantityChange));
                var avgStock = (decimal)productHistory.Average(h => h.NewStock);
                var turnoverRate = avgStock > 0 ? (stockOut / avgStock) : 0;
                var stockVelocity = (decimal)(endDate - startDate).TotalDays > 0 
                    ? stockOut / (decimal)(endDate - startDate).TotalDays 
                    : 0;
                
                items.Add(new ReportItemDTO
                {
                    Name = product.Name,
                    Category = product.Category?.Name ?? "Uncategorized",
                    Quantity = product.CurrentStock,
                    UnitPrice = product.Price,
                    TotalValue = product.CurrentStock * product.Price,
                    LastUpdated = product.UpdatedAt ?? product.CreatedAt,
                    TransactionHistory = productHistory.Select(h => new TransactionHistoryDTO
                    {
                        Date = h.Date,
                        TransactionType = h.Type.ToDisplayString(),
                        Quantity = h.QuantityChange,
                        UnitPrice = h.UnitPrice ?? 0m,
                        ReferenceNumber = h.ReferenceNumber,
                        CreatedBy = h.CreatedBy
                    }).ToList(),
                    Metrics = new Dictionary<string, decimal>
                    {
                        ["TurnoverRate"] = turnoverRate,
                        ["StockVelocity"] = stockVelocity,
                        ["StockIn"] = stockIn,
                        ["StockOut"] = stockOut
                    }
                });
            }

            return new ReportDTO
            {
                ReportType = "Inventory",
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Sum(i => i.Quantity),
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count(i => i.Quantity <= 10),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["AverageTurnoverRate"] = items.Any() ? items.Average(i => i.Metrics["TurnoverRate"]) : 0,
                        ["AverageStockVelocity"] = items.Any() ? items.Average(i => i.Metrics["StockVelocity"]) : 0,
                        ["TotalStockIn"] = items.Sum(i => i.Metrics["StockIn"]),
                        ["TotalStockOut"] = items.Sum(i => i.Metrics["StockOut"])
                    }
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
            // Create base specification for active products
            var specification = new BaseProductSpecification();
            
            // Create a list of specifications to combine
            var specifications = new List<BaseProductSpecification>();
            
            // Add category filter if specified
            if (parameters.CategoryId.HasValue)
            {
                specifications.Add(new ProductsByCategorySpecification(parameters.CategoryId.Value));
            }
            
            // Add supplier filter if specified
            if (parameters.SupplierId.HasValue)
            {
                specifications.Add(new ProductBySupplierSpecification(parameters.SupplierId.Value));
            }
            
            // Add active/inactive filter
            if (!parameters.IncludeInactive)
            {
                specifications.Add(new ActiveProductsSpecification());
            }
            
            // Combine all specifications
            var combinedSpecification = specification;
            
            // Only combine if we have additional specifications
            if (specifications.Any())
            {
                combinedSpecification = specifications.Aggregate((current, next) => 
                    new CombinedProductSpecification(current, next));
            }

            var productsResult = await _unitOfWork.Products.FindAsync(combinedSpecification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;

            // Get stock history for the date range
            var stockHistoryResult = await _unitOfWork.StockHistories
                .FindAsync(new StockHistoryByDateRangeSpecification(
                    parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1),
                    parameters.EndDate ?? DateTime.UtcNow),
                    cancellationToken: cancellationToken);

            if (!stockHistoryResult.IsSuccess)
                throw new InvalidOperationException(stockHistoryResult.Message);

            var stockHistory = stockHistoryResult.Value;
            var items = products
                .Select(p =>
                {
                    var productHistory = stockHistory.Where(h => h.ProductId == p.Id).ToList();
                    return new ReportItemDTO
                    {
                        Name = p.Name,
                        Category = p.Category?.Name ?? "Uncategorized",
                        Quantity = p.CurrentStock,
                        UnitPrice = p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price,
                        TotalValue = p.CurrentStock * (p.ProductSuppliers?.OrderBy(ps => ps.UnitPrice).FirstOrDefault()?.UnitPrice ?? p.Price),
                        LastUpdated = p.UpdatedAt ?? p.CreatedAt,
                        TransactionHistory = productHistory.Select(h => new TransactionHistoryDTO
                        {
                            Date = h.Date,
                            TransactionType = h.Type.ToString(),
                            Quantity = h.QuantityChange,
                            UnitPrice = h.UnitPrice ?? 0,
                            ReferenceNumber = h.ReferenceNumber
                        }).OrderByDescending(h => h.Date).ToList(),
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["StockTurnover"] = CalculateStockTurnover(productHistory),
                            ["StockoutFrequency"] = CalculateStockoutFrequency(productHistory),
                            ["AverageDailyUsage"] = CalculateAverageDailyUsage(productHistory)
                        }
                    };
                })
                .Where(item => item.TransactionHistory.Any()) // Only include products with transactions
                .ToList();

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
                        ["AverageStockTurnover"] = items.Any() ? items.Average(i => i.Metrics["StockTurnover"]) : 0,
                        ["TotalStockouts"] = items.Sum(i => i.Metrics["StockoutFrequency"])
                    }
                }
            };
        }

        private async Task<ReportDTO> GenerateLowStockReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken)
        {
            // Create base specification for active products
            var specification = new BaseProductSpecification();
            
            // Create a list of specifications to combine
            var specifications = new List<BaseProductSpecification>();
            
            // Add category filter if specified
            if (parameters.CategoryId.HasValue)
            {
                specifications.Add(new ProductsByCategorySpecification(parameters.CategoryId.Value));
            }
            
            // Add supplier filter if specified
            if (parameters.SupplierId.HasValue)
            {
                specifications.Add(new ProductBySupplierSpecification(parameters.SupplierId.Value));
            }
            
            // Add active/inactive filter
            if (!parameters.IncludeInactive)
            {
                specifications.Add(new ActiveProductsSpecification());
            }
            
            // Combine all specifications
            var combinedSpecification = specification;
            
            // Only combine if we have additional specifications
            if (specifications.Any())
            {
                combinedSpecification = specifications.Aggregate((current, next) => 
                    new CombinedProductSpecification(current, next));
            }

            var productsResult = await _unitOfWork.Products.FindAsync(combinedSpecification, cancellationToken);

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
            var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = parameters.EndDate ?? DateTime.UtcNow;

            // Create base specification for active products
            var specification = new BaseProductSpecification();
            
            // Create a list of specifications to combine
            var specifications = new List<BaseProductSpecification>();
            
            // Add category filter if specified
            if (parameters.CategoryId.HasValue)
            {
                specifications.Add(new ProductsByCategorySpecification(parameters.CategoryId.Value));
            }
            
            // Add supplier filter if specified
            if (parameters.SupplierId.HasValue)
            {
                specifications.Add(new ProductBySupplierSpecification(parameters.SupplierId.Value));
            }
            
            // Add active/inactive filter
            if (!parameters.IncludeInactive)
            {
                specifications.Add(new ActiveProductsSpecification());
            }
            
            // Combine all specifications
            var combinedSpecification = specification;
            
            // Only combine if we have additional specifications
            if (specifications.Any())
            {
                combinedSpecification = specifications.Aggregate((current, next) => 
                    new CombinedProductSpecification(current, next));
            }

            var productsResult = await _unitOfWork.Products.FindAsync(combinedSpecification, cancellationToken);

            if (!productsResult.IsSuccess)
                throw new InvalidOperationException(productsResult.Message);

            var products = productsResult.Value;

            // Get stock history for the date range
            var stockHistorySpec = new StockHistoryByDateRangeSpecification(startDate, endDate);
            var stockHistoryResult = await _unitOfWork.StockHistories.FindAsync(stockHistorySpec, cancellationToken);

            if (!stockHistoryResult.IsSuccess)
                throw new InvalidOperationException(stockHistoryResult.Message);

            var stockHistory = stockHistoryResult.Value;
            var items = new List<ReportItemDTO>();

            foreach (var product in products)
            {
                // Filter stock history for this product within date range
                var productHistory = stockHistory.Where(h => h.ProductId == product.Id).ToList();
                
                // Only include products that had transactions in this period
                if (productHistory.Any())
                {
                    var stockIn = productHistory.Where(h => h.Type == TransactionType.StockIn).Sum(h => h.QuantityChange);
                    var stockOut = Math.Abs(productHistory.Where(h => h.Type == TransactionType.StockOut).Sum(h => h.QuantityChange));
                    var avgStock = (decimal)productHistory.Average(h => h.NewStock);
                    
                    items.Add(new ReportItemDTO
                    {
                        Name = product.Name,
                        Category = product.Category?.Name ?? "Uncategorized",
                        Quantity = product.CurrentStock,
                        UnitPrice = product.Price,
                        TotalValue = product.CurrentStock * product.Price,
                        LastUpdated = product.UpdatedAt ?? product.CreatedAt,
                        Metrics = new Dictionary<string, decimal>
                        {
                            ["StockIn"] = stockIn,
                            ["StockOut"] = stockOut,
                            ["TurnoverRate"] = avgStock > 0 ? (stockOut / avgStock) : 0,
                            ["StockVelocity"] = (decimal)(endDate - startDate).TotalDays > 0 
                                ? stockOut / (decimal)(endDate - startDate).TotalDays 
                                : 0
                        }
                    });
                }
            }

            return new ReportDTO
            {
                ReportType = "Performance",
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                Items = items,
                Summary = new ReportSummaryDTO
                {
                    TotalItems = items.Count,
                    TotalValue = items.Sum(i => i.TotalValue),
                    LowStockItems = items.Count(i => i.Quantity <= 10),
                    OutOfStockItems = items.Count(i => i.Quantity <= 0),
                    AdditionalMetrics = new Dictionary<string, decimal>
                    {
                        ["AverageTurnoverRate"] = items.Any() ? items.Average(i => i.Metrics["TurnoverRate"]) : 0,
                        ["AverageStockVelocity"] = items.Any() ? items.Average(i => i.Metrics["StockVelocity"]) : 0,
                        ["TotalStockIn"] = items.Sum(i => i.Metrics["StockIn"]),
                        ["TotalStockOut"] = items.Sum(i => i.Metrics["StockOut"])
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