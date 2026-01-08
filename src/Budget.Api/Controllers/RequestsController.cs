using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IMediator mediator, ILogger<RequestsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paged list of budget requests with filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<BudgetRequestListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<BudgetRequestListDto>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] BudgetRequestStatus? status = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? owner = null,
        [FromQuery] int? fiscalYear = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        var query = new GetBudgetRequestsQuery
        {
            Page = page,
            PageSize = Math.Clamp(pageSize, 1, 100),
            SearchTerm = search,
            Status = status,
            Channel = channel,
            Owner = owner,
            FiscalYear = fiscalYear,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get budget request details.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BudgetRequestDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetRequestDetailDto>> GetDetail(Guid id, CancellationToken ct)
    {
        var query = new GetBudgetRequestDetailQuery(id);
        var result = await _mediator.Send(query, ct);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Budget request {id} not found"
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Export budget request as Excel file.
    /// </summary>
    [HttpGet("{id:guid}/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Exporting budget request: {RequestId}", id);

        var query = new ExportBudgetRequestQuery(id);
        var result = await _mediator.Send(query, ct);

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Budget request {id} not found"
            });
        }

        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}

