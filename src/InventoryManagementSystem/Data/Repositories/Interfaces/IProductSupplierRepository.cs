using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface IProductSupplierRepository : IRepository<ProductSupplier>
    {
        Task<Result<bool>> ExistsAsync(int productId, int supplierId);
        Task<Result<ProductSupplier>> GetByCompositeIdAsync(int productId, int supplierId, string? includeProperties = null);
        Task<Result<bool>> DeleteAsync(int productId, int supplierId);
        Task<Result<ProductSupplier>> GetPreferredSupplierForProductAsync(int productId);
        Task<Result<bool>> UnsetPreferredSupplierAsync(int productId, CancellationToken cancellationToken = default);
        Task<Result<bool>> SetPreferredSupplierAsync(int productId, int supplierId, CancellationToken cancellationToken = default);
        Task<Result<bool>> UpdateLastPurchaseDateAsync(int productId, int supplierId, DateTime purchaseDate, CancellationToken cancellationToken = default);
        Task<Result<bool>> RelationshipExistsAsync(int productId, int supplierId);
    }
} 