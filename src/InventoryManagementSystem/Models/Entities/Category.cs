using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Models.Entities.Base;

namespace InventoryManagementSystem.Models.Entities;

public class Category : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public new bool IsActive { get; set; }

    // Navigation property for related products
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
} 