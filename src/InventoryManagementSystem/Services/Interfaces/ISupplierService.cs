using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.DTOs;
using InventoryManagementSystem.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Interface for supplier management operations
    /// </summary>
    public interface ISupplierService
    {
        /// <summary>
        /// Gets all suppliers
        /// </summary>
        Task<Result<IEnumerable<SupplierListItemViewModel>>> GetSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true);

        /// <summary>
        /// Gets a paginated list of suppliers
        /// </summary>
        Task<Result<IPagedList<SupplierListItemViewModel>>> GetPagedSuppliersAsync(
            int page = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? sortBy = null,
            bool ascending = true);

        /// <summary>
        /// Gets a supplier by its ID
        /// </summary>
        Task<Result<SupplierDetailsViewModel>> GetSupplierByIdAsync(int id);

        /// <summary>
        /// Creates a new supplier
        /// </summary>
        Task<Result<SupplierDetailsViewModel>> CreateSupplierAsync(CreateSupplierViewModel model);

        /// <summary>
        /// Updates an existing supplier
        /// </summary>
        Task<Result<SupplierDetailsViewModel>> UpdateSupplierAsync(int id, UpdateSupplierViewModel model);

        /// <summary>
        /// Deletes a supplier
        /// </summary>
        Task<Result<bool>> DeleteSupplierAsync(int id);

        /// <summary>
        /// Checks if a supplier exists by email
        /// </summary>
        Task<Result<bool>> SupplierExistsByEmailAsync(string email);

        /// <summary>
        /// Gets suppliers for a specific product
        /// </summary>
        Task<Result<IEnumerable<SupplierListItemViewModel>>> GetSuppliersForProductAsync(int productId);

        /// <summary>
        /// Gets preferred suppliers
        /// </summary>
        Task<Result<IEnumerable<SupplierListItemViewModel>>> GetPreferredSuppliersAsync();
    }
} 