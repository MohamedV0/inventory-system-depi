using InventoryManagementSystem.Models.Entities;
using InventoryManagementSystem.Models.Entities.Base;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data.Configurations;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace InventoryManagementSystem.Data.Context;

/// <summary>
/// Application's main database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<ProductSupplier> ProductSuppliers { get; set; } = null!;
    public DbSet<StockHistory> StockHistories { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<UserActivity> UserActivities { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new SupplierConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSupplierConfiguration());
        modelBuilder.ApplyConfiguration(new StockHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new UserActivityConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserPermissionConfiguration());

        // Apply global query filters
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
        modelBuilder.Entity<ProductSupplier>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
        modelBuilder.Entity<StockHistory>().HasQueryFilter(e => e.IsActive && !e.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserActivity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserPermission>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await UpdateAuditFieldsAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFieldsAsync().Wait();
        return base.SaveChanges();
    }

    private async Task UpdateAuditFieldsAsync()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (
                e.State == EntityState.Added || 
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted
            ));

        var currentTime = DateTime.UtcNow;
        var currentUser = await GetCurrentUserAsync();

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity entity)
            {
                var currentUsername = await GetCurrentUserAsync();
                
                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.SetCreatedBy(currentUsername);
                        break;

                    case EntityState.Modified:
                        entry.Property("CreatedAt").IsModified = false;
                        entry.Property("CreatedBy").IsModified = false;
                        entity.UpdateAuditFields(currentUsername);
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entity.Delete(currentUsername);
                        break;
                }
            }
        }
    }

    private async Task<string> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            return user.Identity.Name ?? "System";
        }
        return "System";
    }
} 