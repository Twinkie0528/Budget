using Budget.Core.Application.Dtos;
using Budget.Core.Interfaces;
using ClosedXML.Excel;

namespace Budget.Infrastructure.Excel;

public class ClosedXmlExcelExporter : IExcelExporter
{
    public Task<byte[]> ExportAsync(BudgetRequestDetailDto request, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Budget Request");

        // Header section
        var headerRow = 1;
        worksheet.Cell(headerRow, 1).Value = "Request Number";
        worksheet.Cell(headerRow, 2).Value = request.RequestNumber;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Title";
        worksheet.Cell(headerRow, 2).Value = request.Title;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Description";
        worksheet.Cell(headerRow, 2).Value = request.Description;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Channel";
        worksheet.Cell(headerRow, 2).Value = request.Channel;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Owner";
        worksheet.Cell(headerRow, 2).Value = request.Owner;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Frequency";
        worksheet.Cell(headerRow, 2).Value = request.Frequency;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Vendor";
        worksheet.Cell(headerRow, 2).Value = request.Vendor;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Fiscal Year";
        worksheet.Cell(headerRow, 2).Value = request.FiscalYear;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Currency";
        worksheet.Cell(headerRow, 2).Value = request.Currency;
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Total Amount";
        worksheet.Cell(headerRow, 2).Value = request.TotalAmount;
        worksheet.Cell(headerRow, 2).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Status";
        worksheet.Cell(headerRow, 2).Value = request.Status.ToString();
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        worksheet.Cell(++headerRow, 1).Value = "Created";
        worksheet.Cell(headerRow, 2).Value = $"{request.CreatedAt:yyyy-MM-dd HH:mm} by {request.CreatedBy}";
        worksheet.Cell(headerRow, 1).Style.Font.Bold = true;

        // Items header
        var itemsStartRow = headerRow + 3;
        var columns = new[]
        {
            "Row", "Description", "Category", "Sub-Category", "Quantity", "Unit Price", "Amount",
            "Cost Center", "Account Code", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        for (var col = 0; col < columns.Length; col++)
        {
            var cell = worksheet.Cell(itemsStartRow, col + 1);
            cell.Value = columns[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        // Items data
        var row = itemsStartRow + 1;
        foreach (var item in request.Items)
        {
            worksheet.Cell(row, 1).Value = item.RowNumber;
            worksheet.Cell(row, 2).Value = item.LineDescription;
            worksheet.Cell(row, 3).Value = item.Category;
            worksheet.Cell(row, 4).Value = item.SubCategory;
            worksheet.Cell(row, 5).Value = item.Quantity;
            worksheet.Cell(row, 6).Value = item.UnitPrice;
            worksheet.Cell(row, 7).Value = item.Amount;
            worksheet.Cell(row, 8).Value = item.CostCenter;
            worksheet.Cell(row, 9).Value = item.AccountCode;
            worksheet.Cell(row, 10).Value = item.Jan;
            worksheet.Cell(row, 11).Value = item.Feb;
            worksheet.Cell(row, 12).Value = item.Mar;
            worksheet.Cell(row, 13).Value = item.Apr;
            worksheet.Cell(row, 14).Value = item.May;
            worksheet.Cell(row, 15).Value = item.Jun;
            worksheet.Cell(row, 16).Value = item.Jul;
            worksheet.Cell(row, 17).Value = item.Aug;
            worksheet.Cell(row, 18).Value = item.Sep;
            worksheet.Cell(row, 19).Value = item.Oct;
            worksheet.Cell(row, 20).Value = item.Nov;
            worksheet.Cell(row, 21).Value = item.Dec;

            // Format numeric columns
            for (var col = 5; col <= 21; col++)
            {
                worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            }

            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        // Save to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}

