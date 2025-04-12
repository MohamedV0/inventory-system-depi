using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Entities
{
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        [Url]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public new bool IsActive { get; set; } = true;

        // Navigation property
        public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
    }
} 