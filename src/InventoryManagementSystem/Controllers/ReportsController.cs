using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Models.ViewModels.Reports;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using InventoryManagementSystem.Models.Common;
using Microsoft.AspNetCore.Authorization;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IExportService _exportService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            IExportService exportService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: Reports
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get categories for dropdown - using existing method with a large page size to get all
                var categoriesResult = await _categoryService.GetCategoriesAsync(pageSize: 100, cancellationToken: cancellationToken);
                if (categoriesResult.IsSuccess)
                {
                    ViewBag.Categories = new SelectList(
                        categoriesResult.Value,
                        "Id",
                        "Name",
                        null);
                }

                // Get suppliers for dropdown - using existing method with a large page size to get all
                var suppliersResult = await _supplierService.GetSuppliersAsync(pageSize: 100);
                if (suppliersResult.IsSuccess)
                {
                    ViewBag.Suppliers = new SelectList(
                        suppliersResult.Value.ToList(),
                        "Id",
                        "Name",
                        null);
                }

                // Set up report types
                ViewBag.ReportTypes = new SelectList(
                    new[]
                    {
                        new { Id = "Inventory", Name = "Inventory Report" },
                        new { Id = "Product", Name = "Product Report" },
                        new { Id = "Supplier", Name = "Supplier Report" }
                    },
                    "Id",
                    "Name",
                    "Inventory");

                return View(new ReportParameters());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report parameters");
                TempData["Error"] = "Error loading report parameters. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Reports/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> Generate(
            ReportParameters parameters,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return PartialView("_ReportForm", parameters);
            }

            try
            {
                var result = await _reportService.GenerateReportAsync(parameters, cancellationToken);
                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.Message);
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    return PartialView("_ReportForm", parameters);
                }

                // Return the report data directly without storing in TempData
                return Json(new { 
                    success = true, 
                    data = result.Value,
                    message = "Report generated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return Json(new { 
                    success = false, 
                    message = "Error generating report. Please try again."
                });
            }
        }

        // POST: Reports/Export
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> Export(
            ReportParameters parameters,
            string format,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
                return View("Index", parameters);

            try
            {
                // First generate the report
                var reportResult = await _reportService.GenerateReportAsync(parameters, cancellationToken);
                if (!reportResult.IsSuccess)
                {
                    ModelState.AddModelError("", reportResult.Message);
                    return View("Index", parameters);
                }

                // Then export it in the requested format
                var result = format.ToLowerInvariant() switch
                {
                    "pdf" => await _exportService.ExportToPdfAsync(reportResult.Value),
                    "excel" => await _exportService.ExportToExcelAsync(reportResult.Value),
                    "csv" => await _exportService.ExportToCsvAsync(reportResult.Value),
                    _ => Result<byte[]>.Failure($"Unsupported format: {format}")
                };

                if (!result.IsSuccess)
                {
                    ModelState.AddModelError("", result.Message);
                    return View("Index", parameters);
                }

                // Determine content type and filename
                var (contentType, fileExtension) = format.ToLowerInvariant() switch
                {
                    "pdf" => ("application/pdf", "pdf"),
                    "excel" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
                    "csv" => ("text/csv", "csv"),
                    _ => throw new ArgumentException($"Unsupported format: {format}")
                };

                var fileName = $"{parameters.ReportType}Report_{DateTime.Now:yyyyMMdd}.{fileExtension}";
                
                // Set the content disposition header to force download with the correct filename
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(result.Value, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report to {Format}", format);
                ModelState.AddModelError("", "Error exporting report. Please try again.");
                return View("Index", parameters);
            }
        }

        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> InventoryReport()
        {
            // Implementation of InventoryReport method
            return View();
        }

        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> StockMovementReport()
        {
            // Implementation of StockMovementReport method
            return View();
        }

        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> LowStockReport()
        {
            // Implementation of LowStockReport method
            return View();
        }

        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> ExpiryReport()
        {
            // Implementation of ExpiryReport method
            return View();
        }

        [Authorize(Policy = "CanViewReports")]
        public async Task<IActionResult> ActivityReport()
        {
            // Implementation of ActivityReport method
            return View();
        }
    }
} 