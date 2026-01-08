using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Queries;

/// <summary>
/// Query to get a single budget request with all details.
/// </summary>
public record GetBudgetRequestDetailQuery(Guid Id) : IRequest<BudgetRequestDetailDto?>;

