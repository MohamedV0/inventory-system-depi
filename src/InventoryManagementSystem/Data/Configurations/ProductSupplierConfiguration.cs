using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    public class ProductSupplierConfiguration : IEntityTypeConfiguration<ProductSupplier>
    {
        public void Configure(EntityTypeBuilder<ProductSupplier> builder)
        {
            // Primary Key
            builder.HasKey(ps => new { ps.ProductId, ps.SupplierId });

            // Properties configuration
            builder.Property(ps => ps.SupplierPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(ps => ps.LeadTimeDays)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ps => ps.IsPreferred)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ps => ps.LastPurchaseDate)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(ps => ps.SupplierSKU)
                .HasMaxLength(50);

            // Relationships
            builder.HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSuppliers)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ProductSupplier_Product");

            builder.HasOne(ps => ps.Supplier)
                .WithMany(s => s.ProductSuppliers)
                .HasForeignKey(ps => ps.SupplierId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ProductSupplier_Supplier");

            // Indexes
            builder.HasIndex(ps => new { ps.ProductId, ps.IsPreferred })
                .HasDatabaseName("IX_ProductSupplier_Product_IsPreferred");

            builder.HasIndex(ps => new { ps.SupplierId, ps.IsPreferred })
                .HasDatabaseName("IX_ProductSupplier_Supplier_IsPreferred");

            builder.HasIndex(ps => ps.LastPurchaseDate)
                .HasDatabaseName("IX_ProductSupplier_LastPurchaseDate");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_ProductSupplier_SupplierPrice", "[SupplierPrice] >= 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_ProductSupplier_LeadTimeDays", "[LeadTimeDays] >= 1"));
        }
    }
} 