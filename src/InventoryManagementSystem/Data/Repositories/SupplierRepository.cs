using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Data.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        private readonly ILogger<SupplierRepository> _supplierLogger;

        public SupplierRepository(
            ApplicationDbContext context, 
            ILogger<SupplierRepository> logger,
            IUserContextService userContext)
            : base(context, logger, userContext)
        {
            _supplierLogger = logger;
        }

        public async Task<Result<bool>> SupplierEmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _dbSet.AnyAsync(s => s.Email.ToLower() == email.ToLower(), cancellationToken);
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _supplierLogger.LogError(ex, "Error checking supplier email existence: {Email}", email);
                return Result<bool>.Failure("Error checking supplier email existence");
            }
        }

        public async Task<Result<IEnumerable<Supplier>>> GetActiveSuppliers(CancellationToken cancellationToken = default)
        {
            try
            {
                var suppliers = await _dbSet
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);

                return Result<IEnumerable<Supplier>>.Success(suppliers);
            }
            catch (Exception ex)
            {
                _supplierLogger.LogError(ex, "Error retrieving active suppliers");
                return Result<IEnumerable<Supplier>>.Failure("Error retrieving active suppliers");
            }
        }

        public async Task<Result<IPagedList<Supplier>>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<Supplier, bool>>? predicate = null,
            Func<IQueryable<Supplier>, IOrderedQueryable<Supplier>>? orderBy = null,
            string? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.AsQueryable();

                if (predicate != null)
                    query = query.Where(predicate);

                if (!string.IsNullOrWhiteSpace(includeProperties))
                {
                    foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProperty.Trim());
                    }
                }

                var totalItems = await query.CountAsync(cancellationToken);

                if (orderBy != null)
                    query = orderBy(query);
                else
                    query = query.OrderBy(e => e.Id);

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // Create a StaticPagedList instance
                var pagedList = new StaticPagedList<Supplier>(
                    items,
                    page,
                    pageSize,
                    totalItems
                );

                return Result<IPagedList<Supplier>>.Success(pagedList);
            }
            catch (Exception ex)
            {
                _supplierLogger.LogError(ex, "Error retrieving paged suppliers");
                return Result<IPagedList<Supplier>>.Failure("Error retrieving paged suppliers");
            }
        }
    }
} 