using System.Threading.Tasks;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels.Reports;

namespace InventoryManagementSystem.Services.Interfaces
{
    public interface IExportService
    {
        /// <summary>
        /// Exports the report data to a PDF format
        /// </summary>
        /// <param name="report">The report data to export</param>
        /// <returns>A Result containing the PDF file as a byte array if successful</returns>
        Task<Result<byte[]>> ExportToPdfAsync(ReportDTO report);

        /// <summary>
        /// Exports the report data to an Excel format
        /// </summary>
        /// <param name="report">The report data to export</param>
        /// <returns>A Result containing the Excel file as a byte array if successful</returns>
        Task<Result<byte[]>> ExportToExcelAsync(ReportDTO report);

        /// <summary>
        /// Exports the report data to a CSV format
        /// </summary>
        /// <param name="report">The report data to export</param>
        /// <returns>A Result containing the CSV file as a byte array if successful</returns>
        Task<Result<byte[]>> ExportToCsvAsync(ReportDTO report);
    }
} 