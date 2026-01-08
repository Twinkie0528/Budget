using System.Text.Json;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class GetBudgetRequestDetailQueryHandler : IRequestHandler<GetBudgetRequestDetailQuery, BudgetRequestDetailDto?>
{
    private readonly IBudgetRequestRepository _repository;

    public GetBudgetRequestDetailQueryHandler(IBudgetRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<BudgetRequestDetailDto?> Handle(
        GetBudgetRequestDetailQuery request,
        CancellationToken cancellationToken)
    {
        var budgetRequest = await _repository.GetWithItemsAsync(request.Id, cancellationToken);

        if (budgetRequest == null)
            return null;

        var extras = !string.IsNullOrEmpty(budgetRequest.ExtrasJson)
            ? JsonSerializer.Deserialize<Dictionary<string, object?>>(budgetRequest.ExtrasJson)
            : null;

        var items = budgetRequest.Items.Select(i =>
        {
            var itemExtras = !string.IsNullOrEmpty(i.ExtrasJson)
                ? JsonSerializer.Deserialize<Dictionary<string, object?>>(i.ExtrasJson)
                : null;

            return new BudgetItemDto
            {
                Id = i.Id,
                RowNumber = i.RowNumber,
                SectionId = i.SectionId,
                LineDescription = i.LineDescription,
                Category = i.Category,
                SubCategory = i.SubCategory,
                Amount = i.Amount,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                CostCenter = i.CostCenter,
                AccountCode = i.AccountCode,
                Jan = i.Jan,
                Feb = i.Feb,
                Mar = i.Mar,
                Apr = i.Apr,
                May = i.May,
                Jun = i.Jun,
                Jul = i.Jul,
                Aug = i.Aug,
                Sep = i.Sep,
                Oct = i.Oct,
                Nov = i.Nov,
                Dec = i.Dec,
                Extras = itemExtras
            };
        }).OrderBy(i => i.RowNumber).ToList();

        var sections = budgetRequest.Sections.Select(s => new BudgetSectionDto(
            s.Id,
            s.Name,
            s.Code,
            s.SortOrder,
            s.SubTotal,
            s.ParentSectionId
        )).OrderBy(s => s.SortOrder).ToList();

        return new BudgetRequestDetailDto
        {
            Id = budgetRequest.Id,
            RequestNumber = budgetRequest.RequestNumber,
            Title = budgetRequest.Title,
            Description = budgetRequest.Description,
            Channel = budgetRequest.Channel,
            Owner = budgetRequest.Owner,
            Frequency = budgetRequest.Frequency,
            Vendor = budgetRequest.Vendor,
            TotalAmount = budgetRequest.TotalAmount,
            Currency = budgetRequest.Currency,
            Status = budgetRequest.Status,
            FiscalYear = budgetRequest.FiscalYear,
            FiscalQuarter = budgetRequest.FiscalQuarter,
            FiscalMonth = budgetRequest.FiscalMonth,
            Extras = extras,
            CreatedAt = budgetRequest.CreatedAt,
            CreatedBy = budgetRequest.CreatedBy,
            UpdatedAt = budgetRequest.UpdatedAt,
            UpdatedBy = budgetRequest.UpdatedBy,
            ImportRunId = budgetRequest.ImportRunId,
            Items = items,
            Sections = sections
        };
    }
}

