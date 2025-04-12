using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InventoryManagementSystem.Models.Common;
using InventoryManagementSystem.Models.ViewModels.Reports;
using InventoryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagementSystem.Services
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<Result<byte[]>> ExportToPdfAsync(ReportDTO report)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(20);
                        page.Header().Element(x => ComposeHeader(x, report));
                        page.Content().Element(x => ComposeContent(x, report));
                        page.Footer().Element(x => ComposeFooter(x));
                    });
                });

                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return Result<byte[]>.Success(stream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                return Result<byte[]>.Failure($"Failed to generate PDF: {ex.Message}");
            }
        }

        private void ComposeHeader(IContainer container, ReportDTO report)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Report Type: {report.ReportType}")
                        .FontSize(20)
                        .Bold();
                    column.Item().Text($"Generated: {report.GeneratedDate:g}")
                        .FontSize(12);
                    column.Item().Text($"Period: {report.StartDate:d} - {report.EndDate:d}")
                        .FontSize(12);
                });
            });
        }

        private void ComposeContent(IContainer container, ReportDTO report)
        {
            container.Column(column =>
            {
                column.Spacing(20);
                column.Item().Element(x => ComposeSummary(x, report.Summary));
                column.Item().Element(x => ComposeTable(x, report));
            });
        }

        private void ComposeSummary(IContainer container, ReportSummaryDTO summary)
        {
            container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Summary").Bold();
                column.Item().Text($"Total Items: {summary.TotalItems:N0}");
                column.Item().Text($"Total Value: {summary.TotalValue:C2}");
                column.Item().Text($"Low Stock Items: {summary.LowStockItems:N0}");
                column.Item().Text($"Out of Stock Items: {summary.OutOfStockItems:N0}");

                if (summary.AdditionalMetrics?.Any() == true)
                {
                    column.Item().Text("Additional Metrics").Bold();
                    foreach (var metric in summary.AdditionalMetrics)
                    {
                        string formattedValue = metric.Key.Contains("Price") || metric.Key.Contains("Value") || metric.Key.Contains("Range")
                            ? metric.Value.ToString("C2")
                            : metric.Value.ToString("N2");
                        column.Item().Text($"{FormatMetricName(metric.Key)}: {formattedValue}");
                    }
                }
            });
        }

        private string FormatMetricName(string metricKey)
        {
            // Convert camelCase or PascalCase to space-separated words
            return System.Text.RegularExpressions.Regex.Replace(metricKey, "([a-z])([A-Z])", "$1 $2");
        }

        private void ComposeTable(IContainer container, ReportDTO report)
        {
            container.Table(table =>
            {
                // Adjust column widths for better formatting
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);     // Name
                    columns.RelativeColumn(1.5f);  // SKU
                    columns.RelativeColumn(1.5f);  // Category
                    columns.RelativeColumn(1);     // Quantity
                    columns.RelativeColumn(1.2f);  // Unit Price
                    columns.RelativeColumn(1.2f);  // Cost
                    columns.RelativeColumn(1.2f);  // Profit Margin
                    columns.RelativeColumn(1.2f);  // Total Value
                    columns.RelativeColumn(1.5f);  // Stock Status
                    columns.RelativeColumn(2);     // Last Updated
                });

                // Table header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Name").Bold();
                    header.Cell().Element(CellStyle).Text("SKU").Bold();
                    header.Cell().Element(CellStyle).Text("Category").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Quantity").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Cost").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Margin %").Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Total Value").Bold();
                    header.Cell().Element(CellStyle).Text("Stock Status").Bold();
                    header.Cell().Element(CellStyle).Text("Last Updated").Bold();

                    // Style the header
                    header.Cell().BorderBottom(1).BorderColor(Colors.Black);
                });

                // Table content
                foreach (var item in report.Items)
                {
                    table.Cell().Element(CellStyle).Text(item.Name);
                    table.Cell().Element(CellStyle).Text(item.AdditionalFields.GetValueOrDefault("SKU", "N/A"));
                    table.Cell().Element(CellStyle).Text(item.Category);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString("N0"));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.UnitPrice.ToString("C2"));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Metrics.GetValueOrDefault("Cost").ToString("C2"));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Metrics.GetValueOrDefault("ProfitMargin").ToString("N1") + "%");
                    table.Cell().Element(CellStyle).AlignRight().Text(item.TotalValue.ToString("C2"));
                    table.Cell().Element(CellStyle)
                        .Text(GetStockStatusText(item.Metrics.GetValueOrDefault("StockStatus")));
                    table.Cell().Element(CellStyle).Text(item.LastUpdated.ToString("MM/dd/yyyy h:mm tt"));
                }

                // Style the table
                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
            });
        }

        private string GetStockStatusText(decimal status) => status switch
        {
            0 => "Out of Stock",
            1 => "Below Reorder",
            2 => "Low Stock",
            _ => "In Stock"
        };

        private string GetStockStatusColor(decimal status) => status switch
        {
            0 => Colors.Red.Medium,
            1 => Colors.Orange.Medium,
            2 => Colors.Yellow.Medium,
            _ => Colors.Green.Medium
        };

        // Helper method for consistent cell styling
        private static IContainer CellStyle(IContainer container)
        {
            return container.PaddingHorizontal(5).PaddingVertical(3).DefaultTextStyle(x => x.FontSize(10));
        }

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Generated by Inventory Management System");
                });
            });
        }

        public async Task<Result<byte[]>> ExportToExcelAsync(ReportDTO report)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report");

                // Headers
                var headers = new[] { "Name", "Category", "Quantity", "Unit Price", "Total Value", "Last Updated" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                }

                // Data
                int row = 2;
                foreach (var item in report.Items)
                {
                    worksheet.Cell(row, 1).Value = item.Name;
                    worksheet.Cell(row, 2).Value = item.Category;
                    worksheet.Cell(row, 3).Value = item.Quantity;
                    worksheet.Cell(row, 4).Value = item.UnitPrice;
                    worksheet.Cell(row, 5).Value = item.TotalValue;
                    worksheet.Cell(row, 6).Value = item.LastUpdated;

                    // Format currency columns
                    worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(row, 6).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Add summary
                row += 2;
                worksheet.Cell(row, 1).Value = "Summary";
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                worksheet.Cell(row + 1, 1).Value = "Total Items:";
                worksheet.Cell(row + 1, 2).Value = report.Summary.TotalItems;
                worksheet.Cell(row + 2, 1).Value = "Total Value:";
                worksheet.Cell(row + 2, 2).Value = report.Summary.TotalValue;
                worksheet.Cell(row + 2, 2).Style.NumberFormat.Format = "#,##0.00";

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return Result<byte[]>.Success(stream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                return Result<byte[]>.Failure($"Failed to generate Excel file: {ex.Message}");
            }
        }

        public async Task<Result<byte[]>> ExportToCsvAsync(ReportDTO report)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);
                using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.CurrentCulture));

                // Write headers
                csv.WriteField("Name");
                csv.WriteField("Category");
                csv.WriteField("Quantity");
                csv.WriteField("Unit Price");
                csv.WriteField("Total Value");
                csv.WriteField("Last Updated");
                csv.NextRecord();

                // Write data
                foreach (var item in report.Items)
                {
                    csv.WriteField(item.Name);
                    csv.WriteField(item.Category);
                    csv.WriteField(item.Quantity);
                    csv.WriteField(item.UnitPrice.ToString("F2"));
                    csv.WriteField(item.TotalValue.ToString("F2"));
                    csv.WriteField(item.LastUpdated.ToString("g"));
                    csv.NextRecord();
                }

                // Write summary
                csv.NextRecord();
                csv.WriteField("Summary");
                csv.NextRecord();
                csv.WriteField("Total Items");
                csv.WriteField(report.Summary.TotalItems);
                csv.NextRecord();
                csv.WriteField("Total Value");
                csv.WriteField(report.Summary.TotalValue.ToString("F2"));

                writer.Flush();
                return Result<byte[]>.Success(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CSV report");
                return Result<byte[]>.Failure($"Failed to generate CSV file: {ex.Message}");
            }
        }
    }
} 