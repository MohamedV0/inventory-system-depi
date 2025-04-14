using InventoryManagementSystem.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    /// <summary>
    /// Configuration for the Permission entity
    /// </summary>
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.Description)
                .HasMaxLength(255);
                
            builder.Property(p => p.Category)
                .HasMaxLength(50);
                
            builder.HasIndex(p => p.Name)
                .IsUnique();
                
            builder.HasMany(p => p.UserPermissions)
                .WithOne(up => up.Permission)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 