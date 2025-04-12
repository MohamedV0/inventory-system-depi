using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.ViewModels.Notification;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "CanViewNotifications")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                var notifications = await _notificationService.GetUnreadNotificationsAsync(cancellationToken);
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for index view");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to mark notification {Id} as read", id);
                
                var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
                if (!result)
                {
                    _logger.LogWarning("Notification {Id} not found or could not be marked as read", id);
                    return Json(new { success = false, message = "Notification not found or could not be marked as read." });
                }

                _logger.LogInformation("Successfully marked notification {Id} as read", id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {Id} as read", id);
                return Json(new { success = false, message = "An error occurred while marking the notification as read." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Attempting to mark all notifications as read");
                
                var result = await _notificationService.MarkAllAsReadAsync(cancellationToken);
                if (!result)
                {
                    _logger.LogInformation("No unread notifications found to mark as read");
                    return Json(new { success = true, message = "No unread notifications found." });
                }

                _logger.LogInformation("Successfully marked all notifications as read");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false, message = "An error occurred while marking all notifications as read." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _notificationService.GetUnreadCountAsync(cancellationToken);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notification count");
                return Json(new { count = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid notification data" });
            }

            await _notificationService.CreateNotificationAsync(model, cancellationToken);
            return Json(new { success = true });
        }
    }
} 