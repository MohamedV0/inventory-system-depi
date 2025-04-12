using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties configuration
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.ContactPerson)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(s => s.Name)
                .HasDatabaseName("IX_Supplier_Name");

            builder.HasIndex(s => s.Email)
                .HasDatabaseName("IX_Supplier_Email");

            builder.HasIndex(s => s.Phone)
                .HasDatabaseName("IX_Supplier_Phone");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Supplier_IsActive");
        }
    }
} 