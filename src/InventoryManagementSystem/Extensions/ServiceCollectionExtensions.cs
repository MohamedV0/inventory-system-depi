using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Validation;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Extensions;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core application services to the service collection
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Services with proper error handling and validation
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductSupplierService, ProductSupplierService>();
        services.AddScoped<IUserContextService, UserContextService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
    
    /// <summary>
    /// Adds data repositories to the service collection
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register base repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Register specific repositories with proper error handling
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IProductSupplierRepository, ProductSupplierRepository>();

        return services;
    }
    
    /// <summary>
    /// Adds model validators to the service collection
    /// </summary>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        // Register validators for data integrity
        services.AddScoped<CategoryValidator>();
        services.AddScoped<ProductValidator>();
        services.AddScoped<SupplierValidator>();
        services.AddScoped<ProductSupplierValidator>();
        
        return services;
    }

    /// <summary>
    /// Adds health checks for the application
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }
} 