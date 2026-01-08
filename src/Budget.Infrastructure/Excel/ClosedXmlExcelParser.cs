using Budget.Core.Application.Dtos;
using Budget.Core.Interfaces;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;

namespace Budget.Infrastructure.Excel;

public class ClosedXmlExcelParser : IExcelParser
{
    private readonly ExcelParserOptions _options;

    public ClosedXmlExcelParser(IOptions<ExcelParserOptions> options)
    {
        _options = options.Value;
    }

    public Task<ParsedBudgetData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ParsedBudgetData();

        try
        {
            using var workbook = new XLWorkbook(fileStream);
            
            // Get the worksheet
            var worksheet = !string.IsNullOrEmpty(_options.SheetName) && workbook.Worksheets.Contains(_options.SheetName)
                ? workbook.Worksheet(_options.SheetName)
                : workbook.Worksheet(_options.SheetIndex);

            // Parse header
            result.Header = ParseHeader(worksheet, result.Errors);

            // Parse detail rows
            result.Items = ParseDetails(worksheet, result.Errors);

            // Additional validation
            ValidateData(result);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ValidationErrorDto(
                null,
                "File",
                $"Failed to parse Excel file: {ex.Message}",
                "Error"));
        }

        return Task.FromResult(result);
    }

    private ParsedHeaderDto ParseHeader(IXLWorksheet worksheet, List<ValidationErrorDto> errors)
    {
        var header = new ParsedHeaderDto();
        var extras = new Dictionary<string, object?>();

        foreach (var mapping in _options.Header.CellMappings)
        {
            var cellValue = GetCellValue(worksheet, mapping.Value);

            switch (mapping.Key)
            {
                case "Title":
                    header = header with { Title = cellValue?.ToString() };
                    break;
                case "RequestNumber":
                    header = header with { RequestNumber = cellValue?.ToString() };
                    break;
                case "Description":
                    header = header with { Description = cellValue?.ToString() };
                    break;
                case "Channel":
                    header = header with { Channel = cellValue?.ToString() };
                    break;
                case "Owner":
                    header = header with { Owner = cellValue?.ToString() };
                    break;
                case "Frequency":
                    header = header with { Frequency = cellValue?.ToString() };
                    break;
                case "Vendor":
                    header = header with { Vendor = cellValue?.ToString() };
                    break;
                case "Currency":
                    header = header with { Currency = cellValue?.ToString() ?? "USD" };
                    break;
                case "FiscalYear":
                    if (int.TryParse(cellValue?.ToString(), out var year))
                        header = header with { FiscalYear = year };
                    break;
                case "FiscalQuarter":
                    if (int.TryParse(cellValue?.ToString(), out var quarter))
                        header = header with { FiscalQuarter = quarter };
                    break;
                case "TotalAmount":
                    if (decimal.TryParse(cellValue?.ToString(), out var total))
                        header = header with { TotalAmount = total };
                    break;
                default:
                    // Store as extra field
                    extras[mapping.Key] = cellValue;
                    break;
            }
        }

        if (extras.Count > 0)
        {
            header = header with { Extras = extras };
        }

        // Validate required header fields
        if (string.IsNullOrWhiteSpace(header.Title))
        {
            errors.Add(new ValidationErrorDto(null, "Title", "Title is required", "Error"));
        }

        return header;
    }

    private List<ParsedItemDto> ParseDetails(IXLWorksheet worksheet, List<ValidationErrorDto> errors)
    {
        var items = new List<ParsedItemDto>();
        var startRow = _options.Detail.StartRow;
        var endRow = _options.Detail.EndRow > 0 ? _options.Detail.EndRow : 10000; // Safety limit
        var rowNumber = 0;

        for (var row = startRow; row <= endRow; row++)
        {
            // Check if row is empty (using first mapped column)
            var firstColumn = _options.Detail.ColumnMappings.Values.FirstOrDefault() ?? "A";
            var firstCell = worksheet.Cell(row, firstColumn);
            
            if (firstCell.IsEmpty() || string.IsNullOrWhiteSpace(firstCell.GetString()))
            {
                // If we hit an empty row, stop parsing
                break;
            }

            rowNumber++;
            var itemErrors = new List<ValidationErrorDto>();
            var extras = new Dictionary<string, object?>();

            var item = new ParsedItemDto { RowNumber = rowNumber };

            foreach (var mapping in _options.Detail.ColumnMappings)
            {
                var cellValue = GetCellValue(worksheet, $"{mapping.Value}{row}");

                switch (mapping.Key)
                {
                    case "LineDescription":
                        item = item with { LineDescription = cellValue?.ToString() };
                        break;
                    case "Category":
                        item = item with { Category = cellValue?.ToString() };
                        break;
                    case "SubCategory":
                        item = item with { SubCategory = cellValue?.ToString() };
                        break;
                    case "CostCenter":
                        item = item with { CostCenter = cellValue?.ToString() };
                        break;
                    case "AccountCode":
                        item = item with { AccountCode = cellValue?.ToString() };
                        break;
                    case "Amount":
                        item = item with { Amount = ParseDecimal(cellValue, rowNumber, "Amount", itemErrors) };
                        break;
                    case "Quantity":
                        item = item with { Quantity = ParseNullableDecimal(cellValue) };
                        break;
                    case "UnitPrice":
                        item = item with { UnitPrice = ParseNullableDecimal(cellValue) };
                        break;
                    case "Jan":
                        item = item with { Jan = ParseNullableDecimal(cellValue) };
                        break;
                    case "Feb":
                        item = item with { Feb = ParseNullableDecimal(cellValue) };
                        break;
                    case "Mar":
                        item = item with { Mar = ParseNullableDecimal(cellValue) };
                        break;
                    case "Apr":
                        item = item with { Apr = ParseNullableDecimal(cellValue) };
                        break;
                    case "May":
                        item = item with { May = ParseNullableDecimal(cellValue) };
                        break;
                    case "Jun":
                        item = item with { Jun = ParseNullableDecimal(cellValue) };
                        break;
                    case "Jul":
                        item = item with { Jul = ParseNullableDecimal(cellValue) };
                        break;
                    case "Aug":
                        item = item with { Aug = ParseNullableDecimal(cellValue) };
                        break;
                    case "Sep":
                        item = item with { Sep = ParseNullableDecimal(cellValue) };
                        break;
                    case "Oct":
                        item = item with { Oct = ParseNullableDecimal(cellValue) };
                        break;
                    case "Nov":
                        item = item with { Nov = ParseNullableDecimal(cellValue) };
                        break;
                    case "Dec":
                        item = item with { Dec = ParseNullableDecimal(cellValue) };
                        break;
                    default:
                        extras[mapping.Key] = cellValue;
                        break;
                }
            }

            if (extras.Count > 0)
            {
                item = item with { Extras = extras };
            }

            item = item with { HasErrors = itemErrors.Count > 0 };
            errors.AddRange(itemErrors);
            items.Add(item);
        }

        return items;
    }

    private void ValidateData(ParsedBudgetData data)
    {
        // Check if we have any items
        if (data.Items.Count == 0)
        {
            data.Errors.Add(new ValidationErrorDto(null, "Items", "No line items found in the file", "Warning"));
        }

        // Validate totals match if header has a total
        if (data.Header?.TotalAmount.HasValue == true)
        {
            var calculatedTotal = data.Items.Sum(i => i.Amount ?? 0);
            if (Math.Abs(calculatedTotal - data.Header.TotalAmount.Value) > 0.01m)
            {
                data.Errors.Add(new ValidationErrorDto(
                    null,
                    "TotalAmount",
                    $"Header total ({data.Header.TotalAmount:N2}) doesn't match sum of items ({calculatedTotal:N2})",
                    "Warning"));
            }
        }
    }

    private static object? GetCellValue(IXLWorksheet worksheet, string cellAddress)
    {
        var cell = worksheet.Cell(cellAddress);
        
        if (cell.IsEmpty())
            return null;

        return cell.DataType switch
        {
            XLDataType.Number => cell.GetDouble(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.Boolean => cell.GetBoolean(),
            _ => cell.GetString()
        };
    }

    private static decimal? ParseDecimal(object? value, int rowNumber, string field, List<ValidationErrorDto> errors)
    {
        if (value == null)
            return null;

        if (value is double d)
            return (decimal)d;

        if (decimal.TryParse(value.ToString(), out var result))
            return result;

        errors.Add(new ValidationErrorDto(rowNumber, field, $"Invalid number: {value}", "Error"));
        return null;
    }

    private static decimal? ParseNullableDecimal(object? value)
    {
        if (value == null)
            return null;

        if (value is double d)
            return (decimal)d;

        return decimal.TryParse(value.ToString(), out var result) ? result : null;
    }
}

