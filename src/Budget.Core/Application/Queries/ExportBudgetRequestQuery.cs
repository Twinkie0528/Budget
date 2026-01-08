using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to export a budget request as Excel.
/// </summary>
public record ExportBudgetRequestQuery(Guid Id) : IRequest<ExportResult?>;

/// <summary>
/// Export result containing file bytes and metadata.
/// </summary>
public record ExportResult(
    byte[] FileBytes,
    string FileName,
    string ContentType
);

