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
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
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
    options.AddPolicy("CanViewProducts", policy => policy.AddRequirements(new PermissionRequirement("CanViewProducts")));
    options.AddPolicy("CanCreateProducts", policy => policy.AddRequirements(new PermissionRequirement("CanCreateProducts")));
    options.AddPolicy("CanEditProducts", policy => policy.AddRequirements(new PermissionRequirement("CanEditProducts")));
    options.AddPolicy("CanDeleteProducts", policy => policy.AddRequirements(new PermissionRequirement("CanDeleteProducts")));
    
    // Stock policies
    options.AddPolicy("CanViewStock", policy => policy.AddRequirements(new PermissionRequirement("CanViewStock")));
    options.AddPolicy("CanAddStock", policy => policy.AddRequirements(new PermissionRequirement("CanAddStock")));
    options.AddPolicy("CanRemoveStock", policy => policy.AddRequirements(new PermissionRequirement("CanRemoveStock")));
    options.AddPolicy("CanAdjustStock", policy => policy.AddRequirements(new PermissionRequirement("CanAdjustStock")));
    
    // Category policies
    options.AddPolicy("CanViewCategories", policy => policy.AddRequirements(new PermissionRequirement("CanViewCategories")));
    options.AddPolicy("CanManageCategories", policy => policy.AddRequirements(new PermissionRequirement("CanManageCategories")));
    
    // Supplier policies
    options.AddPolicy("CanViewSuppliers", policy => policy.AddRequirements(new PermissionRequirement("CanViewSuppliers")));
    options.AddPolicy("CanManageSuppliers", policy => policy.AddRequirements(new PermissionRequirement("CanManageSuppliers")));
    
    // Report policies
    options.AddPolicy("CanViewReports", policy => policy.AddRequirements(new PermissionRequirement("CanViewReports")));
    options.AddPolicy("CanExportReports", policy => policy.AddRequirements(new PermissionRequirement("CanExportReports")));
    
    // Notification policies
    options.AddPolicy("CanViewNotifications", policy => policy.AddRequirements(new PermissionRequirement("CanViewNotifications")));
    
    // Dashboard policy
    options.AddPolicy("CanAccessDashboard", policy => policy.AddRequirements(new PermissionRequirement("CanAccessDashboard")));
    
    // User Management policies
    options.AddPolicy("CanViewUsers", policy => policy.AddRequirements(new PermissionRequirement("CanViewUsers")));
    options.AddPolicy("CanCreateUsers", policy => policy.AddRequirements(new PermissionRequirement("CanCreateUsers")));
    options.AddPolicy("CanEditUsers", policy => policy.AddRequirements(new PermissionRequirement("CanEditUsers")));
    options.AddPolicy("CanDeleteUsers", policy => policy.AddRequirements(new PermissionRequirement("CanDeleteUsers")));
    options.AddPolicy("CanManagePermissions", policy => policy.AddRequirements(new PermissionRequirement("CanManagePermissions")));
});

// Add custom authorization handler for dynamic permissions
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

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
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

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

// Apply migrations and seed data for both development and production
// Modified to support portfolio project deployment
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Apply migrations
    dbContext.Database.Migrate();
    
    // Check if database needs seeding (no products exist)
    var productCount = dbContext.Products.Count();
    if (productCount == 0)
    {
        // Seed the database with test data
        await DbSeeder.SeedDatabaseAsync(app.Services);
        
        // Create default permissions
        var userManagementService = scope.ServiceProvider.GetRequiredService<IUserManagementService>();
        await userManagementService.CreateDefaultPermissionsAsync();
        
        // Fix admin permissions
        await userManagementService.FixAdminPermissionsAsync();
    }
    
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Database migration and seeding completed successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
}

// Enable Swagger middleware in development only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

// Custom authorization handler for dynamic permissions
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionAuthorizationHandler(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null || !context.User.Identity.IsAuthenticated)
        {
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        // Check if the user is in the Admin role, which bypasses permission checks
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Check if the user has the required permission
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Name == requirement.PermissionName);

        if (permission != null)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && 
                                         up.PermissionId == permission.Id && 
                                         up.IsGranted);

            if (userPermission != null)
            {
                context.Succeed(requirement);
            }
        }
    }
}

// Custom authorization requirement for permissions
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }

    public PermissionRequirement(string permissionName)
    {
        PermissionName = permissionName;
    }
}
