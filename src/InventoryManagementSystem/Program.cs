using InventoryManagementSystem.Configuration;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Services;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Middleware;
using FluentValidation;
using InventoryManagementSystem.Data.Repositories;
using InventoryManagementSystem.Data.Repositories.Interfaces;
using InventoryManagementSystem.Models.Validation;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using InventoryManagementSystem.Filters;
using InventoryManagementSystem.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Set DataDirectory
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}
AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);

// Add configuration
var databaseConfig = builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();
if (databaseConfig?.ConnectionString == null)
{
    throw new InvalidOperationException("Database connection string not found in DatabaseConfig.");
}

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Register the global exception filter - handles controller/action specific exceptions
    options.Filters.Add<GlobalExceptionFilterAttribute>();
});

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(databaseConfig.ConnectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: databaseConfig.MaxRetryCount,
            maxRetryDelay: TimeSpan.FromSeconds(databaseConfig.MaxRetryDelay),
            errorNumbersToAdd: null);
        
        if (databaseConfig.CommandTimeout > 0)
            sqlOptions.CommandTimeout(databaseConfig.CommandTimeout);
    });

    if (builder.Environment.IsDevelopment() && databaseConfig.EnableDetailedErrors)
        options.EnableDetailedErrors();
});

// Add Identity with simplified password requirements for MVP
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    
    // Configure lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    // Basic role policies
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
    
    // Product policies
    options.AddPolicy("CanViewProducts", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanCreateProducts", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanEditProducts", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CanDeleteProducts", policy => policy.RequireRole("Admin"));
    
    // Stock policies
    options.AddPolicy("CanViewStock", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanAddStock", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanRemoveStock", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanAdjustStock", policy => policy.RequireRole("Admin", "Staff"));
    
    // Category policies
    options.AddPolicy("CanViewCategories", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanManageCategories", policy => policy.RequireRole("Admin"));
    
    // Supplier policies
    options.AddPolicy("CanViewSuppliers", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanManageSuppliers", policy => policy.RequireRole("Admin"));
    
    // Report policies
    options.AddPolicy("CanViewReports", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("CanExportReports", policy => policy.RequireRole("Admin"));
    
    // Notification policies
    options.AddPolicy("CanViewNotifications", policy => policy.RequireRole("Admin", "Staff"));
    
    // Dashboard policy
    options.AddPolicy("CanAccessDashboard", policy => policy.RequireRole("Admin", "Staff"));
});

// Add UserActivity service
builder.Services.AddScoped<IUserActivityService, UserActivityService>();

// Add basic logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Warning);
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Register repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IProductSupplierRepository, ProductSupplierRepository>();
builder.Services.AddScoped<IStockHistoryRepository, StockHistoryRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register transaction logging
builder.Services.AddScoped<ILogger<DatabaseTransaction>, Logger<DatabaseTransaction>>();
builder.Services.AddScoped<ILogger<DistributedDatabaseTransaction>, Logger<DistributedDatabaseTransaction>>();

// Register validators
builder.Services.AddScoped<CategoryValidator>();
builder.Services.AddScoped<ProductValidator>();
builder.Services.AddScoped<SupplierValidator>();
builder.Services.AddScoped<ProductSupplierValidator>();

// Register services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductSupplierService, ProductSupplierService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register caching services
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Add HttpContextAccessor for UserContextService
builder.Services.AddHttpContextAccessor();

// Register export service
builder.Services.AddScoped<IExportService, ExportService>();

// Enable Swagger in development only
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Essential middleware only - order matters!

// Add security headers including correlation ID
app.Use(async (context, next) =>
{
    // Add correlation ID tracking
    context.Response.Headers["X-Correlation-ID"] = 
        context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? context.TraceIdentifier;
    
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

// Add request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Add exception handling middleware as a global safety net
// This catches exceptions not handled by controller filters
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add this line to map Razor Pages (required for Identity UI)
app.MapRazorPages();

// Apply migrations in development only
if (app.Environment.IsDevelopment() && databaseConfig?.EnableAutoMigration == true)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Apply migrations
        dbContext.Database.Migrate();
        
        // Seed the database with test data
        await DbSeeder.SeedDatabaseAsync(app.Services);
        
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Enable Swagger middleware in development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
