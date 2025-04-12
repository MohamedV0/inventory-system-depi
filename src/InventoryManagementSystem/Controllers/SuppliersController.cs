using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.Helpers;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(
            ISupplierService supplierService,
            ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        [Authorize(Policy = "CanViewSuppliers")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchTerm = null, string? sortBy = null, bool ascending = true)
        {
            var result = await _supplierService.GetPagedSuppliersAsync(page, pageSize, searchTerm, sortBy, ascending);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve suppliers: {Message}", result.Message);
                return View("Error", new ErrorViewModel { Message = result.Message });
            }
            
            // Store the parameters in ViewBag for use in the view
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortBy = sortBy;
            ViewBag.Ascending = ascending;
            
            return View(result.Value);
        }

        [Authorize(Policy = "CanManageSuppliers")]
        public IActionResult Create()
        {
            return View(new CreateSupplierViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageSuppliers")]
        public async Task<IActionResult> Create(CreateSupplierViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _supplierService.CreateSupplierAsync(model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to create supplier: {Message}", result.Message);
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
                return View(model);
            }

            TempData["SuccessMessage"] = "Supplier created successfully.";
            return RedirectToAction(nameof(Details), new { id = result.Value.Id });
        }

        [Authorize(Policy = "CanManageSuppliers")]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _supplierService.GetSupplierByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve supplier for editing with ID {SupplierId}: {Message}", id, result.Message);
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            var updateModel = new UpdateSupplierViewModel
            {
                Id = result.Value.Id,
                Name = result.Value.Name,
                ContactPerson = result.Value.ContactPerson,
                Email = result.Value.Email,
                Phone = result.Value.Phone,
                Address = result.Value.Address,
                IsActive = result.Value.IsActive
            };

            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageSuppliers")]
        public async Task<IActionResult> Edit(int id, UpdateSupplierViewModel model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning("ID mismatch in Edit Supplier: URL ID {UrlId} vs Model ID {ModelId}", id, model.Id);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _supplierService.UpdateSupplierAsync(id, model);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to update supplier: {Message}", result.Message);
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
                return View(model);
            }

            TempData["SuccessMessage"] = "Supplier updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "CanViewSuppliers")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _supplierService.GetSupplierByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve supplier details for ID {SupplierId}: {Message}", id, result.Message);
                return View("Error", new ErrorViewModel { Message = result.Message });
            }

            return View(result.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanManageSuppliers")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _supplierService.DeleteSupplierAsync(id);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete supplier with ID {SupplierId}: {Message}", id, result.Message);
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = "Supplier deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 