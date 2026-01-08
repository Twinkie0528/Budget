using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to get import preview with parsed data and validation errors.
/// </summary>
public record GetImportPreviewQuery(Guid ImportRunId) : IRequest<ImportPreviewDto>;

