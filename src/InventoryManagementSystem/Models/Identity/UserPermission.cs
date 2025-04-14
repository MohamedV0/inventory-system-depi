using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Identity
{
    /// <summary>
    /// Represents the assignment of a permission to a user
    /// </summary>
    public class UserPermission : BaseEntity
    {
        /// <summary>
        /// The ID of the user
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// The ID of the permission
        /// </summary>
        [Required]
        public int PermissionId { get; set; }
        
        /// <summary>
        /// Whether the permission is granted
        /// </summary>
        public bool IsGranted { get; set; } = true;
        
        /// <summary>
        /// When the permission was granted
        /// </summary>
        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Who granted the permission
        /// </summary>
        [MaxLength(100)]
        public string GrantedBy { get; set; } = string.Empty;
        
        /// <summary>
        /// Navigation property for the user
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        /// <summary>
        /// Navigation property for the permission
        /// </summary>
        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;
    }
} 