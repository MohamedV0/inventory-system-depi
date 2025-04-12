using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Common;

namespace InventoryManagementSystem.Models.ViewModels.Notification
{
    public class CreateNotificationViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; }

        public int? ProductId { get; set; }
    }
} 