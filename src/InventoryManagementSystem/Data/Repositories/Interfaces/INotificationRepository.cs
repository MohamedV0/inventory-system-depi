using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.Entities;

namespace InventoryManagementSystem.Data.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    }
} 