using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementSystem.Data.Context;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Services
{
    /// <summary>
    /// Service for dashboard operations
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IUnitOfWork unitOfWork,
            IProductService productService,
            ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the dashboard data
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dashboard data</returns>
        public async Task<Result<DashboardViewModel>> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var dashboardViewModel = new DashboardViewModel();

                // Get product count
                var productCountResult = await _unitOfWork.Products.CountAsync(p => p.IsActive);
                if (productCountResult.IsSuccess)
                {
                    dashboardViewModel.TotalProducts = productCountResult.Value;
                }

                // Get supplier count
                var supplierCountResult = await _unitOfWork.Suppliers.CountAsync(s => s.IsActive);
                if (supplierCountResult.IsSuccess)
                {
                    dashboardViewModel.TotalSuppliers = supplierCountResult.Value;
                }

                // Get category count
                var categoryCountResult = await _unitOfWork.Categories.CountAsync(c => c.IsActive);
                if (categoryCountResult.IsSuccess)
                {
                    dashboardViewModel.TotalCategories = categoryCountResult.Value;
                }

                // Get low stock products
                var lowStockResult = await _productService.GetProductsNeedingReorderAsync();
                if (lowStockResult.IsSuccess && lowStockResult.Value != null)
                {
                    dashboardViewModel.LowStockProducts = lowStockResult.Value.ToList();
                }

                // Get total inventory value
                var inventoryValueResult = await _unitOfWork.Products.GetTotalInventoryValueAsync();
                if (inventoryValueResult.IsSuccess)
                {
                    dashboardViewModel.TotalInventoryValue = inventoryValueResult.Value;
                }

                return Result<DashboardViewModel>.Success(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard data");
                return Result<DashboardViewModel>.Failure("Error generating dashboard data");
            }
        }
    }
} 