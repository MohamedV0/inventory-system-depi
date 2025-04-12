namespace InventoryManagementSystem.Models.Entities.Base
{
    /// <summary>
    /// Base interface for all entities
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Primary key for the entity
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Indicates whether the entity is active in the system
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Indicates whether the entity is deleted
        /// </summary>
        bool IsDeleted { get; }
    }
} 