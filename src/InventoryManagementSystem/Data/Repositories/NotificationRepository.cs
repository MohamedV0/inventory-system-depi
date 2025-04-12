using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Data.Repositories
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Notification> _dbSet;
        private readonly ILogger<NotificationRepository> _logger;
        private readonly IUserContextService _userContext;

        public NotificationRepository(
            ApplicationDbContext context,
            ILogger<NotificationRepository> logger,
            IUserContextService userContext)
            : base(context, logger, userContext)
        {
            _context = context;
            _dbSet = context.Set<Notification>();
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(n => !n.IsRead && !n.IsDeleted)
                    .Include(n => n.Product)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications");
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .CountAsync(n => !n.IsRead && !n.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting unread notifications");
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                if (notification == null)
                {
                    _logger.LogWarning("Attempted to mark non-existent notification as read. ID: {Id}", id);
                    return false;
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _dbSet.Update(notification);
                await _context.SaveChangesAsync(cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read. ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var unreadNotifications = await _dbSet
                    .Where(n => !n.IsRead && !n.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!unreadNotifications.Any())
                    return true;

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                _dbSet.UpdateRange(unreadNotifications);
                await _context.SaveChangesAsync(cancellationToken);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }
    }
} 