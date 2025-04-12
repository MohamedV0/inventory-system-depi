using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Result<bool>> ProductCodeExistsAsync(string code);
        Task<Result<bool>> ProductNameExistsAsync(string name);
        Task<Result<bool>> ProductExistsBySkuAsync(string sku);
        Task<Result<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId);
        Task<Result<IEnumerable<Product>>> GetProductsNeedingReorderAsync();
        Task<Result<IEnumerable<Product>>> GetLowStockProductsAsync(int threshold);
        Task<Result<Dictionary<string, decimal>>> GetProductStockLevelsAsync();
        Task<Result<IEnumerable<Product>>> GetProductsCreatedAfterAsync(DateTime date);
        Task<Result<IEnumerable<Product>>> GetProductsUpdatedAfterAsync(DateTime date);
        Task<Result<bool>> UpdateStockLevelAsync(int productId, int quantity);
        Task<Result<decimal>> GetTotalInventoryValueAsync();
    }
} 