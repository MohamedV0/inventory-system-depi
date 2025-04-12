using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Helpers;
using X.PagedList;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [Authorize(Policy = "CanViewCategories")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = null, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetCategoriesAsync(page, pageSize, searchTerm, cancellationToken: cancellationToken);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve categories: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }
            
            // Store the search term in ViewBag for use in the view
            ViewBag.SearchTerm = searchTerm;

            return View(result.Value);
        }

        [Authorize(Policy = "CanManageCategories")]
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageCategories")]
        public async Task<IActionResult> Create(CategoryViewModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return View(model);
            }

            var result = await _categoryService.CreateCategoryAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to create category: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanManageCategories")]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve category for editing: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageCategories")]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model, CancellationToken cancellationToken = default)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Invalid category ID";
                return View("Error", new ErrorViewModel { Message = "Invalid category ID" });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the validation errors.";
                return View(model);
            }

            var result = await _categoryService.UpdateCategoryAsync(id, model, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to update category: {Message}", result.Message);
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View(model);
            }

            TempData["SuccessMessage"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "CanViewCategories")]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve category details: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageCategories")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete category: {Message}", result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
} 