using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using InventoryManagementSystem.Models.Identity;

namespace InventoryManagementSystem.Data
{
    public static class DbSeeder
    {
        private static readonly Random _random = new Random();
        private static readonly string[] _egyptianCities = new[] { "Cairo", "Alexandria", "Giza", "Sharm El Sheikh", "Luxor", "Aswan", "Hurghada", "Port Said", "Suez", "Tanta", "Mansoura", "Ismailia", "Faiyum", "Zagazig", "Damietta", "Minya", "Qena", "Beni Suef", "Sohag", "Asyut" };
        private static readonly string[] _egyptianStreets = new[] { "Nasr", "El Tahrir", "El Moez", "Fouad", "El Gomhoreya", "Talaat Harb", "26th of July", "Mohamed Ali", "Al Azhar", "Salah Salem", "Ramses", "El Hegaz", "Qasr El Nil", "El Gaish", "El Merghany", "El Ferdous", "El Bahr El Azam", "Pyramids", "El Mesaha", "Mesdaq" };
        private static readonly string[] _categoryPrefixes = new[] { "Electronics", "Office Supplies", "Furniture", "IT Equipment", "Kitchen", "Cleaning", "Stationery", "Decor", "Storage", "Lighting", "Audio", "Video", "Computer", "Networking", "Security", "Tools", "Maintenance", "Outdoor", "Indoor", "Health" };
        private static readonly string[] _categorySuffixes = new[] { "Devices", "Accessories", "Equipment", "Supplies", "Products", "Essentials", "Components", "Items", "Goods", "Materials", "Solutions", "Systems", "Units", "Tools", "Machines", "Instruments", "Appliances", "Hardware", "Fixtures", "Kits" };
        private static readonly string[] _supplierTypes = new[] { "Tech", "Office", "Furniture", "IT", "Electronics", "Supplies", "Equipment", "Hardware", "Solutions", "Systems", "Components", "Materials", "Products", "Goods", "Devices", "Appliances", "Accessories", "Tools", "Machines", "Instruments" };
        private static readonly string[] _supplierSuffixes = new[] { "Solutions", "Inc.", "Ltd.", "Group", "Co.", "Corporation", "International", "Enterprises", "Industries", "Systems", "Technologies", "Innovations", "Distributors", "Supplies", "Traders", "Partners", "Associates", "Imports", "Exports", "Egypt" };
        private static readonly string[] _firstNames = new[] { "Ahmed", "Mohamed", "Mahmoud", "Ali", "Hassan", "Hussein", "Ibrahim", "Karim", "Khaled", "Omar", "Tamer", "Youssef", "Amr", "Sherif", "Mostafa", "Amir", "Adel", "Tarek", "Walid", "Sameh", "Fatma", "Mona", "Sara", "Nour", "Heba", "Amira", "Amal", "Dina", "Eman", "Mariam", "Laila", "Rania", "Yasmin", "Nada", "Hala", "Salma", "Noha", "Rasha", "Samar", "Ghada" };
        private static readonly string[] _lastNames = new[] { "Mohamed", "Ahmed", "Ali", "Ibrahim", "Hassan", "Hussein", "Mahmoud", "Samir", "Sayed", "Abdelrahman", "Youssef", "Khalil", "Mostafa", "Farouk", "Magdy", "Nasser", "Salah", "Fawzy", "Kamal", "Hamdy", "Abbas", "Shawky", "Hamed", "Mansour", "Naguib", "Ramadan", "Ismail", "Abdallah", "Fouad", "Sobhy" };
        private static readonly string[] _productTypes = new[] { "Laptop", "Desktop", "Monitor", "Printer", "Scanner", "Keyboard", "Mouse", "Headset", "Webcam", "Speakers", "USB Drive", "Hard Drive", "SSD", "Router", "Switch", "Server", "UPS", "Cable", "Adapter", "Hub", "Desk", "Chair", "Table", "Cabinet", "Bookshelf", "Whiteboard", "Projector", "Screen", "Phone", "Tablet", "Paper", "Notebook", "Pen", "Pencil", "Stapler", "Scissors", "Tape", "Folder", "Binder", "Calculator" };
        private static readonly string[] _productAdjectives = new[] { "Professional", "Business", "Office", "Premium", "Deluxe", "Standard", "Basic", "Advanced", "High-End", "Entry-Level", "Compact", "Portable", "Wireless", "Wired", "Ergonomic", "Adjustable", "Foldable", "Lightweight", "Heavy-Duty", "Durable", "Slim", "Ultra", "Multi-Function", "All-in-One", "Touch", "Smart", "HD", "4K", "LED", "LCD" };
        private static readonly string[] _unitOfMeasurements = new[] { "Unit", "Pack", "Box", "Set", "Kit", "Bundle", "Piece", "Roll", "Ream", "Carton", "Pair", "Dozen", "Case" };
        private static readonly string[] _stockReasons = new[] {
            "Initial stock", "Opening inventory", "First batch", "Initial purchase", "Starter inventory",
            "Restocking", "Regular order", "Replenishment", "Inventory refresh", "Stock update",
            "Bulk purchase", "Volume order", "Wholesale acquisition", "Large order", "Quantity discount purchase",
            "Special order", "Custom purchase", "Specific requirement", "Special requirement fulfillment",
            "Emergency restock", "Urgent order", "Rush purchase", "Quick replenishment",
            "Seasonal stock", "Holiday inventory", "Special event preparation", "Promotional stock",
            "Customer order", "Client request", "Specific customer needs", "Customer specification",
            "Department use", "Internal use", "Office consumption", "Staff requirements",
            "Inventory adjustment", "Stock reconciliation", "Count correction", "Inventory verification"
        };

        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                
                await context.Database.MigrateAsync();
                
