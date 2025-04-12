using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.ViewModels.Notification;
using System.Threading;

namespace InventoryManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;
        private readonly IProductService _productService;

        public NotificationService(
            ApplicationDbContext context,
            ILogger<NotificationService> logger,
            IMapper mapper,
            IUserContextService userContextService,
            IProductService productService)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _userContextService = userContextService;
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public async Task<NotificationBellViewModel> GetNotificationBellDataAsync(CancellationToken cancellationToken = default)
        {
            var unreadNotifications = await GetUnreadNotificationsAsync(cancellationToken);
            return new NotificationBellViewModel
            {
                UnreadNotifications = unreadNotifications,
                UnreadCount = unreadNotifications.Count
            };
        }

        public async Task<List<NotificationViewModel>> GetAllNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Include(n => n.Product)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);

                return _mapper.Map<List<NotificationViewModel>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all notifications");
                throw;
            }
        }

        public async Task<List<NotificationViewModel>> GetUnreadNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Include(n => n.Product)
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);

                return _mapper.Map<List<NotificationViewModel>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications");
                throw;
            }
        }

        public async Task<NotificationViewModel> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _context.Notifications
                    .Include(n => n.Product)
                    .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

                return _mapper.Map<NotificationViewModel>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification with ID: {Id}", id);
                throw;
            }
        }

        public async Task CreateNotificationAsync(CreateNotificationViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUserId = _userContextService.GetCurrentUserId();
                var notification = new Notification
                {
                    Title = model.Title,
                    Message = model.Message,
                    Type = model.Type,
                    Priority = model.Priority,
                    ProductId = model.ProductId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = currentUserId
                };

                await _context.Notifications.AddAsync(notification, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task CreateLowStockNotificationAsync(int productId, int currentStock)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return;

                var currentUserId = _userContextService.GetCurrentUserId();
                var notification = new Notification
                {
                    Title = "Low Stock Alert",
                    Message = $"Product {product.Name} is running low on stock. Current stock level: {currentStock}",
                    Type = NotificationType.LowStock,
                    Priority = NotificationPriority.High,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = currentUserId,
                    IsRead = false
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating low stock notification for product ID: {ProductId}", productId);
                throw;
            }
        }

        public async Task CreateOutOfStockNotificationAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return;

                var currentUserId = _userContextService.GetCurrentUserId();
                var notification = new Notification
                {
                    Title = "Out of Stock Alert",
                    Message = $"Product {product.Name} is out of stock!",
                    Type = NotificationType.OutOfStock,
                    Priority = NotificationPriority.High,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = currentUserId,
                    IsRead = false
                };

                await _context.Notifications.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating out of stock notification for product ID: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted, cancellationToken);
                    
                if (notification == null)
                {
                    _logger.LogWarning("Notification {Id} not found or is deleted", id);
                    return false;
                }

                if (notification.IsRead)
                {
                    _logger.LogInformation("Notification {Id} is already marked as read", id);
                    return true;
                }

                var currentUserId = _userContextService.GetCurrentUserId() ?? "anonymous";
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.ModifiedAt = DateTime.UtcNow;
                notification.ModifiedBy = currentUserId;

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully marked notification {Id} as read by user {UserId}", id, currentUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {Id} as read", id);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => !n.IsDeleted && !n.IsRead)
                    .ToListAsync(cancellationToken);

                if (!unreadNotifications.Any())
                {
                    _logger.LogInformation("No unread notifications found to mark as read");
                    return false;
                }

                var currentUserId = _userContextService.GetCurrentUserId() ?? "anonymous";
                var currentTime = DateTime.UtcNow;

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = currentTime;
                    notification.ModifiedAt = currentTime;
                    notification.ModifiedBy = currentUserId;
                }

                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully marked {Count} notifications as read by user {UserId}", 
                    unreadNotifications.Count, currentUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(new object[] { id }, cancellationToken);
                if (notification == null)
                {
                    return false;
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification. ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAllNotificationsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var notifications = await _context.Notifications.ToListAsync(cancellationToken);
                if (!notifications.Any())
                {
                    return false;
                }

                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications");
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Notifications.CountAsync(n => !n.IsRead, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                throw;
            }
        }

        /// <summary>
        /// Centralized method to check stock levels and create appropriate notifications.
        /// This method handles both low stock and out of stock scenarios.
        /// </summary>
        /// <param name="productId">The ID of the product to check</param>
        /// <param name="currentStock">The current stock level</param>
        /// <param name="reorderLevel">The reorder level threshold</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A tuple indicating if notifications were created (lowStock, outOfStock)</returns>
        public async Task<(bool lowStockNotified, bool outOfStockNotified)> CheckAndCreateStockNotificationsAsync(
            int productId,
            int currentStock,
            int? reorderLevel = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // If reorderLevel is not provided, fetch it from the product
                if (!reorderLevel.HasValue)
                {
                    var product = await _context.Products
                        .Where(p => p.Id == productId)
                        .Select(p => new { p.ReorderLevel })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (product == null)
                    {
                        _logger.LogWarning("Product not found for stock notification check: {ProductId}", productId);
                        return (false, false);
                    }

                    reorderLevel = product.ReorderLevel;
                }

                bool lowStockNotified = false;
                bool outOfStockNotified = false;

                // Check for existing unread notifications to avoid duplicates
                var existingNotifications = await _context.Notifications
                    .Where(n => n.ProductId == productId && !n.IsRead &&
                           (n.Type == NotificationType.LowStock || n.Type == NotificationType.OutOfStock))
                    .ToListAsync(cancellationToken);

                // Handle out of stock scenario
                if (currentStock == 0 && !existingNotifications.Any(n => n.Type == NotificationType.OutOfStock))
                {
                    await CreateOutOfStockNotificationAsync(productId);
                    outOfStockNotified = true;
                }
                // Handle low stock scenario
                else if (currentStock <= reorderLevel && currentStock > 0 && 
                         !existingNotifications.Any(n => n.Type == NotificationType.LowStock))
                {
                    await CreateLowStockNotificationAsync(productId, currentStock);
                    lowStockNotified = true;
                }

                return (lowStockNotified, outOfStockNotified);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and creating stock notifications for product ID: {ProductId}", productId);
                throw;
            }
        }

        private static string CalculateTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)}mo ago";

            return $"{(int)(timeSpan.TotalDays / 365)}y ago";
        }
    }
} 