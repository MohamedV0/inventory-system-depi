using System;
using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Entities
{
    public class Notification : BaseEntity, IAuditable
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationPriority Priority { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }

        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
} 