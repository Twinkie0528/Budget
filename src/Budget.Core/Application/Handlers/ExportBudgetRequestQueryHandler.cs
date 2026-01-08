using Budget.Core.Application.Queries;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class ExportBudgetRequestQueryHandler : IRequestHandler<ExportBudgetRequestQuery, ExportResult?>
{
    private readonly IBudgetRequestRepository _repository;
    private readonly IExcelExporter _excelExporter;

    public ExportBudgetRequestQueryHandler(
        IBudgetRequestRepository repository,
        IExcelExporter excelExporter)
    {
        _repository = repository;
        _excelExporter = excelExporter;
    }

    public async Task<ExportResult?> Handle(ExportBudgetRequestQuery request, CancellationToken cancellationToken)
    {
        var detailHandler = new GetBudgetRequestDetailQueryHandler(_repository);
        var detail = await detailHandler.Handle(new GetBudgetRequestDetailQuery(request.Id), cancellationToken);

        if (detail == null)
            return null;

        var fileBytes = await _excelExporter.ExportAsync(detail, cancellationToken);
        var fileName = $"BudgetRequest_{detail.RequestNumber}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        return new ExportResult(
            fileBytes,
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }
}

