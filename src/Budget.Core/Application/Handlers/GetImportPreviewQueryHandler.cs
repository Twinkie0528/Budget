using System.Text.Json;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class GetImportPreviewQueryHandler : IRequestHandler<GetImportPreviewQuery, ImportPreviewDto>
{
    private readonly IImportRunRepository _importRunRepository;

    public GetImportPreviewQueryHandler(IImportRunRepository importRunRepository)
    {
        _importRunRepository = importRunRepository;
    }

    public async Task<ImportPreviewDto> Handle(GetImportPreviewQuery request, CancellationToken cancellationToken)
    {
        var importRun = await _importRunRepository.GetByIdAsync(request.ImportRunId, cancellationToken)
            ?? throw new InvalidOperationException($"Import run {request.ImportRunId} not found");

        var header = !string.IsNullOrEmpty(importRun.ParsedHeaderJson)
            ? JsonSerializer.Deserialize<ParsedHeaderDto>(importRun.ParsedHeaderJson)
            : null;

        var items = !string.IsNullOrEmpty(importRun.ParsedItemsJson)
            ? JsonSerializer.Deserialize<List<ParsedItemDto>>(importRun.ParsedItemsJson) ?? new List<ParsedItemDto>()
            : new List<ParsedItemDto>();

        var errors = !string.IsNullOrEmpty(importRun.ValidationErrorsJson)
            ? JsonSerializer.Deserialize<List<ValidationErrorDto>>(importRun.ValidationErrorsJson) ?? new List<ValidationErrorDto>()
            : new List<ValidationErrorDto>();

        var canCommit = importRun.Status == Domain.Entities.ImportStatus.Parsed
            && !errors.Any(e => e.Severity == "Error");

        return new ImportPreviewDto(
            importRun.Id,
            importRun.FileName,
            importRun.Status,
            header,
            items,
            errors,
            canCommit);
    }
}

