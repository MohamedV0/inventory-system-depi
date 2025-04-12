using System;
using System.Diagnostics;
using System.Threading.Tasks;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDashboardService _dashboardService;

        public HomeController(
            ILogger<HomeController> logger,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        // Allow anonymous access to home page
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // If user is not authenticated, show the landing page
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return View("Landing");
            }

            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync();
                
                if (dashboardData != null && dashboardData.IsSuccess && dashboardData.Value != null)
                {
                    ViewBag.ProductCount = dashboardData.Value.TotalProducts;
                    ViewBag.SupplierCount = dashboardData.Value.TotalSuppliers;
                    ViewBag.CategoryCount = dashboardData.Value.TotalCategories;
                    ViewBag.LowStockCount = dashboardData.Value.LowStockProducts?.Count ?? 0;
                    ViewBag.TotalInventoryValue = dashboardData.Value.TotalInventoryValue;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve dashboard data: {Message}", dashboardData?.Message ?? "Unknown error");
                    // Set default values to avoid NaN in view
                    ViewBag.ProductCount = 0;
                    ViewBag.SupplierCount = 0;
                    ViewBag.CategoryCount = 0;
                    ViewBag.LowStockCount = 0;
                    ViewBag.TotalInventoryValue = 0;
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data for home page");
                // Set default values to avoid NaN in view
                ViewBag.ProductCount = 0;
                ViewBag.SupplierCount = 0;
                ViewBag.CategoryCount = 0;
                ViewBag.LowStockCount = 0;
                ViewBag.TotalInventoryValue = 0;
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
