using Budget.Core.Application.Dtos;

namespace Budget.Core.Interfaces;

/// <summary>
/// Excel file parsing service.
/// </summary>
public interface IExcelParser
{
    /// <summary>
    /// Parses an Excel file and returns preview data with validation.
    /// </summary>
    Task<ParsedBudgetData> ParseAsync(Stream fileStream, CancellationToken ct = default);
}

/// <summary>
/// Excel export service.
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// Generates an Excel export for a budget request.
    /// </summary>
    Task<byte[]> ExportAsync(BudgetRequestDetailDto request, CancellationToken ct = default);
}

