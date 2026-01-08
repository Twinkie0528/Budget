using Budget.Core.Domain.Entities;

namespace Budget.Core.Application.Dtos;

/// <summary>
/// Result of file upload.
/// </summary>
public record UploadResultDto(
    Guid ImportRunId,
    string FileName,
    long FileSizeBytes,
    ImportStatus Status
);

/// <summary>
/// Preview of parsed budget data.
/// </summary>
public record ImportPreviewDto(
    Guid ImportRunId,
    string FileName,
    ImportStatus Status,
    ParsedHeaderDto? Header,
    IReadOnlyList<ParsedItemDto> Items,
    IReadOnlyList<ValidationErrorDto> Errors,
    bool CanCommit
);

/// <summary>
/// Parsed header fields from Excel.
/// </summary>
public record ParsedHeaderDto
{
    public string? RequestNumber { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Channel { get; init; }
    public string? Owner { get; init; }
    public string? Frequency { get; init; }
    public string? Vendor { get; init; }
    public decimal? TotalAmount { get; init; }
    public string? Currency { get; init; }
    public int? FiscalYear { get; init; }
    public int? FiscalQuarter { get; init; }
    public Dictionary<string, object?>? Extras { get; init; }
}

/// <summary>
/// Parsed line item from Excel.
/// </summary>
public record ParsedItemDto
{
    public int RowNumber { get; init; }
    public string? LineDescription { get; init; }
    public string? Category { get; init; }
    public string? SubCategory { get; init; }
    public decimal? Amount { get; init; }
    public decimal? Quantity { get; init; }
    public decimal? UnitPrice { get; init; }
    public string? CostCenter { get; init; }
    public string? AccountCode { get; init; }
    public decimal? Jan { get; init; }
    public decimal? Feb { get; init; }
    public decimal? Mar { get; init; }
    public decimal? Apr { get; init; }
    public decimal? May { get; init; }
    public decimal? Jun { get; init; }
    public decimal? Jul { get; init; }
    public decimal? Aug { get; init; }
    public decimal? Sep { get; init; }
    public decimal? Oct { get; init; }
    public decimal? Nov { get; init; }
    public decimal? Dec { get; init; }
    public Dictionary<string, object?>? Extras { get; init; }
    public bool HasErrors { get; init; }
}

/// <summary>
/// Validation error detail.
/// </summary>
public record ValidationErrorDto(
    int? RowNumber,
    string Field,
    string Message,
    string Severity // "Error", "Warning"
);

/// <summary>
/// Complete parsed data from Excel.
/// </summary>
public class ParsedBudgetData
{
    public ParsedHeaderDto? Header { get; set; }
    public List<ParsedItemDto> Items { get; set; } = new();
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public bool IsValid => Errors.All(e => e.Severity != "Error");
}

/// <summary>
/// Result of commit operation.
/// </summary>
public record CommitResultDto(
    Guid ImportRunId,
    Guid BudgetRequestId,
    string RequestNumber,
    int ItemCount
);

