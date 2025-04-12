using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Helpers;
using X.PagedList;
using System.Threading.Tasks;
using System.Collections.Generic;
using InventoryManagementSystem.Models.DTOs;
using System.Linq;
using InventoryManagementSystem.Extensions;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IProductSupplierService _productSupplierService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IProductSupplierService productSupplierService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _productSupplierService = productSupplierService;
            _logger = logger;
        }

        [Authorize(Policy = "CanViewProducts")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = null, string category = null, string stockFilter = null)
        {
            // Determine stock filtering
            bool? lowStock = null;
            bool? outOfStock = null;
            
            if (!string.IsNullOrEmpty(stockFilter))
            {
                switch (stockFilter.ToLower())
                {
                    case "low":
                        lowStock = true;
                        break;
                    case "out":
                        outOfStock = true;
                        break;
                }
            }
            
            // Pass the category parameter to the service layer
            var result = await _productService.GetPagedProductsAsync(
                page: page, 
                pageSize: pageSize, 
                searchTerm: searchTerm,
                categoryName: category,
                lowStock: lowStock,
                outOfStock: outOfStock);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve products: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }
            
            // Store the search term and category in ViewBag for use in the view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = category;
            ViewBag.StockFilter = stockFilter;

            // Get categories for the category filter dropdown
            var categoriesResult = await _categoryService.GetCategoriesAsync(pageSize: 100); // Get all categories (limited to 100)
            if (categoriesResult.IsSuccess)
            {
                ViewBag.Categories = categoriesResult.Value.Select(c => c.Name).ToList();
            }
            else
            {
                _logger.LogWarning("Failed to retrieve categories for filter: {Message}", categoriesResult.Message);
                TempData["WarningMessage"] = "Could not load category filters. " + categoriesResult.Message;
                ViewBag.Categories = new List<string>(); // Empty list as fallback
            }

            return View(result.Value);
        }

        [Authorize(Policy = "CanViewProducts")]
        public async Task<IActionResult> LowStock(int page = 1, int pageSize = 10)
        {
            var result = await _productService.GetProductsNeedingReorderAsync();
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve low stock products: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            // Convert to PagedList
            var products = result.Value.ToList();
            var pagedList = new StaticPagedList<ProductListItemViewModel>(
                products.Skip((page - 1) * pageSize).Take(pageSize),
                page,
                pageSize,
                products.Count
            );

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            
            return View(pagedList);
        }

        [Authorize(Policy = "CanViewProducts")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve product details for ID {ProductId}: {Message}", id, result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            return View(result.Value);
        }

        [Authorize(Policy = "CanCreateProducts")]
        public async Task<IActionResult> Create(int? categoryId = null)
        {
            // Get categories for dropdown
            var categoriesResult = await _categoryService.GetCategoriesAsync();
            
            if (!categoriesResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve categories for product creation: {Message}", categoriesResult.Message);
                TempData["ErrorMessage"] = categoriesResult.Message;
                return View("Error", new ErrorViewModel { Message = categoriesResult.Message });
            }
            
            ViewBag.Categories = categoriesResult.Value;
            
            // Get suppliers for dropdown
            var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
            
            if (!suppliersResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve suppliers for product creation: {Message}", suppliersResult.Message);
                TempData["WarningMessage"] = "Could not load suppliers. " + suppliersResult.Message;
            }
            else
            {
                ViewBag.Suppliers = suppliersResult.Value;
            }
            
            // Pre-populate the category if specified
            var model = new CreateProductViewModel();
            if (categoryId.HasValue)
            {
                model.CategoryId = categoryId.Value;
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanCreateProducts")]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categoriesResult = await _categoryService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.IsSuccess 
                    ? categoriesResult.Value 
                    : Enumerable.Empty<CategoryViewModel>();
                
                var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
                ViewBag.Suppliers = suppliersResult.IsSuccess 
                    ? suppliersResult.Value 
                    : Enumerable.Empty<SupplierListItemViewModel>();
                
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return View(model);
            }

            var result = await _productService.CreateProductAsync(model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to create product: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;
                
                var categoriesResult = await _categoryService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.IsSuccess 
                    ? categoriesResult.Value 
                    : Enumerable.Empty<CategoryViewModel>();
                
                var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
                ViewBag.Suppliers = suppliersResult.IsSuccess 
                    ? suppliersResult.Value 
                    : Enumerable.Empty<SupplierListItemViewModel>();
                
                return View(model);
            }

            // Create product-supplier relationship if a supplier was selected
            if (model.PrimarySupplierID.HasValue && 
                model.SupplierUnitPrice.HasValue && 
                model.LeadTimeDays.HasValue && 
                model.MinimumOrderQuantity.HasValue)
            {
                var productSupplierModel = new CreateProductSupplierViewModel
                {
                    ProductId = result.Value.Id,
                    SupplierId = model.PrimarySupplierID.Value,
                    UnitPrice = model.SupplierUnitPrice.Value,
                    LeadTimeDays = model.LeadTimeDays.Value,
                    MinimumOrderQuantity = model.MinimumOrderQuantity.Value,
                    IsPreferredSupplier = model.IsPreferredSupplier
                };
                
                var supplierResult = await _productSupplierService.CreateProductSupplierAsync(productSupplierModel);
                if (!supplierResult.IsSuccess)
                {
                    _logger.LogWarning("Product created but failed to link supplier: {Message}", supplierResult.Message);
                    TempData["WarningMessage"] = "Product created but failed to link supplier: " + supplierResult.Message;
                }
                else
                {
                    TempData["SuccessMessage"] = "Product created successfully with supplier assignment.";
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Product created successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanEditProducts")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve product for editing with ID {ProductId}: {Message}", id, result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            // Get categories for dropdown
            var categoriesResult = await _categoryService.GetCategoriesAsync();
            if (!categoriesResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve categories for product editing: {Message}", categoriesResult.Message);
                TempData["ErrorMessage"] = categoriesResult.Message;
                return View("Error", new ErrorViewModel { Message = categoriesResult.Message });
            }
            
            // Get suppliers for dropdown
            var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
            if (suppliersResult.IsSuccess)
            {
                ViewBag.Suppliers = suppliersResult.Value;
            }
            else
            {
                _logger.LogWarning("Failed to retrieve suppliers for product editing: {Message}", suppliersResult.Message);
                TempData["WarningMessage"] = "Could not load suppliers. " + suppliersResult.Message;
                ViewBag.Suppliers = Enumerable.Empty<SupplierListItemViewModel>(); 
            }
            
            ViewBag.Categories = categoriesResult.Value;
            
            // Get product supplier information
            var productSuppliersResult = await _productSupplierService.GetProductSuppliersAsync(
                productId: id,
                pageSize: 1 // We just need the primary/preferred supplier
            );
            
            var updateModel = new UpdateProductViewModel
            {
                Id = result.Value.Id,
                Name = result.Value.Name,
                Description = result.Value.Description,
                SKU = result.Value.SKU,
                UnitOfMeasurement = result.Value.UnitOfMeasurement,
                Price = result.Value.Price,
                Cost = result.Value.Cost,
                ReorderLevel = result.Value.ReorderLevel,
                CurrentStock = result.Value.CurrentStock,
                CategoryId = result.Value.CategoryId,
                IsActive = result.Value.IsActive
            };
            
            // If we found product supplier info, populate the model
            if (productSuppliersResult.IsSuccess && productSuppliersResult.Value.Any())
            {
                var productSupplier = productSuppliersResult.Value.FirstOrDefault();
                if (productSupplier != null)
                {
                    updateModel.PrimarySupplierID = productSupplier.SupplierId;
                    updateModel.SupplierUnitPrice = productSupplier.UnitPrice;
                    updateModel.LeadTimeDays = productSupplier.LeadTimeDays;
                    updateModel.MinimumOrderQuantity = productSupplier.MinimumOrderQuantity;
                    updateModel.IsPreferredSupplier = productSupplier.IsPreferredSupplier;
                }
            }
            
            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanEditProducts")]
        public async Task<IActionResult> Edit(int id, UpdateProductViewModel model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning("ID mismatch in Edit Product: URL ID {UrlId} vs Model ID {ModelId}", id, model.Id);
                TempData["ErrorMessage"] = "Invalid product ID.";
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                var categoriesResult = await _categoryService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.IsSuccess 
                    ? categoriesResult.Value 
                    : Enumerable.Empty<CategoryViewModel>();
                
                var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
                ViewBag.Suppliers = suppliersResult.IsSuccess 
                    ? suppliersResult.Value 
                    : Enumerable.Empty<SupplierListItemViewModel>();
                
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return View(model);
            }

            var result = await _productService.UpdateProductAsync(id, model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to update product: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;
                
                var categoriesResult = await _categoryService.GetCategoriesAsync();
                ViewBag.Categories = categoriesResult.IsSuccess 
                    ? categoriesResult.Value 
                    : Enumerable.Empty<CategoryViewModel>();
                
                var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
                ViewBag.Suppliers = suppliersResult.IsSuccess 
                    ? suppliersResult.Value 
                    : Enumerable.Empty<SupplierListItemViewModel>();
                
                return View(model);
            }

            // Handle supplier information
            if (model.PrimarySupplierID.HasValue && 
                model.SupplierUnitPrice.HasValue && 
                model.LeadTimeDays.HasValue && 
                model.MinimumOrderQuantity.HasValue)
            {
                // Check if product-supplier relation already exists
                var productSuppliersResult = await _productSupplierService.GetProductSuppliersAsync(
                    productId: id,
                    supplierId: model.PrimarySupplierID.Value,
                    pageSize: 1
                );
                
                if (productSuppliersResult.IsSuccess && productSuppliersResult.Value.Any())
                {
                    // Update existing relationship
                    var existingRelation = productSuppliersResult.Value.First();
                    var updateModel = new UpdateProductSupplierViewModel
                    {
                        UnitPrice = model.SupplierUnitPrice.Value,
                        LeadTimeDays = model.LeadTimeDays.Value,
                        MinimumOrderQuantity = model.MinimumOrderQuantity.Value,
                        IsPreferredSupplier = model.IsPreferredSupplier
                    };
                    
                    await _productSupplierService.UpdateProductSupplierAsync(
                        id, 
                        model.PrimarySupplierID.Value, 
                        updateModel
                    );
                }
                else
                {
                    // Create new relationship
                    var createModel = new CreateProductSupplierViewModel
                    {
                        ProductId = id,
                        SupplierId = model.PrimarySupplierID.Value,
                        UnitPrice = model.SupplierUnitPrice.Value,
                        LeadTimeDays = model.LeadTimeDays.Value,
                        MinimumOrderQuantity = model.MinimumOrderQuantity.Value,
                        IsPreferredSupplier = model.IsPreferredSupplier
                    };
                    
                    await _productSupplierService.CreateProductSupplierAsync(createModel);
                }
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanDeleteProducts")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete product with ID {ProductId}: {Message}", id, result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 