using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels.Notification;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationBellViewModel> GetNotificationBellDataAsync(CancellationToken cancellationToken = default);
        Task<List<NotificationViewModel>> GetAllNotificationsAsync(CancellationToken cancellationToken = default);
        Task<List<NotificationViewModel>> GetUnreadNotificationsAsync(CancellationToken cancellationToken = default);
        Task<NotificationViewModel> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);
        Task CreateNotificationAsync(CreateNotificationViewModel model, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default);
        Task<bool> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAllNotificationsAsync(CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks stock levels and creates appropriate notifications (low stock or out of stock).
        /// </summary>
        /// <param name="productId">The ID of the product to check</param>
        /// <param name="currentStock">The current stock level</param>
        /// <param name="reorderLevel">Optional reorder level threshold. If not provided, will be fetched from the product.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A tuple indicating if notifications were created (lowStock, outOfStock)</returns>
        Task<(bool lowStockNotified, bool outOfStockNotified)> CheckAndCreateStockNotificationsAsync(
            int productId,
            int currentStock,
            int? reorderLevel = null,
            CancellationToken cancellationToken = default);
    }
} 