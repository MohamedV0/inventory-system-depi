using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Models.Validation;
using InventoryManagementSystem.Helpers;
using X.PagedList;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services
{
    public class ProductSupplierService : IProductSupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductSupplierService> _logger;
        private readonly ProductSupplierValidator _validator;
        private readonly IMapper _mapper;
        private const int MaxPageSize = 100;

        public ProductSupplierService(
            IUnitOfWork unitOfWork,
            ILogger<ProductSupplierService> logger,
            ProductSupplierValidator validator,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<IEnumerable<ProductSupplierListItemViewModel>>> GetProductSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            bool ascending = true,
            int? productId = null,
            int? supplierId = null)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                Expression<Func<ProductSupplier, bool>>? predicate = null;
                var predicates = new List<Expression<Func<ProductSupplier, bool>>>();

                if (productId.HasValue)
                {
                    predicates.Add(ps => ps.ProductId == productId.Value);
                }

                if (supplierId.HasValue)
                {
                    predicates.Add(ps => ps.SupplierId == supplierId.Value);
                }

                if (predicates.Any())
                {
                    predicate = predicates.Aggregate((current, next) =>
                        Expression.Lambda<Func<ProductSupplier, bool>>(
                            Expression.AndAlso(current.Body, next.Body),
                            current.Parameters));
                }

                var result = await _unitOfWork.ProductSuppliers.GetPagedAsync(
                    page, 
                    pageSize, 
                    predicate, 
                    "Product,Supplier");

                if (!result.IsSuccess)
                    return Result<IEnumerable<ProductSupplierListItemViewModel>>.Failure(result.Message);

                var viewModels = new List<ProductSupplierListItemViewModel>();
                foreach (var item in result.Value)
                {
                    viewModels.Add(_mapper.Map<ProductSupplierListItemViewModel>(item));
                }
                
                return Result<IEnumerable<ProductSupplierListItemViewModel>>.Success(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product suppliers list");
                return Result<IEnumerable<ProductSupplierListItemViewModel>>.Failure("Error retrieving product suppliers");
            }
        }

        public async Task<Result<ProductSupplierDetailsViewModel>> GetProductSupplierAsync(
            int productId,
            int supplierId)
        {
            try
            {
                var result = await _unitOfWork.ProductSuppliers.GetByCompositeIdAsync(productId, supplierId, "Product,Supplier");

                if (!result.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure(result.Message);

                var viewModel = _mapper.Map<ProductSupplierDetailsViewModel>(result.Value);
                return Result<ProductSupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product supplier details. ProductId: {ProductId}, SupplierId: {SupplierId}",
                    productId, supplierId);
                return Result<ProductSupplierDetailsViewModel>.Failure("Error retrieving product supplier details");
            }
        }

        public async Task<Result<ProductSupplierDetailsViewModel>> CreateProductSupplierAsync(
            CreateProductSupplierViewModel model)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    return Result<ProductSupplierDetailsViewModel>.Failure(
                        "Validation failed for product supplier",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                // Verify product exists
                var productResult = await _unitOfWork.Products.GetByIdAsync(model.ProductId);
                if (!productResult.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure($"Product with ID {model.ProductId} not found");

                // Verify supplier exists
                var supplierResult = await _unitOfWork.Suppliers.GetByIdAsync(model.SupplierId);
                if (!supplierResult.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure($"Supplier with ID {model.SupplierId} not found");

                // Check if relation already exists
                var existingResult = await _unitOfWork.ProductSuppliers.ExistsAsync(model.ProductId, model.SupplierId);
                if (existingResult.IsSuccess && existingResult.Value)
                    return Result<ProductSupplierDetailsViewModel>.Failure("This product supplier relationship already exists");

                var productSupplier = _mapper.Map<ProductSupplier>(model);

                var result = await _unitOfWork.ProductSuppliers.AddAsync(productSupplier);
                if (!result.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync();

                var viewModel = _mapper.Map<ProductSupplierDetailsViewModel>(result.Value);
                return Result<ProductSupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product supplier. ProductId: {ProductId}, SupplierId: {SupplierId}",
                    model.ProductId, model.SupplierId);
                return Result<ProductSupplierDetailsViewModel>.Failure("Error creating product supplier");
            }
        }

        public async Task<Result<ProductSupplierDetailsViewModel>> UpdateProductSupplierAsync(
            int productId,
            int supplierId,
            UpdateProductSupplierViewModel model)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                {
                    return Result<ProductSupplierDetailsViewModel>.Failure(
                        "Validation failed for product supplier",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var result = await _unitOfWork.ProductSuppliers.GetByCompositeIdAsync(productId, supplierId);
                if (!result.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure(result.Message);

                var productSupplier = result.Value;
                _mapper.Map(model, productSupplier);

                var updateResult = await _unitOfWork.ProductSuppliers.UpdateAsync(productSupplier);
                if (!updateResult.IsSuccess)
                    return Result<ProductSupplierDetailsViewModel>.Failure(updateResult.Message);

                await _unitOfWork.SaveChangesAsync();

                var viewModel = _mapper.Map<ProductSupplierDetailsViewModel>(updateResult.Value);
                return Result<ProductSupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product supplier. ProductId: {ProductId}, SupplierId: {SupplierId}",
                    productId, supplierId);
                return Result<ProductSupplierDetailsViewModel>.Failure("Error updating product supplier");
            }
        }

        public async Task<Result<bool>> DeleteProductSupplierAsync(
            int productId,
            int supplierId)
        {
            try
            {
                var result = await _unitOfWork.ProductSuppliers.DeleteAsync(productId, supplierId);
                if (!result.IsSuccess)
                    return Result<bool>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product supplier. ProductId: {ProductId}, SupplierId: {SupplierId}",
                    productId, supplierId);
                return Result<bool>.Failure("Error deleting product supplier");
            }
        }

        public async Task<Result<SupplierDetailsViewModel>> GetPreferredSupplierForProductAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _unitOfWork.ProductSuppliers.GetPreferredSupplierForProductAsync(productId);
                if (!result.IsSuccess)
                    return Result<SupplierDetailsViewModel>.Failure(result.Message);

                if (result.Value?.Supplier == null)
                    return Result<SupplierDetailsViewModel>.Failure("Supplier data not found for preferred supplier");

                var viewModel = _mapper.Map<SupplierDetailsViewModel>(result.Value.Supplier);
                return Result<SupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred supplier for product. ProductId: {ProductId}", productId);
                return Result<SupplierDetailsViewModel>.Failure("Error retrieving preferred supplier");
            }
        }

        public async Task<Result<bool>> SetPreferredSupplierAsync(
            int productId,
            int supplierId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _unitOfWork.ProductSuppliers.SetPreferredSupplierAsync(productId, supplierId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting preferred supplier. ProductId: {ProductId}, SupplierId: {SupplierId}",
                    productId, supplierId);
                return Result<bool>.Failure("Error setting preferred supplier");
            }
        }

        public async Task<Result<bool>> UpdateLastPurchaseDateAsync(
            int productId,
            int supplierId,
            DateTime purchaseDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _unitOfWork.ProductSuppliers.UpdateLastPurchaseDateAsync(
                    productId, 
                    supplierId, 
                    purchaseDate, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last purchase date. ProductId: {ProductId}, SupplierId: {SupplierId}, Date: {Date}",
                    productId, supplierId, purchaseDate);
                return Result<bool>.Failure("Error updating last purchase date");
            }
        }
    }
} 