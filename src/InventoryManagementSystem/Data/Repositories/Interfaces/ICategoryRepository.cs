using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for category-specific operations
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Checks if a category name already exists
        /// </summary>
        Task<Result<bool>> CategoryNameExistsAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a category has any active products
        /// </summary>
        Task<Result<bool>> HasActiveProductsAsync(int categoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets categories with their product counts
        /// </summary>
        Task<Result<Dictionary<string, int>>> GetCategoryProductCountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets categories with active products
        /// </summary>
        Task<Result<IEnumerable<Category>>> GetCategoriesWithProductsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets categories with no products
        /// </summary>
        Task<Result<IEnumerable<Category>>> GetEmptyCategoriesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets categories created after specified date
        /// </summary>
        Task<Result<IEnumerable<Category>>> GetCategoriesCreatedAfterAsync(DateTime date, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets categories updated after specified date
        /// </summary>
        Task<Result<IEnumerable<Category>>> GetCategoriesUpdatedAfterAsync(DateTime date, CancellationToken cancellationToken = default);
    }
} 