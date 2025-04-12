using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for category management operations
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets a paginated list of categories
        /// </summary>
        Task<Result<IPagedList<CategoryViewModel>>> GetCategoriesAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a category by its ID
        /// </summary>
        Task<Result<CategoryViewModel>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new category
        /// </summary>
        Task<Result<CategoryViewModel>> CreateCategoryAsync(CategoryViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing category
        /// </summary>
        Task<Result<CategoryViewModel>> UpdateCategoryAsync(int id, CategoryViewModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a category
        /// </summary>
        Task<Result<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    }
} 