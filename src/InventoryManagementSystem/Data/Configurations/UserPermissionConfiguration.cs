using InventoryManagementSystem.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    /// <summary>
    /// Configuration for the UserPermission entity
    /// </summary>
    public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.ToTable("UserPermissions");
            
            builder.HasKey(up => up.Id);
            
            builder.Property(up => up.UserId)
                .IsRequired();
                
            builder.Property(up => up.PermissionId)
                .IsRequired();
                
            builder.Property(up => up.GrantedBy)
                .HasMaxLength(100);
                
            builder.HasIndex(up => new { up.UserId, up.PermissionId })
                .IsUnique();
                
            builder.HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 