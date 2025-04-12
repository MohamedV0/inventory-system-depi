using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<Result<bool>> SupplierEmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<Supplier>>> GetActiveSuppliers(CancellationToken cancellationToken = default);
        Task<Result<IPagedList<Supplier>>> GetPagedAsync(
            int page,
            int pageSize,
            Expression<Func<Supplier, bool>>? predicate = null,
            Func<IQueryable<Supplier>, IOrderedQueryable<Supplier>>? orderBy = null,
            string? includeProperties = null,
            CancellationToken cancellationToken = default);
    }
} 