using Budget.Core.Domain.Entities;

namespace Budget.Core.Application.Dtos;

/// <summary>
/// Budget request list item DTO.
/// </summary>
public record BudgetRequestListDto(
    Guid Id,
    string RequestNumber,
    string Title,
    string? Channel,
    string? Owner,
    decimal TotalAmount,
    string Currency,
    BudgetRequestStatus Status,
    int FiscalYear,
    int? FiscalQuarter,
    DateTime CreatedAt,
    string CreatedBy,
    int ItemCount
);

/// <summary>
/// Budget request detail DTO.
/// </summary>
public record BudgetRequestDetailDto
{
    public Guid Id { get; init; }
    public string RequestNumber { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Channel { get; init; }
    public string? Owner { get; init; }
    public string? Frequency { get; init; }
    public string? Vendor { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public BudgetRequestStatus Status { get; init; }
    public int FiscalYear { get; init; }
    public int? FiscalQuarter { get; init; }
    public int? FiscalMonth { get; init; }
    public Dictionary<string, object?>? Extras { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public Guid? ImportRunId { get; init; }
    public IReadOnlyList<BudgetItemDto> Items { get; init; } = Array.Empty<BudgetItemDto>();
    public IReadOnlyList<BudgetSectionDto> Sections { get; init; } = Array.Empty<BudgetSectionDto>();
}

/// <summary>
/// Budget item DTO.
/// </summary>
public record BudgetItemDto
{
    public Guid Id { get; init; }
    public int RowNumber { get; init; }
    public Guid? SectionId { get; init; }
    public string? LineDescription { get; init; }
    public string? Category { get; init; }
    public string? SubCategory { get; init; }
    public decimal Amount { get; init; }
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
}

/// <summary>
/// Budget section DTO.
/// </summary>
public record BudgetSectionDto(
    Guid Id,
    string Name,
    string? Code,
    int SortOrder,
    decimal SubTotal,
    Guid? ParentSectionId
);

/// <summary>
/// Paged response wrapper.
/// </summary>
public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

