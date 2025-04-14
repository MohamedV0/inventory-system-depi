using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Identity
{
    /// <summary>
    /// Represents a permission that can be assigned to users
    /// </summary>
    public class Permission : BaseEntity
    {
        /// <summary>
        /// The unique name of the permission (e.g., "CanViewProducts")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// A description of what the permission allows
        /// </summary>
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// The category this permission belongs to (e.g., "Products", "Stock")
        /// </summary>
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Navigation property for user permissions
        /// </summary>
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
} 