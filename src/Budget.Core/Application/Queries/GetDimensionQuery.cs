using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to get dimension values by enum key.
/// </summary>
public record GetDimensionQuery(string EnumKey, bool ActiveOnly = true) : IRequest<DimensionListDto>;

/// <summary>
/// Query to get all available dimension enum keys.
/// </summary>
public record GetDimensionKeysQuery : IRequest<IReadOnlyList<string>>;

