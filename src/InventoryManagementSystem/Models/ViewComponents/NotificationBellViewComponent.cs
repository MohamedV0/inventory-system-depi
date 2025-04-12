using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Models.ViewModels;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Models.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;

        public NotificationBellViewComponent(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var notificationData = await _notificationService.GetNotificationBellDataAsync();
            return View(notificationData);
        }
    }
} 