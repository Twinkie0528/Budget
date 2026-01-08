using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[ApiController]
[Route("api/imports")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportsController> _logger;

    public ImportsController(IMediator mediator, ILogger<ImportsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Upload an Excel file for import.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadResultDto>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "No file provided"
            });
        }

        _logger.LogInformation("Uploading file: {FileName}, Size: {Size}", file.FileName, file.Length);

        await using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            stream,
            file.FileName,
            file.Length,
            file.ContentType);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get preview of parsed import data.
    /// </summary>
    [HttpGet("{id:guid}/preview")]
    [ProducesResponseType(typeof(ImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPreviewDto>> GetPreview(Guid id, CancellationToken ct)
    {
        var query = new GetImportPreviewQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Commit a parsed import to create a budget request.
    /// </summary>
    [HttpPost("{id:guid}/commit")]
    [ProducesResponseType(typeof(CommitResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommitResultDto>> Commit(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Committing import: {ImportRunId}", id);

        var command = new CommitImportCommand(id);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

