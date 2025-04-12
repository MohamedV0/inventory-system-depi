using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Models.Validation;
using X.PagedList;
using InventoryManagementSystem.Helpers;
using System.Threading;
using AutoMapper;
using InventoryManagementSystem.Data;

namespace InventoryManagementSystem.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SupplierService> _logger;
        private readonly SupplierValidator _validator;
        private readonly IMapper _mapper;
        private const int MaxPageSize = 100;

        public SupplierService(
            IUnitOfWork unitOfWork,
            ILogger<SupplierService> logger,
            SupplierValidator validator,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<IEnumerable<SupplierListItemViewModel>>> GetSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                Expression<Func<Supplier, bool>>? predicate = null;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    predicate = s => s.Name.ToLower().Contains(searchTerm) || 
                                   s.Email.ToLower().Contains(searchTerm) ||
                                   s.Phone.Contains(searchTerm) ||
                                   (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(searchTerm));
                }

                Func<IQueryable<Supplier>, IOrderedQueryable<Supplier>>? orderBy = null;
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    orderBy = sortBy.ToLower() switch
                    {
                        "name" => query => ascending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                        "email" => query => ascending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
                        "contactperson" => query => ascending ? query.OrderBy(s => s.ContactPerson) : query.OrderByDescending(s => s.ContactPerson),
                        "createdat" => query => ascending ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                        "updatedat" => query => ascending ? query.OrderBy(s => s.UpdatedAt) : query.OrderByDescending(s => s.UpdatedAt),
                        _ => query => ascending ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id)
                    };
                }

                // Include ProductSuppliers to get the correct product count
                var result = await _unitOfWork.Suppliers.GetPagedAsync(page, pageSize, predicate, orderBy, includeProperties: "ProductSuppliers");
                if (!result.IsSuccess)
                    return Result<IEnumerable<SupplierListItemViewModel>>.Failure(result.Message);

                var suppliers = result.Value.MapPagedList(s => _mapper.Map<SupplierListItemViewModel>(s)).ToList();
                return Result<IEnumerable<SupplierListItemViewModel>>.Success(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                return Result<IEnumerable<SupplierListItemViewModel>>.Failure("Error retrieving suppliers");
            }
        }

        public async Task<Result<SupplierDetailsViewModel>> GetSupplierByIdAsync(int id)
        {
            try
            {
                // Use the existing repository patterns with include properties pattern
                var result = await _unitOfWork.Suppliers.GetByIdAsync(
                    id, 
                    cancellationToken: default, 
                    trackChanges: false);
                    
                if (!result.IsSuccess)
                    return Result<SupplierDetailsViewModel>.NotFound("Supplier");

                // Get product-supplier relationships separately with Product included
                // Use the correct parameter pattern for FindAsync method
                var productSuppliersResult = await _unitOfWork.ProductSuppliers.FindAsync(
                    ps => ps.SupplierId == id,
                    cancellationToken: default,
                    options: new QueryOptions 
                    { 
                        TrackingEnabled = false,
                        SplitQuery = false
                    });
                    
                if (productSuppliersResult.IsSuccess)
                {
                    // Now fetch the products for these relationships
                    var productSuppliers = productSuppliersResult.Value.ToList();
                    
                    // Load the products for each product-supplier relationship
                    foreach (var ps in productSuppliers)
                    {
                        var productResult = await _unitOfWork.Products.GetByIdAsync(ps.ProductId);
                        if (productResult.IsSuccess)
                        {
                            ps.Product = productResult.Value;
                        }
                    }
                    
                    // Ensure the supplier has the product-supplier relationships loaded
                    result.Value.ProductSuppliers = productSuppliers;
                }
                
                var viewModel = _mapper.Map<SupplierDetailsViewModel>(result.Value);
                return Result<SupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID {SupplierId}", id);
                return Result<SupplierDetailsViewModel>.Failure("Error retrieving supplier");
            }
        }

        public async Task<Result<SupplierDetailsViewModel>> CreateSupplierAsync(CreateSupplierViewModel model)
        {
            if (model == null)
                return Result<SupplierDetailsViewModel>.ValidationError("Supplier model cannot be null");

            try
            {
                // Convert to SupplierViewModel for validation
                var supplierViewModel = new SupplierViewModel
                {
                    Name = model.Name,
                    ContactPerson = model.ContactPerson,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    IsActive = true
                };
                
                var validationResult = await _validator.ValidateAsync(supplierViewModel);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<SupplierDetailsViewModel>.ValidationError("Validation failed", errors);
                }

                var supplier = new Supplier
                {
                    Name = model.Name.Trim(),
                    ContactPerson = model.ContactPerson.Trim(),
                    Email = model.Email.Trim(),
                    Phone = model.Phone.Trim(),
                    Address = model.Address?.Trim(),
                    IsActive = true
                };

                var result = await _unitOfWork.Suppliers.AddAsync(supplier);
                if (!result.IsSuccess)
                    return Result<SupplierDetailsViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync();
                
                var viewModel = _mapper.Map<SupplierDetailsViewModel>(result.Value);
                return Result<SupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier: {SupplierName}", model.Name);
                return Result<SupplierDetailsViewModel>.Failure("Error creating supplier");
            }
        }

        public async Task<Result<SupplierDetailsViewModel>> UpdateSupplierAsync(int id, UpdateSupplierViewModel model)
        {
            if (model == null)
                return Result<SupplierDetailsViewModel>.ValidationError("Supplier model cannot be null");

            if (id != model.Id)
                return Result<SupplierDetailsViewModel>.ValidationError("ID mismatch between URL and model");

            try
            {
                // Convert to SupplierViewModel for validation
                var supplierViewModel = new SupplierViewModel
                {
                    Id = model.Id,
                    Name = model.Name,
                    ContactPerson = model.ContactPerson,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    IsActive = model.IsActive
                };
                
                var validationResult = await _validator.ValidateAsync(supplierViewModel);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Result<SupplierDetailsViewModel>.ValidationError("Validation failed", errors);
                }

                var existingSupplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (!existingSupplier.IsSuccess)
                    return Result<SupplierDetailsViewModel>.NotFound("Supplier");

                existingSupplier.Value.Name = model.Name.Trim();
                existingSupplier.Value.ContactPerson = model.ContactPerson?.Trim() ?? string.Empty;
                existingSupplier.Value.Email = model.Email.Trim();
                existingSupplier.Value.Phone = model.Phone?.Trim() ?? string.Empty;
                existingSupplier.Value.Address = model.Address?.Trim();
                existingSupplier.Value.IsActive = model.IsActive;

                var result = await _unitOfWork.Suppliers.UpdateAsync(existingSupplier.Value);
                if (!result.IsSuccess)
                    return Result<SupplierDetailsViewModel>.Failure(result.Message);

                await _unitOfWork.SaveChangesAsync();
                
                var viewModel = _mapper.Map<SupplierDetailsViewModel>(result.Value);
                return Result<SupplierDetailsViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier: {SupplierId}", id);
                return Result<SupplierDetailsViewModel>.Failure("Error updating supplier");
            }
        }

        public async Task<Result<bool>> DeleteSupplierAsync(int id)
        {
            try
            {
                var result = await _unitOfWork.Suppliers.DeleteAsync(id);
                if (!result.IsSuccess)
                    return Result<bool>.NotFound("Supplier");

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier: {SupplierId}", id);
                return Result<bool>.Failure("Error deleting supplier");
            }
        }

        public async Task<Result<bool>> SupplierExistsByEmailAsync(string email)
        {
            try
            {
                var result = await _unitOfWork.Suppliers.SupplierEmailExistsAsync(email);
                if (result.IsSuccess)
                {
                    return Result<bool>.Success(result.Value);
                }
                return Result<bool>.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supplier email existence: {Email}", email);
                return Result<bool>.Failure("Error checking supplier email existence");
            }
        }

        public async Task<Result<IEnumerable<SupplierListItemViewModel>>> GetSuppliersForProductAsync(int productId)
        {
            try
            {
                var predicate = (Expression<Func<Supplier, bool>>)(s => s.ProductSuppliers.Any(ps => ps.ProductId == productId));
                var result = await _unitOfWork.Suppliers.GetPagedAsync(1, int.MaxValue, predicate, includeProperties: null);

                if (!result.IsSuccess)
                    return Result<IEnumerable<SupplierListItemViewModel>>.Failure(result.Message);

                var suppliers = result.Value.MapPagedList(s => _mapper.Map<SupplierListItemViewModel>(s)).ToList();
                return Result<IEnumerable<SupplierListItemViewModel>>.Success(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers for product: {ProductId}", productId);
                return Result<IEnumerable<SupplierListItemViewModel>>.Failure("Error retrieving suppliers for product");
            }
        }

        public async Task<Result<IEnumerable<SupplierListItemViewModel>>> GetPreferredSuppliersAsync()
        {
            try
            {
                var predicate = (Expression<Func<Supplier, bool>>)(s => s.ProductSuppliers.Any(ps => ps.IsPreferred));
                var result = await _unitOfWork.Suppliers.GetPagedAsync(1, int.MaxValue, predicate);

                if (!result.IsSuccess)
                    return Result<IEnumerable<SupplierListItemViewModel>>.Failure(result.Message);

                var suppliers = result.Value.MapPagedList(s => _mapper.Map<SupplierListItemViewModel>(s)).ToList();
                return Result<IEnumerable<SupplierListItemViewModel>>.Success(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving preferred suppliers");
                return Result<IEnumerable<SupplierListItemViewModel>>.Failure("Error retrieving preferred suppliers");
            }
        }

        public async Task<Result<IPagedList<SupplierListItemViewModel>>> GetPagedSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize));

                Expression<Func<Supplier, bool>>? predicate = null;
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    predicate = s => s.Name.ToLower().Contains(searchTerm) ||
                                   s.Email.ToLower().Contains(searchTerm) ||
                                   s.Phone.Contains(searchTerm);
                }

                Func<IQueryable<Supplier>, IOrderedQueryable<Supplier>>? orderBy = sortBy?.ToLower() switch
                {
                    "name" => query => ascending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                    "email" => query => ascending ? query.OrderBy(s => s.Email) : query.OrderByDescending(s => s.Email),
                    "phone" => query => ascending ? query.OrderBy(s => s.Phone) : query.OrderByDescending(s => s.Phone),
                    _ => query => ascending ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id)
                };

                // Include ProductSuppliers to get the correct product count
                var result = await _unitOfWork.Suppliers.GetPagedAsync(page, pageSize, predicate, orderBy, includeProperties: "ProductSuppliers");
                if (!result.IsSuccess)
                    return Result<IPagedList<SupplierListItemViewModel>>.Failure(result.Message);

                // Map suppliers to view models
                var pagedList = result.Value.MapPagedList(s => _mapper.Map<SupplierListItemViewModel>(s));
                
                return Result<IPagedList<SupplierListItemViewModel>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged suppliers");
                return Result<IPagedList<SupplierListItemViewModel>>.Failure("Error retrieving suppliers");
            }
        }
    }
} 