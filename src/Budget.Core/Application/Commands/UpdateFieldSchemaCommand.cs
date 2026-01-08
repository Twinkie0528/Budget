using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Commands;

/// <summary>
/// Command to update field schemas (bulk upsert).
/// </summary>
public record UpdateFieldSchemaCommand(
    IReadOnlyList<FieldSchemaUpdateDto> Schemas
) : IRequest<IReadOnlyList<FieldSchemaDto>>;

