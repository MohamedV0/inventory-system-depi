using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Gets the dashboard data
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard data</returns>
        Task<Result<DashboardViewModel>> GetDashboardDataAsync(CancellationToken cancellationToken = default);
    }
} 