using System;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Models.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
    }
} 