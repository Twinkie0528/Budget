using Budget.Core.Application.Dtos;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to get paged list of budget requests.
/// </summary>
public record GetBudgetRequestsQuery : IRequest<PagedResultDto<BudgetRequestListDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public BudgetRequestStatus? Status { get; init; }
    public string? Channel { get; init; }
    public string? Owner { get; init; }
    public int? FiscalYear { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;

    public BudgetRequestFilter ToFilter() => new()
    {
        SearchTerm = SearchTerm,
        Status = Status,
        Channel = Channel,
        Owner = Owner,
        FiscalYear = FiscalYear,
        FromDate = FromDate,
        ToDate = ToDate,
        SortBy = SortBy,
        SortDescending = SortDescending
    };
}

