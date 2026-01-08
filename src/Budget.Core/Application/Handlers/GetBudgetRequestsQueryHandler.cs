using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class GetBudgetRequestsQueryHandler : IRequestHandler<GetBudgetRequestsQuery, PagedResultDto<BudgetRequestListDto>>
{
    private readonly IBudgetRequestRepository _repository;

    public GetBudgetRequestsQueryHandler(IBudgetRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<BudgetRequestListDto>> Handle(
        GetBudgetRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            request.ToFilter(),
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = items.Select(r => new BudgetRequestListDto(
            r.Id,
            r.RequestNumber,
            r.Title,
            r.Channel,
            r.Owner,
            r.TotalAmount,
            r.Currency,
            r.Status,
            r.FiscalYear,
            r.FiscalQuarter,
            r.CreatedAt,
            r.CreatedBy,
            r.Items.Count
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResultDto<BudgetRequestListDto>(
            dtos,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}

