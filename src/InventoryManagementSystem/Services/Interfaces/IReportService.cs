using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels.Reports;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementSystem.Services.Interfaces
{
    /// <summary>
    /// Service for generating and exporting inventory reports
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates a report based on the provided parameters
        /// </summary>
        /// <param name="parameters">Report generation parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Result containing the generated report or error details</returns>
        Task<Result<ReportDTO>> GenerateReportAsync(
            ReportParameters parameters,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports a report to the specified format
        /// </summary>
        /// <param name="parameters">Report parameters</param>
        /// <param name="format">Export format (pdf, excel, csv)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A Result containing the exported report bytes or error details</returns>
        Task<Result<byte[]>> ExportReportAsync(
            ReportParameters parameters,
            string format,
            CancellationToken cancellationToken = default);
    }
} 