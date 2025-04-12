using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InventoryManagementSystem.Data.Configurations
{
    public class StockHistoryConfiguration : IEntityTypeConfiguration<StockHistory>
    {
        public void Configure(EntityTypeBuilder<StockHistory> builder)
        {
            // Primary Key
            builder.HasKey(sh => sh.Id);

            // Properties configuration
            builder.Property(sh => sh.ProductId)
                .IsRequired();

            builder.Property(sh => sh.QuantityChange)
                .IsRequired();

            builder.Property(sh => sh.PreviousStock)
                .IsRequired();

            builder.Property(sh => sh.NewStock)
                .IsRequired();

            builder.Property(sh => sh.Reason)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sh => sh.Date)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(sh => sh.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(sh => sh.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(sh => sh.Type)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(sh => sh.CreatedBy)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sh => sh.Notes)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(sh => sh.Product)
                .WithMany(p => p.StockHistory)
                .HasForeignKey(sh => sh.ProductId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_StockHistory_Product");

            // Indexes
            builder.HasIndex(sh => sh.ProductId)
                .HasDatabaseName("IX_StockHistory_ProductId");

            builder.HasIndex(sh => sh.Date)
                .HasDatabaseName("IX_StockHistory_Date");

            builder.HasIndex(sh => sh.Type)
                .HasDatabaseName("IX_StockHistory_Type");

            builder.HasIndex(sh => sh.ReferenceNumber)
                .HasDatabaseName("IX_StockHistory_ReferenceNumber");

            // Check constraints
            builder.ToTable(t => t.HasCheckConstraint("CK_StockHistory_NewStock", "[NewStock] >= 0"));
        }
    }
} 