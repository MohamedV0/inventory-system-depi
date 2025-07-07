using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Helpers;
using X.PagedList;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class StocksController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IProductService _productService;
        private readonly ILogger<StocksController> _logger;

        public StocksController(
            IStockService stockService,
            IProductService productService,
            ILogger<StocksController> logger)
        {
            _stockService = stockService;
            _productService = productService;
            _logger = logger;
        }

        [Authorize(Policy = "CanViewStock")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null, string? referenceSearch = null, string? reasonSearch = null, string? sortBy = null, bool ascending = true, int? productId = null, int? transactionType = null, string? startDate = null, string? endDate = null)
        {
            // Parse date strings to DateTime objects
            DateTime? startDateValue = null;
            DateTime? endDateValue = null;
            
            if (!string.IsNullOrEmpty(startDate))
            {
                if (DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    startDateValue = parsedStartDate;
                }
            }
            
            if (!string.IsNullOrEmpty(endDate))
            {
                if (DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    endDateValue = parsedEndDate;
                }
            }
            
            var result = await _stockService.GetStockHistoryAsync(page, pageSize, searchTerm, referenceSearch, reasonSearch, sortBy, ascending, productId, transactionType, startDateValue, endDateValue);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve stock history: {Message}", result.Message);
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            // Get products for filter dropdown
            var productsResult = await _productService.GetProductsAsync();
            if (productsResult.IsSuccess)
            {
                ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", productId);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.ReferenceSearch = referenceSearch;
            ViewBag.ReasonSearch = reasonSearch;
            ViewBag.SortBy = sortBy;
            ViewBag.Ascending = ascending;
            ViewBag.ProductId = productId;
            ViewBag.TransactionType = transactionType;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            
            return View(result.Value);
        }

        public async Task<IActionResult> Details(int id)
        {
            var result = await _stockService.GetStockHistoryByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve stock history details: {Message}", result.Message);
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            // Pass the detailed view model directly
            return View(result.Value);
        }

        [Authorize(Policy = "CanAddStock")]
        public async Task<IActionResult> AddStock(int? productId = null)
        {
            var productsResult = await _productService.GetProductsAsync();
            if (!productsResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve products for stock addition: {Message}", productsResult.Message);
                return View("Error", new ErrorViewModel { Message = productsResult.Message });
            }

            ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", productId);
            
            var model = new AddStockViewModel();
            if (productId.HasValue)
            {
                var productResult = await _productService.GetProductByIdAsync(productId.Value);
                if (productResult.IsSuccess)
                {
                    model.ProductId = productId.Value;
                    model.ProductName = productResult.Value.Name;
                    model.CurrentStock = productResult.Value.CurrentStock;
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanAddStock")]
        public async Task<IActionResult> AddStock(AddStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                return View(model);
            }

            var result = await _stockService.AddStockAsync(model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to add stock: {Message}", result.Message);
                
                if (result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
                
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                
                return View(model);
            }

            TempData["SuccessMessage"] = $"Successfully added {model.Quantity} unit(s) to inventory.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanRemoveStock")]
        public async Task<IActionResult> RemoveStock(int? productId = null)
        {
            var productsResult = await _productService.GetProductsAsync();
            if (!productsResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve products for stock removal: {Message}", productsResult.Message);
                return View("Error", new ErrorViewModel { Message = productsResult.Message });
            }

            ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", productId);
            
            var model = new RemoveStockViewModel();
            if (productId.HasValue)
            {
                var productResult = await _productService.GetProductByIdAsync(productId.Value);
                if (productResult.IsSuccess)
                {
                    model.ProductId = productId.Value;
                    model.ProductName = productResult.Value.Name;
                    model.CurrentStock = productResult.Value.CurrentStock;
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanRemoveStock")]
        public async Task<IActionResult> RemoveStock(RemoveStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                return View(model);
            }

            var result = await _stockService.RemoveStockAsync(model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to remove stock: {Message}", result.Message);
                
                if (result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
                
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                
                return View(model);
            }

            TempData["SuccessMessage"] = $"Successfully removed {model.Quantity} unit(s) from inventory.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanAdjustStock")]
        public async Task<IActionResult> AdjustStock(int? productId = null)
        {
            var productsResult = await _productService.GetProductsAsync();
            if (!productsResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve products for stock adjustment: {Message}", productsResult.Message);
                return View("Error", new ErrorViewModel { Message = productsResult.Message });
            }

            ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", productId);
            
            var model = new AdjustStockViewModel();
            if (productId.HasValue)
            {
                var productResult = await _productService.GetProductByIdAsync(productId.Value);
                if (productResult.IsSuccess)
                {
                    model.ProductId = productId.Value;
                    model.ProductName = productResult.Value.Name;
                    model.CurrentStock = productResult.Value.CurrentStock;
                    model.NewQuantity = productResult.Value.CurrentStock; // Default to current stock
                }
            }
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanAdjustStock")]
        public async Task<IActionResult> AdjustStock(AdjustStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                return View(model);
            }

            var result = await _stockService.AdjustStockAsync(model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to adjust stock: {Message}", result.Message);
                
                if (result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
                
                var productsResult = await _productService.GetProductsAsync();
                if (productsResult.IsSuccess)
                {
                    ViewBag.Products = new SelectList(productsResult.Value, "Id", "Name", model.ProductId);
                }
                
                return View(model);
            }

            TempData["SuccessMessage"] = $"Successfully adjusted stock to {model.NewQuantity} unit(s).";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ProductHistory(int id)
        {
            var productResult = await _productService.GetProductByIdAsync(id);
            if (!productResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve product for stock history: {Message}", productResult.Message);
                return View("Error", new ErrorViewModel { Message = productResult.Message });
            }

            var historyResult = await _stockService.GetProductStockHistoryAsync(id);
            if (!historyResult.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve stock history for product: {Message}", historyResult.Message);
                return View("Error", new ErrorViewModel { Message = historyResult.Message });
            }

            ViewBag.Product = productResult.Value;
            return View(historyResult.Value);
        }

        public async Task<IActionResult> LowStock(int page = 1, int pageSize = 10)
        {
            // Redirect to the consolidated low stock view in ProductsController
            return RedirectToAction("LowStock", "Products", new { page, pageSize });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int productId)
        {
            var result = await _productService.GetProductByIdAsync(productId);
            
            if (!result.IsSuccess)
                return Json(new { success = false, message = result.Message });

            return Json(new { 
                success = true, 
                currentStock = result.Value.CurrentStock,
                name = result.Value.Name,
                sku = result.Value.SKU
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentStock(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            
            if (!result.IsSuccess)
                return NotFound();

            return Content(result.Value.CurrentStock.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new { results = new List<object>() });

            var productsResult = await _productService.GetProductsAsync(
                page: 1,
                pageSize: 10,
                searchTerm: term);

            if (!productsResult.IsSuccess)
                return Json(new { results = new List<object>() });

            var result = productsResult.Value.Select(p => new 
            {
                id = p.Id,
                text = $"{p.Name} ({p.SKU})"
            });

            return Json(new { results = result });
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get low stock items
                var lowStockResult = await _stockService.GetLowStockItemsAsync();
                ViewBag.LowStockItems = lowStockResult.IsSuccess ? lowStockResult.Value.Take(5).ToList() : new List<LowStockViewModel>();
                ViewBag.LowStockCount = lowStockResult.IsSuccess ? lowStockResult.Value.Count() : 0;
                
                // Get recent transactions
                var recentTransactionsResult = await _stockService.GetStockHistoryAsync(
                    page: 1, 
                    pageSize: 5,
                    sortBy: "date",
                    ascending: false);
                ViewBag.RecentTransactions = recentTransactionsResult.IsSuccess ? recentTransactionsResult.Value : new StaticPagedList<StockHistoryViewModel>(new List<StockHistoryViewModel>(), 1, 1, 0);
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inventory dashboard");
                return View("Error", new ErrorViewModel { Message = "Error loading inventory dashboard" });
            }
        }
    }
} 