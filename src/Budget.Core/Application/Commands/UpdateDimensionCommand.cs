using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Commands;

/// <summary>
/// Command to update dimension values for an enum key.
/// </summary>
public record UpdateDimensionCommand(
    string EnumKey,
    IReadOnlyList<DimensionValueUpdateDto> Values
) : IRequest<DimensionListDto>;

