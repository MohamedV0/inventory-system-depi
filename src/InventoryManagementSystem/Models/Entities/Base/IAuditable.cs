namespace InventoryManagementSystem.Models.Entities.Base
{
    /// <summary>
    /// Interface for entities that need audit information
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        DateTime CreatedAt { get; }

        /// <summary>
        /// Username or identifier of the user who created the entity
        /// </summary>
        string CreatedBy { get; }

        /// <summary>
        /// UTC timestamp when the entity was last updated
        /// </summary>
        DateTime? UpdatedAt { get; }

        /// <summary>
        /// Username or identifier of the user who last updated the entity
        /// </summary>
        string? UpdatedBy { get; }
    }
} 