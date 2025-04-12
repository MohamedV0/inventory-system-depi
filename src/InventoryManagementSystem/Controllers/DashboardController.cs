using System.Threading.Tasks;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [Authorize(Policy = "CanAccessDashboard")]
        public async Task<IActionResult> Index()
        {
            var result = await _dashboardService.GetDashboardDataAsync();
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to retrieve dashboard data: {Message}", result.Message);
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }

            return View(result.Value);
        }
    }
} 