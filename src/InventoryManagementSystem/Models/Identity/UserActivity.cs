using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementSystem.Models.Identity
{
    /// <summary>
    /// Represents a user activity record in the system
    /// </summary>
    public class UserActivity : BaseEntity
    {
        /// <summary>
        /// The ID of the user who performed the action
        /// </summary>
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The username of the user who performed the action
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The type of activity performed
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// The action performed (for backward compatibility and additional context)
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The type of entity affected (e.g., "Product", "Supplier")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the affected entity (if applicable)
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Additional details about the activity
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Navigation property to the user who performed the action
        /// </summary>
        public virtual ApplicationUser User { get; set; } = null!;
    }
} 