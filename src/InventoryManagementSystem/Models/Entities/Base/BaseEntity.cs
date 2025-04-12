using System.ComponentModel.DataAnnotations;

namespace InventoryManagementSystem.Models.Entities.Base;

/// <summary>
/// Base class for all entities in the system, providing common audit and tracking properties
/// </summary>
public abstract class BaseEntity : IEntity, IAuditable
{
    /// <summary>
    /// Primary key for the entity
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Indicates whether the entity is active in the system
    /// Inactive entities are filtered out by default using global query filters
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates whether the entity is deleted
    /// </summary>
    [Required]
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// UTC timestamp when the entity was created
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Username or identifier of the user who created the entity
    /// </summary>
    [Required]
    [StringLength(50)]
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the entity was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Username or identifier of the user who last updated the entity
    /// </summary>
    [StringLength(50)]
    public string? UpdatedBy { get; private set; }

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    // Add methods for state changes
    public virtual void Deactivate(string updatedBy)
    {
        IsActive = false;
        UpdateAuditFields(updatedBy);
    }

    public virtual void Activate(string updatedBy)
    {
        IsActive = true;
        UpdateAuditFields(updatedBy);
    }

    public virtual void Delete(string deletedBy)
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        IsActive = false;
        UpdateAuditFields(deletedBy);
    }

    public virtual void Restore(string restoredBy)
    {
        if (!IsDeleted) return;
        
        IsDeleted = false;
        IsActive = true;
        UpdateAuditFields(restoredBy);
    }

    public virtual void SetCreatedBy(string createdBy)
    {
        if (Id != 0) return; // Only allow setting creator for new entities
        CreatedBy = createdBy;
    }

    public virtual void UpdateAuditFields(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
} 