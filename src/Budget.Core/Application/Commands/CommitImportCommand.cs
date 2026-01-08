using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Commands;

/// <summary>
/// Command to commit a parsed import to the database.
/// </summary>
public record CommitImportCommand(Guid ImportRunId) : IRequest<CommitResultDto>;

