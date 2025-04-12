using System.Collections.Generic;

namespace InventoryManagementSystem.Models.ViewModels.Notification
{
    public class NotificationBellViewModel
    {
        public int UnreadCount { get; set; }
        public IEnumerable<NotificationViewModel> UnreadNotifications { get; set; }

        public NotificationBellViewModel()
        {
            UnreadNotifications = new List<NotificationViewModel>();
        }
    }
} 