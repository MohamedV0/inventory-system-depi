using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties configuration
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.UnitOfMeasurement)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Cost)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.UpdatedBy)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(p => p.SKU)
                .IsUnique()
                .HasDatabaseName("IX_Product_SKU");

            builder.HasIndex(p => p.Name)
                .HasDatabaseName("IX_Product_Name");

            // Relationships
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Product_Category");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_Price", "[Price] >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_Cost", "[Cost] >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_ReorderLevel", "[ReorderLevel] >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_CurrentStock", "[CurrentStock] >= 0"));
        }
    }
} 