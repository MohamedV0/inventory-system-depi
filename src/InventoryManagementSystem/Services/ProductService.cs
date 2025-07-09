using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Validation;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using System.Linq.Expressions;
using InventoryManagementSystem.Data.Context;
using Microsoft.Extensions.Logging;
using X.PagedList;
using InventoryManagementSystem.Helpers;
using System.Threading;
using AutoMapper;
using InventoryManagementSystem.Extensions;
using InventoryManagementSystem.Data.Specifications;

namespace InventoryManagementSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly ProductValidator _validator;
        private readonly IMapper _mapper;
        private const int MaxPageSize = 100;

        public ProductService(
            IUnitOfWork unitOfWork,
            ILogger<ProductService> logger,
            ProductValidator validator,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            int? categoryId = null,
            bool? lowStock = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                Expression<Func<Product, bool>>? predicate = null;
                var predicates = new List<Expression<Func<Product, bool>>>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    predicates.Add(p => p.Name.ToLower().Contains(searchTerm) ||
                                     p.SKU.ToLower().Contains(searchTerm) ||
                                     p.Description.ToLower().Contains(searchTerm));
                }

                if (categoryId.HasValue)
                {
                    predicates.Add(p => p.CategoryId == categoryId.Value);
                }

                if (lowStock.HasValue && lowStock.Value)
                {
                    predicates.Add(p => p.CurrentStock <= p.ReorderLevel);
                }

                if (predicates.Any())
                {
                    predicate = predicates.Aggregate((current, next) =>
                        Expression.Lambda<Func<Product, bool>>(
                            Expression.AndAlso(current.Body, next.Body),
                            current.Parameters));
                }

                Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = sortBy?.ToLower() switch
                {
                    "name" => query => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    "sku" => query => ascending ? query.OrderBy(p => p.SKU) : query.OrderByDescending(p => p.SKU),
                    "price" => query => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                    "stock" => query => ascending ? query.OrderBy(p => p.CurrentStock) : query.OrderByDescending(p => p.CurrentStock),
                    "category" => query => ascending ? query.OrderBy(p => p.Category.Name) : query.OrderByDescending(p => p.Category.Name),
                    _ => query => ascending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id)
                };

                var queryOptions = new QueryOptions 
                { 
                    TrackingEnabled = false,
                    SplitQuery = false
                };

                var result = await _unitOfWork.Products.GetPagedAsync(
                    page, 
                    pageSize, 
                    predicate, 
                    "Category",
                    queryOptions,
                    cancellationToken);
                
                if (!result.IsSuccess)
                    return Result<IEnumerable<ProductListItemViewModel>>.Failure(result.Message);

                var products = _mapper.Map<IEnumerable<ProductListItemViewModel>>(result.Value);
                return Result<IEnumerable<ProductListItemViewModel>>.Success(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return Result<IEnumerable<ProductListItemViewModel>>.Failure("Error retrieving products");
            }
        }

        public async Task<Result<ProductDetailsViewModel>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                if (!result.IsSuccess)
                    return Result<ProductDetailsViewModel>.NotFound("Product");

                var viewModel = _mapper.Map<ProductDetailsViewModel>(result.Value);
                return Result<ProductDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product: {ProductId}", id);
                return Result<ProductDetailsViewModel>.Failure("Error retrieving product");
            }
        }

        public async Task<Result<ProductDetailsViewModel>> CreateProductAsync(CreateProductViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<ProductDetailsViewModel>.ValidationError("Product model cannot be null");

            try
            {
                var productViewModel = new ProductViewModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    SKU = model.SKU,
                    UnitOfMeasurement = model.UnitOfMeasurement,
                    Price = model.Price,
                    Cost = model.Cost,
                    ReorderLevel = model.ReorderLevel,
                    CategoryId = model.CategoryId,
                    IsActive = true
                };
                
                var validationResult = await _validator.ValidateAsync(productViewModel, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<ProductDetailsViewModel>.ValidationError("Validation failed", errors);
                }

                var product = _mapper.Map<Product>(model);
                product.IsActive = true;

                var result = await _unitOfWork.Products.AddAsync(product, cancellationToken);
                if (!result.IsSuccess)
                    return Result<ProductDetailsViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var viewModel = _mapper.Map<ProductDetailsViewModel>(result.Value);
                return Result<ProductDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product: {ProductName}", model.Name);
                return Result<ProductDetailsViewModel>.Failure("Error creating product");
            }
        }

        public async Task<Result<ProductDetailsViewModel>> UpdateProductAsync(int id, UpdateProductViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
                return Result<ProductDetailsViewModel>.ValidationError("Product model cannot be null");

            if (id != model.Id)
                return Result<ProductDetailsViewModel>.ValidationError("ID mismatch between URL and model");

            try
            {
                var productViewModel = new ProductViewModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    SKU = model.SKU,
                    UnitOfMeasurement = model.UnitOfMeasurement,
                    Price = model.Price,
                    Cost = model.Cost,
                    ReorderLevel = model.ReorderLevel,
                    CategoryId = model.CategoryId,
                    IsActive = model.IsActive
                };
                
                var validationResult = await _validator.ValidateAsync(productViewModel, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<ProductDetailsViewModel>.ValidationError("Validation failed", errors);
                }

                var existingProductResult = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                if (!existingProductResult.IsSuccess)
                    return Result<ProductDetailsViewModel>.NotFound("Product");

                var existingProduct = existingProductResult.Value;
                
                // Preserve current stock if not explicitly changed or zero
                int originalStock = existingProduct.CurrentStock;
                
                // Use AutoMapper to map properties from model to existing entity
                _mapper.Map(model, existingProduct);
                
                // Ensure we don't lose the stock value if model.CurrentStock is 0
                if (model.CurrentStock == 0 && originalStock > 0)
                {
                    existingProduct.CurrentStock = originalStock;
                }

                var result = await _unitOfWork.Products.UpdateAsync(existingProduct, cancellationToken);
                if (!result.IsSuccess)
                    return Result<ProductDetailsViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                var viewModel = _mapper.Map<ProductDetailsViewModel>(result.Value);
                return Result<ProductDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                return Result<ProductDetailsViewModel>.Failure("Error updating product");
            }
        }

        public async Task<Result<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
                if (!result.IsSuccess)
                    return Result<bool>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return Result<bool>.Failure("Error deleting product");
            }
        }

        public async Task<Result<bool>> ProductExistsBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create specification for checking SKU
                var specification = new ProductBySkuSpecification(sku);
                var result = await _unitOfWork.Products.AnyAsync(specification, cancellationToken);
                
                // Check if the result is successful first
                if (!result.IsSuccess)
                    return Result<bool>.Failure(result.Message);
                
                // Return the actual boolean value
                return Result<bool>.Success(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking product existence by SKU: {SKU}", sku);
                return Result<bool>.Failure("Error checking product existence");
            }
        }

        public async Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsNeedingReorderAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Create specification for products needing reorder
                var specification = new ProductsNeedingReorderSpecification();
                var result = await _unitOfWork.Products.FindAsync(
                    specification, 
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                    return Result<IEnumerable<ProductListItemViewModel>>.Failure(result.Message);

                // Map each product and populate supplier information
                var products = result.Value.Select(p => {
                    var viewModel = _mapper.Map<ProductListItemViewModel>(p);
                    
                    // Find preferred supplier or any supplier if no preferred one exists
                    var preferredSupplier = p.ProductSuppliers
                        ?.FirstOrDefault(ps => ps.IsPreferredSupplier)
                        ?.Supplier;
                        
                    if (preferredSupplier == null && p.ProductSuppliers != null && p.ProductSuppliers.Any())
                        preferredSupplier = p.ProductSuppliers.First().Supplier;
                        
                    viewModel.PrimarySupplierName = preferredSupplier?.Name;
                    
                    return viewModel;
                });

                return Result<IEnumerable<ProductListItemViewModel>>.Success(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products needing reorder");
                return Result<IEnumerable<ProductListItemViewModel>>.Failure("Error retrieving products needing reorder");
            }
        }

        public async Task<Result<ProductDetailsViewModel>> UpdateStockLevelAsync(int id, int quantity, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                // Begin transaction
                var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    var productResult = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                    if (!productResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProductDetailsViewModel>.NotFound("Product");
                    }

                    var product = productResult.Value;
                    var previousStock = product.CurrentStock;
                    product.CurrentStock += quantity;

                    if (product.CurrentStock < 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProductDetailsViewModel>.ValidationError("Stock adjustment would result in negative inventory");
                    }

                    // Create stock history entry
                    var stockHistory = new StockHistory
                    {
                        ProductId = id,
                        Date = DateTime.UtcNow,
                        QuantityChange = quantity,
                        PreviousStock = previousStock,
                        NewStock = product.CurrentStock,
                        Reason = reason,
                        Type = quantity > 0 ? TransactionType.StockIn : TransactionType.StockOut
                    };

                    var stockResult = await _unitOfWork.StockHistories.AddAsync(stockHistory, cancellationToken);
                    if (!stockResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProductDetailsViewModel>.Failure(stockResult.Message);
                    }

                    var updateResult = await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                    if (!updateResult.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProductDetailsViewModel>.Failure(updateResult.Message);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    
                    var viewModel = _mapper.Map<ProductDetailsViewModel>(product);
                    return Result<ProductDetailsViewModel>.Success(viewModel);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock level for product: {ProductId}", id);
                return Result<ProductDetailsViewModel>.Failure("Error updating stock level");
            }
        }

        public async Task<Result<IEnumerable<StockHistoryViewModel>>> GetStockHistoryAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var productResult = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
                if (!productResult.IsSuccess)
                    return Result<IEnumerable<StockHistoryViewModel>>.NotFound("Product");

                // Create specification for stock history
                var specification = new StockHistoryByProductSpecification(productId);
                var result = await _unitOfWork.StockHistories.FindAsync(specification, cancellationToken);

                if (!result.IsSuccess)
                    return Result<IEnumerable<StockHistoryViewModel>>.Failure(result.Message);

                var history = _mapper.Map<IEnumerable<StockHistoryViewModel>>(result.Value);
                return Result<IEnumerable<StockHistoryViewModel>>.Success(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stock history for product: {ProductId}", productId);
                return Result<IEnumerable<StockHistoryViewModel>>.Failure("Error retrieving stock history");
            }
        }

        public async Task<Result<IEnumerable<ProductListItemViewModel>>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create specification for products by category
                var specification = new ProductsByCategorySpecification(categoryId);
                var result = await _unitOfWork.Products.FindAsync(specification, cancellationToken);

                if (!result.IsSuccess)
                    return Result<IEnumerable<ProductListItemViewModel>>.Failure(result.Message);

                var products = _mapper.Map<IEnumerable<ProductListItemViewModel>>(result.Value);
                return Result<IEnumerable<ProductListItemViewModel>>.Success(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category: {CategoryId}", categoryId);
                return Result<IEnumerable<ProductListItemViewModel>>.Failure("Error retrieving products");
            }
        }

        public async Task<Result<IPagedList<ProductListItemViewModel>>> GetPagedProductsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? categoryName = null,
            bool? lowStock = null,
            bool? outOfStock = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                // Build the predicate for filtering
                Expression<Func<Product, bool>>? predicate = null;
                
                // Search term filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    predicate = p => p.Name.ToLower().Contains(searchTerm) ||
                                    p.SKU.ToLower().Contains(searchTerm) ||
                                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                                    (p.Category != null && p.Category.Name.ToLower().Contains(searchTerm));
                }
                
                // Category name filter (now removed as it's included in the search term)
                
                // Low Stock filter
                if (lowStock == true)
                {
                    if (predicate != null)
                    {
                        var existingPredicate = predicate;
                        predicate = p => existingPredicate.Compile()(p) && 
                                      p.CurrentStock <= p.ReorderLevel && 
                                      p.CurrentStock > 0;
                    }
                    else
                    {
                        predicate = p => p.CurrentStock <= p.ReorderLevel && p.CurrentStock > 0;
                    }
                }
                
                // Out of Stock filter
                if (outOfStock == true)
                {
                    if (predicate != null)
                    {
                        var existingPredicate = predicate;
                        predicate = p => existingPredicate.Compile()(p) && p.CurrentStock <= 0;
                    }
                    else
                    {
                        predicate = p => p.CurrentStock <= 0;
                    }
                }

                var queryOptions = new QueryOptions
                {
                    TrackingEnabled = false,
                    SplitQuery = true
                };

                var result = await _unitOfWork.Products.GetPagedAsync(
                    page,
                    pageSize,
                    predicate,
                    includeProperties: "Category,ProductSuppliers.Supplier",
                    cancellationToken: cancellationToken);

                if (!result.IsSuccess)
                    return Result<IPagedList<ProductListItemViewModel>>.Failure(result.Message);

                // Map each product and populate supplier information
                var mappedProducts = result.Value.Select(p => {
                    var viewModel = _mapper.Map<ProductListItemViewModel>(p);
                    
                    // Find preferred supplier or any supplier if no preferred one exists
                    var preferredSupplier = p.ProductSuppliers
                        ?.FirstOrDefault(ps => ps.IsPreferredSupplier)
                        ?.Supplier;
                        
                    if (preferredSupplier == null && p.ProductSuppliers != null && p.ProductSuppliers.Any())
                        preferredSupplier = p.ProductSuppliers.First().Supplier;
                        
                    viewModel.PrimarySupplierName = preferredSupplier?.Name;
                    
                    return viewModel;
                });
                
                // Create paged list from the mapped items
                var pagedList = new StaticPagedList<ProductListItemViewModel>(
                    mappedProducts,
                    result.Value.PageNumber,
                    result.Value.PageSize,
                    result.Value.TotalItemCount
                );
                
                return Result<IPagedList<ProductListItemViewModel>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged products");
                return Result<IPagedList<ProductListItemViewModel>>.Failure("Error retrieving products");
            }
        }
    }
} 