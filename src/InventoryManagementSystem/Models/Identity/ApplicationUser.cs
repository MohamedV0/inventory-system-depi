using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace InventoryManagementSystem.Models.Identity
{
    /// <summary>
    /// Custom application user class that extends IdentityUser with additional properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The user's full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The date and time of the user's last login
        /// </summary>
        public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The date and time when the user last modified any inventory item
        /// </summary>
        public DateTime? LastModifiedInventoryDate { get; set; }

        /// <summary>
        /// Navigation property for user activities
        /// </summary>
        public virtual ICollection<UserActivity> Activities { get; set; } = new List<UserActivity>();
    }
} 