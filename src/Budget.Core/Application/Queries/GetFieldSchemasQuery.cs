using Budget.Core.Application.Dtos;
using Budget.Core.Domain.Entities;
using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to get all field schemas.
/// </summary>
public record GetFieldSchemasQuery(FieldAppliesTo? AppliesTo = null) : IRequest<IReadOnlyList<FieldSchemaDto>>;

