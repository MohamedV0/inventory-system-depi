using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Validation;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Threading;
using X.PagedList;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Data;
using AutoMapper;
using InventoryManagementSystem.Data.Specifications;
using InventoryManagementSystem.Extensions;

namespace InventoryManagementSystem.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockService> _logger;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private const int MaxPageSize = 100;

        public StockService(
            IUnitOfWork unitOfWork,
            ILogger<StockService> logger,
            IProductService productService,
            IMapper mapper,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<Result<IPagedList<StockHistoryViewModel>>> GetStockHistoryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? referenceSearch = null,
            string? reasonSearch = null,
            string? sortBy = null,
            bool ascending = true,
            int? productId = null,
            int? transactionType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                // Build the filter expression
                Expression<Func<StockHistory, bool>>? predicate = null;
                
                if (productId.HasValue)
                {
                    predicate = sh => sh.ProductId == productId.Value;
                }
                
                if (transactionType.HasValue)
                {
                    var type = (TransactionType)transactionType.Value;
                    Expression<Func<StockHistory, bool>> typePredicate = sh => sh.Type == type;
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, typePredicate) 
                        : typePredicate;
                }

                // Handle date range filtering
                if (startDate.HasValue)
                {
                    var startDateValue = startDate.Value.Date; // Use Date to ignore time component
                    _logger.LogInformation("Filtering by start date: {StartDate}", startDateValue.ToString("yyyy-MM-dd"));
                    Expression<Func<StockHistory, bool>> startDatePredicate = sh => sh.Date >= startDateValue;
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, startDatePredicate) 
                        : startDatePredicate;
                }

                if (endDate.HasValue)
                {
                    var endDateValue = endDate.Value.Date.AddDays(1).AddSeconds(-1); // End of the selected day
                    _logger.LogInformation("Filtering by end date: {EndDate}", endDateValue.ToString("yyyy-MM-dd HH:mm:ss"));
                    Expression<Func<StockHistory, bool>> endDatePredicate = sh => sh.Date <= endDateValue;
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, endDatePredicate) 
                        : endDatePredicate;
                }

                // Handle reference number search
                if (!string.IsNullOrWhiteSpace(referenceSearch))
                {
                    var refSearchTerm = referenceSearch.Trim().ToLower();
                    Expression<Func<StockHistory, bool>> refPredicate = sh => 
                        sh.ReferenceNumber != null && sh.ReferenceNumber.ToLower().Contains(refSearchTerm);
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, refPredicate) 
                        : refPredicate;
                }

                // Handle reason search
                if (!string.IsNullOrWhiteSpace(reasonSearch))
                {
                    var reasonSearchTerm = reasonSearch.Trim().ToLower();
                    Expression<Func<StockHistory, bool>> reasonPredicate = sh => 
                        sh.Reason != null && sh.Reason.ToLower().Contains(reasonSearchTerm);
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, reasonPredicate) 
                        : reasonPredicate;
                }

                // Handle general search term
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    
                    Expression<Func<StockHistory, bool>> searchPredicate = sh => 
                        sh.Product.Name.ToLower().Contains(searchTerm) ||
                        (sh.ReferenceNumber != null && sh.ReferenceNumber.ToLower().Contains(searchTerm)) ||
                        (sh.Reason != null && sh.Reason.ToLower().Contains(searchTerm));
                    
                    predicate = predicate != null 
                        ? ExpressionHelpers.AndAlso(predicate, searchPredicate) 
                        : searchPredicate;
                }

                // Define ordering
                Func<IQueryable<StockHistory>, IOrderedQueryable<StockHistory>>? orderBy = sortBy?.ToLower() switch
                {
                    "date" => query => ascending ? query.OrderBy(sh => sh.Date) : query.OrderByDescending(sh => sh.Date),
                    "product" => query => ascending ? query.OrderBy(sh => sh.Product.Name) : query.OrderByDescending(sh => sh.Product.Name),
                    "quantity" => query => ascending ? query.OrderBy(sh => sh.QuantityChange) : query.OrderByDescending(sh => sh.QuantityChange),
                    "type" => query => ascending ? query.OrderBy(sh => sh.Type) : query.OrderByDescending(sh => sh.Type),
                    "price" => query => ascending ? query.OrderBy(sh => sh.UnitPrice) : query.OrderByDescending(sh => sh.UnitPrice),
                    "reference" => query => ascending ? query.OrderBy(sh => sh.ReferenceNumber) : query.OrderByDescending(sh => sh.ReferenceNumber),
                    _ => query => query.OrderByDescending(sh => sh.Date), // Default to date desc
                };

                var queryOptions = new QueryOptions 
                { 
                    TrackingEnabled = false,
                    SplitQuery = false
                };

                var result = await _unitOfWork.StockHistories.GetPagedAsync(
                    page, 
                    pageSize, 
                    predicate, 
                    "Product",
                    queryOptions,
                    cancellationToken
                );

                if (!result.IsSuccess)
                    return Result<IPagedList<StockHistoryViewModel>>.Failure(result.Message);

                var pagedList = new StaticPagedList<StockHistoryViewModel>(
                    _mapper.Map<IEnumerable<StockHistoryViewModel>>(result.Value).ToList(),
                    result.Value.PageNumber,
                    result.Value.PageSize,
                    result.Value.TotalItemCount
                );
                
                return Result<IPagedList<StockHistoryViewModel>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged StockHistory");
                return Result<IPagedList<StockHistoryViewModel>>.Failure("Error retrieving paged StockHistory list");
            }
        }

        public async Task<Result<StockHistoryDetailViewModel>> GetStockHistoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Use the specialized method that includes Product and Category
                var result = await _unitOfWork.StockHistories.GetByIdWithDetailsAsync(id, cancellationToken);
                if (!result.IsSuccess)
                {
                    return Result<StockHistoryDetailViewModel>.NotFound("Stock history entry");
                }

                var viewModel = _mapper.Map<StockHistoryDetailViewModel>(result.Value);
                return Result<StockHistoryDetailViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history entry: {ErrorMessage}", ex.Message);
                return Result<StockHistoryDetailViewModel>.Failure("Error retrieving stock history entry");
            }
        }

        public async Task<Result<StockHistoryViewModel>> AddStockAsync(AddStockViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<StockHistoryViewModel>.ValidationError("Stock movement model cannot be null");

            try
            {
                // Validate product exists and get current stock
                var productResult = await _unitOfWork.Products.GetByIdAsync(model.ProductId);
                if (!productResult.IsSuccess)
                    return Result<StockHistoryViewModel>.NotFound("Product");

                var product = productResult.Value;
                int previousStock = product.CurrentStock;
                int newStock = previousStock + model.Quantity;

                // Update product stock
                product.CurrentStock = newStock;
                await _unitOfWork.Products.UpdateAsync(product);

                // Create stock history record
                var stockHistory = new StockHistory
                {
                    ProductId = model.ProductId,
                    QuantityChange = model.Quantity,
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    Reason = model.Reason,
                    Date = DateTime.UtcNow,
                    ReferenceNumber = model.Reference,
                    UnitPrice = model.UnitPrice,
                    Notes = model.Notes,
                    Type = TransactionType.StockIn
                };

                var result = await _unitOfWork.StockHistories.AddAsync(stockHistory);
                if (!result.IsSuccess)
                    return Result<StockHistoryViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync();

                var viewModel = _mapper.Map<StockHistoryViewModel>(stockHistory);
                viewModel.ProductName = product.Name;
                
                return Result<StockHistoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock for product ID {ProductId}", model.ProductId);
                return Result<StockHistoryViewModel>.Failure("Error adding stock");
            }
        }

        public async Task<Result<StockHistoryViewModel>> RemoveStockAsync(RemoveStockViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<StockHistoryViewModel>.ValidationError("Stock movement model cannot be null");

            try
            {
                // Validate product exists and get current stock
                var productResult = await _unitOfWork.Products.GetByIdAsync(model.ProductId);
                if (!productResult.IsSuccess)
                    return Result<StockHistoryViewModel>.NotFound("Product");

                var product = productResult.Value;
                int previousStock = product.CurrentStock;
                
                // Check if there's enough stock
                if (previousStock < model.Quantity)
                    return Result<StockHistoryViewModel>.ValidationError(
                        $"Not enough stock available. Current stock: {previousStock}, Requested: {model.Quantity}");

                int newStock = previousStock - model.Quantity;

                // Update product stock
                product.CurrentStock = newStock;
                await _unitOfWork.Products.UpdateAsync(product);

                // Create stock history record
                var stockHistory = new StockHistory
                {
                    ProductId = model.ProductId,
                    QuantityChange = -model.Quantity, // Negative for stock out
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    Reason = model.Reason,
                    Date = DateTime.UtcNow,
                    ReferenceNumber = model.Reference,
                    Notes = model.Notes,
                    UnitPrice = null, // No unit price for stock removal
                    Type = TransactionType.StockOut
                };

                try 
                {
                    var result = await _unitOfWork.StockHistories.AddAsync(stockHistory);
                    if (!result.IsSuccess)
                        return Result<StockHistoryViewModel>.Failure(result.Message);

                    await _unitOfWork.SaveChangesAsync();

                    // Use the centralized notification method
                    await _notificationService.CheckAndCreateStockNotificationsAsync(
                        model.ProductId,
                        newStock,
                        product.ReorderLevel,
                        cancellationToken);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error saving StockHistory for product ID {ProductId}: {ErrorMessage}", 
                        model.ProductId, dbEx.InnerException?.Message ?? dbEx.Message);
                    return Result<StockHistoryViewModel>.Failure("Error saving stock history to database. Please try again.");
                }

                var viewModel = _mapper.Map<StockHistoryViewModel>(stockHistory);
                viewModel.ProductName = product.Name;
                
                return Result<StockHistoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock for product ID {ProductId}", model.ProductId);
                return Result<StockHistoryViewModel>.Failure("Error removing stock");
            }
        }

        public async Task<Result<ProductStockHistoryViewModel>> GetProductStockHistoryAsync(int productId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get product information
                var productResult = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
                if (!productResult.IsSuccess)
                    return Result<ProductStockHistoryViewModel>.NotFound("Product");

                var product = productResult.Value;

                // Get stock history for the product
                Expression<Func<StockHistory, bool>> predicate = sh => sh.ProductId == productId;
                
                var queryOptions = new QueryOptions { TrackingEnabled = false };
                
                var result = await _unitOfWork.StockHistories.GetPagedAsync(
                    1, 
                    int.MaxValue, 
                    predicate, 
                    "Product",
                    queryOptions,
                    cancellationToken
                );

                if (!result.IsSuccess)
                    return Result<ProductStockHistoryViewModel>.Failure(result.Message);

                var stockHistoryList = _mapper.Map<IEnumerable<StockHistoryViewModel>>(result.Value).ToList();
                
                // Create the product stock history view model
                var viewModel = new ProductStockHistoryViewModel
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    CategoryName = product.Category?.Name ?? "Unknown",
                    CurrentStock = product.CurrentStock,
                    ReorderLevel = product.ReorderLevel,
                    TargetStockLevel = product.MaximumStockLevel,
                    CreatedAt = product.CreatedAt,
                    StockHistory = stockHistoryList
                };

                return Result<ProductStockHistoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history for product ID {ProductId}", productId);
                return Result<ProductStockHistoryViewModel>.Failure("Error retrieving product stock history");
            }
        }

        public async Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var lowStockPredicate = (Expression<Func<Product, bool>>)(p => p.CurrentStock <= p.ReorderLevel);
                
                var queryOptions = new QueryOptions { TrackingEnabled = false };
                
                var result = await _unitOfWork.Products.GetPagedAsync(
                    1, 
                    int.MaxValue, 
                    lowStockPredicate, 
                    "Category",
                    queryOptions,
                    cancellationToken
                );

                if (!result.IsSuccess)
                    return Result<IEnumerable<ProductListItemViewModel>>.Failure(result.Message);

                var products = result.Value.Select(p => new ProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    CurrentStock = p.CurrentStock,
                    ReorderLevel = p.ReorderLevel,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "Unknown",
                    NeedsReorder = p.CurrentStock <= p.ReorderLevel
                }).ToList();

                return Result<IEnumerable<ProductListItemViewModel>>.Success(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products needing reorder");
                return Result<IEnumerable<ProductListItemViewModel>>.Failure("Error retrieving products needing reorder");
            }
        }

        public async Task<Result<StockHistoryViewModel>> AdjustStockAsync(AdjustStockViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<StockHistoryViewModel>.ValidationError("Stock adjustment model cannot be null");

            try
            {
                // Validate product exists and get current stock
                var productResult = await _unitOfWork.Products.GetByIdAsync(model.ProductId);
                if (!productResult.IsSuccess)
                    return Result<StockHistoryViewModel>.NotFound("Product");

                var product = productResult.Value;
                int previousStock = product.CurrentStock;
                int newStock = model.NewQuantity;
                int quantityChange = newStock - previousStock;

                // No need to update if the quantity is the same
                if (quantityChange == 0)
                {
                    return Result<StockHistoryViewModel>.Success(new StockHistoryViewModel
                    {
                        ProductId = model.ProductId,
                        ProductName = product.Name,
                        QuantityChange = 0,
                        PreviousStock = previousStock,
                        NewStock = newStock,
                        Reason = model.Reason,
                        Type = TransactionType.Adjustment
                    });
                }

                // Update product stock
                product.CurrentStock = newStock;
                await _unitOfWork.Products.UpdateAsync(product);

                // Create stock history record
                var stockHistory = new StockHistory
                {
                    ProductId = model.ProductId,
                    QuantityChange = quantityChange,
                    PreviousStock = previousStock,
                    NewStock = newStock,
                    Reason = model.Reason,
                    Date = DateTime.UtcNow,
                    ReferenceNumber = model.Reference,
                    Notes = model.Notes,
                    UnitPrice = null, // No unit price for adjustments
                    Type = TransactionType.Adjustment
                };

                try 
                {
                    var result = await _unitOfWork.StockHistories.AddAsync(stockHistory);
                    if (!result.IsSuccess)
                        return Result<StockHistoryViewModel>.Failure(result.Message);

                    await _unitOfWork.SaveChangesAsync();

                    // Use the centralized notification method
                    await _notificationService.CheckAndCreateStockNotificationsAsync(
                        model.ProductId,
                        newStock,
                        product.ReorderLevel,
                        cancellationToken);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error saving StockHistory for product ID {ProductId}: {ErrorMessage}", 
                        model.ProductId, dbEx.InnerException?.Message ?? dbEx.Message);
                    return Result<StockHistoryViewModel>.Failure("Error saving stock history to database. Please try again.");
                }

                var viewModel = _mapper.Map<StockHistoryViewModel>(stockHistory);
                viewModel.ProductName = product.Name;
                
                return Result<StockHistoryViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adjusting stock for product ID {ProductId}", model.ProductId);
                return Result<StockHistoryViewModel>.Failure("Error adjusting stock");
            }
        }

        public async Task<Result<IEnumerable<LowStockViewModel>>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var lowStockPredicate = (Expression<Func<Product, bool>>)(p => p.CurrentStock <= p.ReorderLevel);
                
                var queryOptions = new QueryOptions { TrackingEnabled = false };
                
                var result = await _unitOfWork.Products.GetPagedAsync(
                    1, 
                    int.MaxValue, 
                    lowStockPredicate, 
                    "Category,ProductSuppliers.Supplier",
                    queryOptions,
                    cancellationToken
                );

                if (!result.IsSuccess)
                    return Result<IEnumerable<LowStockViewModel>>.Failure(result.Message);

                var lowStockItems = result.Value.Select(p => new LowStockViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category?.Name ?? "Unknown",
                    StockLevel = p.CurrentStock,
                    ReorderLevel = p.ReorderLevel,
                    TargetStockLevel = p.MaximumStockLevel,
                    SupplierName = p.ProductSuppliers
                        ?.Where(ps => ps.IsPreferredSupplier)
                        .Select(ps => ps.Supplier?.Name)
                        .FirstOrDefault() ?? "Not specified"
                }).ToList();

                return Result<IEnumerable<LowStockViewModel>>.Success(lowStockItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock items");
                return Result<IEnumerable<LowStockViewModel>>.Failure("Error retrieving low stock items");
            }
        }
    }
} 