                // First seed roles and admin user
                await SeedRolesAndAdminAsync(userManager, roleManager);
                
                // Then seed other data
                await SeedDataAsync(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedRolesAndAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            string[] roles = { "Admin", "Staff" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user if it doesn't exist
            var adminEmail = "admin@inventory.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    LastLoginDate = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }

        private static string GenerateEmail(string name, string domain)
        {
            // Convert name to lowercase, remove spaces, and append domain
            return $"{name.ToLower().Replace(" ", ".")}@{domain.ToLower().Replace(" ", "")}.com";
        }

        private static string GeneratePhone()
        {
            // Generate Egyptian mobile format
            string prefix = _random.Next(0, 2) == 0 ? "011" : _random.Next(0, 2) == 0 ? "012" : _random.Next(0, 2) == 0 ? "010" : "015";
            return prefix + _random.Next(10000000, 99999999).ToString();
        }

        private static string GenerateAddress()
        {
            return $"{_random.Next(1, 200)} {GetRandomItem(_egyptianStreets)} Street, {GetRandomItem(_egyptianCities)}";
        }

        private static string GenerateWebsite(string companyName)
        {
            return $"www.{companyName.ToLower().Replace(" ", "").Replace(".", "")}.com";
        }

        private static string GenerateProductCode(string category, int index)
        {
            // Generate a code based on category abbreviation and index
            string prefix = new string(category.Split(' ').Select(s => char.ToUpper(s[0])).ToArray());
            return $"{prefix}-{index:D3}";
        }

        private static string GenerateSKU(string productName, int index)
        {
            // Generate SKU from product name abbreviation and index
            string prefix = new string(productName.Split(' ').Take(2).Select(s => char.ToUpper(s[0])).ToArray());
            return $"{prefix}{index:D4}";
        }

        private static string GenerateSupplierSKU(string supplierName, string productName, int index)
        {
            // Generate supplier-specific SKU
            string prefix = new string(supplierName.Split(' ').Select(s => char.ToUpper(s[0])).ToArray());
            string productPrefix = new string(productName.Split(' ').Take(1).Select(s => char.ToUpper(s[0])).ToArray());
            return $"{prefix}-{productPrefix}{index:D3}";
        }

        private static string GenerateReferenceNumber(string transactionType, int index)
        {
            return $"{transactionType}-{index:D4}";
        }

        private static T GetRandomItem<T>(T[] items)
        {
            return items[_random.Next(items.Length)];
        }

        private static int GetRandomNumber(int min, int max)
        {
            return _random.Next(min, max + 1);
        }

        private static decimal GetRandomPrice(decimal min, decimal max)
        {
            // Generate random decimal price between min and max
            double range = (double)(max - min);
            double sample = _random.NextDouble();
            double scaled = (sample * range) + (double)min;
            return Math.Round((decimal)scaled, 2);
        }

        private static DateTime GetRandomDate(int minDaysAgo, int maxDaysAgo)
        {
            return DateTime.UtcNow.AddDays(-_random.Next(minDaysAgo, maxDaysAgo + 1));
        }

        private static bool GetRandomBool(double trueProbability = 0.5)
        {
            return _random.NextDouble() < trueProbability;
        }

        private static DateTime? GetRandomNullableDate(int minDaysAgo, int maxDaysAgo, double nullProbability = 0.3)
        {
            if (_random.NextDouble() < nullProbability)
                return null;
            
            return GetRandomDate(minDaysAgo, maxDaysAgo);
        }

        private static string GenerateProductName()
        {
            return $"{GetRandomItem(_productAdjectives)} {GetRandomItem(_productTypes)}";
        }

        private static string GenerateCategoryName()
        {
            return $"{GetRandomItem(_categoryPrefixes)} {GetRandomItem(_categorySuffixes)}";
        }

        private static string GenerateSupplierName()
        {
            string city = GetRandomItem(_egyptianCities);
            string type = GetRandomItem(_supplierTypes);
            string suffix = GetRandomItem(_supplierSuffixes);
            
            return $"{city} {type} {suffix}";
        }

        private static string GeneratePersonName()
        {
            return $"{GetRandomItem(_firstNames)} {GetRandomItem(_lastNames)}";
        }
        
        private static decimal ApplySeasonalFactor(decimal baseValue, DateTime date)
        {
            // Define seasonal factors (Q1: Winter, Q2: Spring, Q3: Summer, Q4: Fall)
            var month = date.Month;
            var seasonalFactor = month switch
            {
                12 or 1 or 2 => 1.3m,  // Winter - high demand
                3 or 4 or 5 => 1.1m,   // Spring - moderate high
                6 or 7 or 8 => 0.8m,   // Summer - low
                _ => 1.2m              // Fall - rising
            };

            // Add some random variation
            var randomFactor = 0.9m + (decimal)(_random.NextDouble() * 0.2);
            return baseValue * seasonalFactor * randomFactor;
        }

        private static int ApplyDailyPattern(int baseQuantity, DateTime date)
        {
            // Define daily patterns
            var hour = date.Hour;
            var dayFactor = hour switch
            {
                >= 9 and <= 11 => 1.5,  // Morning peak
                >= 14 and <= 16 => 1.3, // Afternoon peak
                >= 17 and <= 19 => 1.2, // Evening moderate
                >= 20 or <= 8 => 0.7,   // Night/early morning low
                _ => 1.0                // Normal hours
            };

            return (int)(baseQuantity * dayFactor);
        }

        private static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Only seed if database is empty
            if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
            {
                return; // Database already has data
            }

            // Constants for data generation
            const int CATEGORY_COUNT = 100;
            const int SUPPLIER_COUNT = 50;
            const int PRODUCT_COUNT = 700;
            const int MIN_SUPPLIERS_PER_PRODUCT = 0;  // Some products may have no suppliers
            const int MAX_SUPPLIERS_PER_PRODUCT = 4;
            const int MIN_STOCK_HISTORY_PER_PRODUCT = 50;  // Increased for more data points
            const int MAX_STOCK_HISTORY_PER_PRODUCT = 200; // Increased for more data points
            
            // Distribution of stock scenarios with more nuanced probabilities
            const double NORMAL_STOCK_PROBABILITY = 0.5;    // 50% normal stock
            const double LOW_STOCK_PROBABILITY = 0.25;      // 25% low stock
            const double CRITICAL_STOCK_PROBABILITY = 0.15; // 15% critical stock
            const double OUT_OF_STOCK_PROBABILITY = 0.1;    // 10% out of stock

            Console.WriteLine("Starting to seed database with large dataset...");

            // Seed Categories
            Console.WriteLine($"Generating {CATEGORY_COUNT} categories...");
            var categories = new List<Category>();
            var uniqueCategories = new HashSet<string>();

            while (categories.Count < CATEGORY_COUNT)
            {
                string categoryName = GenerateCategoryName();
                if (uniqueCategories.Add(categoryName))
                {
                    categories.Add(new Category
                    {
                        Name = categoryName,
                        Description = $"{categoryName} for business and office use",
                        IsActive = GetRandomBool(0.95) // 95% are active
                    });
                }
            }
            
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("Categories seeded successfully.");

            // Seed Suppliers
            Console.WriteLine($"Generating {SUPPLIER_COUNT} suppliers...");
            var suppliers = new List<Supplier>();
            var uniqueSuppliers = new HashSet<string>();

            while (suppliers.Count < SUPPLIER_COUNT)
            {
                string supplierName = GenerateSupplierName();
                if (uniqueSuppliers.Add(supplierName))
                {
                    string contactPerson = GeneratePersonName();
                    string domain = supplierName.Split(' ')[0].ToLower() + supplierName.Split(' ')[1].ToLower();
                    
                    suppliers.Add(new Supplier
                    {
                        Name = supplierName,
                        ContactPerson = contactPerson,
                        Email = GenerateEmail(contactPerson, domain),
                        Phone = GeneratePhone(),
                        Address = GenerateAddress(),
                        Website = GenerateWebsite(supplierName),
                        Notes = $"Supplier for various {supplierName.Split(' ')[1].ToLower()} products",
                        IsActive = GetRandomBool(0.9) // 90% are active
                    });
                }
            }
            
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
            Console.WriteLine("Suppliers seeded successfully.");

            // Seed Products
            Console.WriteLine($"Generating {PRODUCT_COUNT} products...");
            var products = new List<Product>();
            var uniqueProductNames = new HashSet<string>();
            
            var categoryIds = await context.Categories.Select(c => c.Id).ToListAsync();
            
            while (products.Count < PRODUCT_COUNT)
            {
                string productName = GenerateProductName();
                if (uniqueProductNames.Add(productName))
                {
                    var categoryId = categoryIds[_random.Next(categoryIds.Count)];
                    var category = await context.Categories.FindAsync(categoryId);
                    
                    decimal cost = GetRandomPrice(50, 30000);
                    decimal price = cost * (decimal)(1 + (_random.NextDouble() * 0.5)); // 0-50% markup
                    
                    int reorderLevel = GetRandomNumber(3, 30);
                    int minStockLevel = GetRandomNumber(1, reorderLevel - 1);
                    int maxStockLevel = GetRandomNumber(reorderLevel * 2, reorderLevel * 5);
                    
                    // Determine stock scenario
                    int currentStock;
                    double stockScenario = _random.NextDouble();
                    
                    if (stockScenario < OUT_OF_STOCK_PROBABILITY)
                    {
                        // Out of stock (10% of products)
                        currentStock = 0;
                    }
                    else if (stockScenario < OUT_OF_STOCK_PROBABILITY + LOW_STOCK_PROBABILITY)
                    {
                        // Low stock (30% of products)
                        currentStock = GetRandomNumber(1, reorderLevel - 1);
                    }
                    else
                    {
                        // Normal stock (60% of products)
                        currentStock = GetRandomNumber(reorderLevel, maxStockLevel);
                    }
                    
                    products.Add(new Product
                    {
                        Name = productName,
                        Code = GenerateProductCode(category.Name, products.Count + 1),
                        Description = $"{productName} - High quality for office use",
                        SKU = GenerateSKU(productName, products.Count + 1),
                        UnitOfMeasurement = GetRandomItem(_unitOfMeasurements),
                        Price = Math.Round(price, 2),
                        Cost = Math.Round(cost, 2),
                        ReorderLevel = reorderLevel,
                        MinimumStockLevel = minStockLevel,
                        MaximumStockLevel = maxStockLevel,
                        CurrentStock = currentStock,
                        CategoryId = categoryId,
                        IsActive = GetRandomBool(0.95) // 95% are active
                    });
                }
            }
            
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            Console.WriteLine("Products seeded successfully.");

            // Seed ProductSuppliers
            Console.WriteLine("Generating product-supplier relationships...");
            var productSuppliers = new List<ProductSupplier>();
            var allProductIds = await context.Products.Select(p => p.Id).ToListAsync();
            var allSupplierIds = await context.Suppliers.Select(s => s.Id).ToListAsync();
            
            foreach (var productId in allProductIds)
            {
                var product = await context.Products.FindAsync(productId);
                
                // Determine how many suppliers this product will have
                int supplierCount = GetRandomNumber(MIN_SUPPLIERS_PER_PRODUCT, MAX_SUPPLIERS_PER_PRODUCT);
                
                if (supplierCount > 0)
                {
                    // Get random supplier IDs for this product
                    var randomSupplierIds = allSupplierIds.OrderBy(x => Guid.NewGuid()).Take(supplierCount).ToList();
                    
                    // First supplier is preferred
                    bool isFirstSupplier = true;
                    
                    foreach (var supplierId in randomSupplierIds)
                    {
                        var supplier = await context.Suppliers.FindAsync(supplierId);
                        
                        decimal unitPrice = GetRandomPrice(product.Cost * 0.9m, product.Cost * 1.1m); // Within 10% of cost
                        
                        productSuppliers.Add(new ProductSupplier
                        {
                            ProductId = productId,
                            SupplierId = supplierId,
                            UnitPrice = Math.Round(unitPrice, 2),
                            SupplierSKU = GenerateSupplierSKU(supplier.Name, product.Name, productSuppliers.Count + 1),
                            LeadTimeDays = GetRandomNumber(1, 21),
                            MinimumOrderQuantity = GetRandomNumber(1, 10),
                            IsPreferredSupplier = isFirstSupplier,
                            Notes = $"{(isFirstSupplier ? "Primary" : "Alternative")} supplier for {product.Name}",
                            LastPurchaseDate = GetRandomNullableDate(5, 120),
                            IsActive = GetRandomBool(0.9) // 90% are active
                        });
                        
                        isFirstSupplier = false;
                    }
                }
            }

            await context.ProductSuppliers.AddRangeAsync(productSuppliers);
            await context.SaveChangesAsync();
            Console.WriteLine("Product-supplier relationships seeded successfully.");

            // Enhanced Stock History Generation
            Console.WriteLine("Generating detailed stock history records...");
            var stockHistories = new List<StockHistory>();
            var startDate = DateTime.UtcNow.AddYears(-1); // Start from one year ago

            foreach (var productId in allProductIds)
            {
                var product = await context.Products.FindAsync(productId);
                var runningStock = 0;
                
                // Create product performance profile
                var isHighDemand = GetRandomBool(0.3); // 30% are high demand products
                var isSeasonalProduct = GetRandomBool(0.4); // 40% are seasonal products
                var hasTrend = GetRandomBool(0.6); // 60% have a trend (up or down)
                var trendFactor = hasTrend ? (_random.NextDouble() > 0.5 ? 1.002 : 0.998) : 1.0; // Subtle trend

                // Initial stock entry
                var initialStockDate = startDate;
                var initialQuantity = GetRandomNumber(20, 100);
                
                stockHistories.Add(new StockHistory
                {
                    ProductId = productId,
                    QuantityChange = initialQuantity,
                    PreviousStock = 0,
                    NewStock = initialQuantity,
                    Reason = "Initial stock",
                    Date = initialStockDate,
                    ReferenceNumber = GenerateReferenceNumber("INI", stockHistories.Count + 1),
                    UnitPrice = product.Cost,
                    Type = TransactionType.Initial,
                    IsActive = true
                });

                runningStock = initialQuantity;
                
                // Generate history entries across the year
                var currentDate = startDate;
                var historyCount = GetRandomNumber(MIN_STOCK_HISTORY_PER_PRODUCT, MAX_STOCK_HISTORY_PER_PRODUCT);
                
                for (int i = 1; i < historyCount; i++)
                {
                    // Advance time randomly (1-3 days)
                    currentDate = currentDate.AddDays(_random.Next(1, 4));
                    if (currentDate > DateTime.UtcNow) break;

                    // Apply seasonal and trend factors
                    var baseQuantity = isHighDemand ? GetRandomNumber(10, 30) : GetRandomNumber(5, 15);
                    if (isSeasonalProduct)
                    {
                        baseQuantity = (int)(baseQuantity * (decimal)ApplySeasonalFactor(1m, currentDate));
                    }
                    baseQuantity = (int)(baseQuantity * (decimal)Math.Pow(trendFactor, (currentDate - startDate).Days));

                    // Determine transaction type with time-based patterns
                    TransactionType transactionType;
                    int quantityChange;
                    string reason;
                    string referencePrefix;

                    var hourOfDay = currentDate.Hour;
                    var dayOfWeek = (int)currentDate.DayOfWeek;
                    var isBusinessHour = hourOfDay >= 9 && hourOfDay <= 17;
                    var isWeekday = dayOfWeek >= 1 && dayOfWeek <= 5;

                    // Adjust probabilities based on time
                    var stockInProb = isBusinessHour && isWeekday ? 0.4 : 0.2;
                    var stockOutProb = isBusinessHour ? 0.5 : 0.3;

                    var typeRandom = _random.NextDouble();
                    
                    if (typeRandom < stockInProb)
                    {
                        transactionType = TransactionType.StockIn;
                        quantityChange = ApplyDailyPattern(baseQuantity, currentDate);
                        reason = GetRandomItem(_stockReasons);
                        referencePrefix = "PO";
                    }
                    else if (typeRandom < stockInProb + stockOutProb)
                    {
                        transactionType = TransactionType.StockOut;
                        var maxOut = Math.Min(runningStock, ApplyDailyPattern(baseQuantity, currentDate));
                        quantityChange = maxOut > 0 ? -maxOut : 0;
                        reason = "Customer order";
                        referencePrefix = "SO";
                    }
                    else
                    {
                        transactionType = TransactionType.Adjustment;
                        quantityChange = GetRandomNumber(-3, 3); // Smaller adjustments
                        if (runningStock + quantityChange < 0)
                            quantityChange = -runningStock;
                        reason = "Inventory adjustment";
                        referencePrefix = "ADJ";
                    }

                    if (quantityChange == 0) continue;

                    var previousStock = runningStock;
                    runningStock += quantityChange;

                    // Add price variation based on market conditions
                    var priceVariation = 1m + ((decimal)_random.NextDouble() * 0.1m - 0.05m); // ±5% variation
                    var transactionPrice = transactionType == TransactionType.StockOut ? 
                        product.Price * priceVariation : 
                        product.Cost * priceVariation;

                    stockHistories.Add(new StockHistory
                    {
                        ProductId = productId,
                        QuantityChange = quantityChange,
                        PreviousStock = previousStock,
                        NewStock = runningStock,
                        Reason = reason,
                        Date = currentDate,
                        ReferenceNumber = GenerateReferenceNumber(referencePrefix, stockHistories.Count + 1),
                        UnitPrice = Math.Round(transactionPrice, 2),
                        Type = transactionType,
                        IsActive = true
                    });
                }

                // Update product's current stock
                if (stockHistories.Where(sh => sh.ProductId == productId).Any())
                {
                    var lastHistory = stockHistories.Where(sh => sh.ProductId == productId)
                                                  .OrderByDescending(sh => sh.Date)
                                                  .First();
                    product.CurrentStock = lastHistory.NewStock;
                    context.Products.Update(product);
                }
            }

            await context.StockHistories.AddRangeAsync(stockHistories);
            await context.SaveChangesAsync();
            Console.WriteLine("Stock history records seeded successfully.");
            
            Console.WriteLine("Database seeding completed successfully!");
        }
    }
} 