using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Queries;
using Budget.Core.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Field Schema

    /// <summary>
    /// Get all field schemas.
    /// </summary>
    [HttpGet("field-schema")]
    [ProducesResponseType(typeof(IReadOnlyList<FieldSchemaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FieldSchemaDto>>> GetFieldSchemas(
        [FromQuery] FieldAppliesTo? appliesTo = null,
        CancellationToken ct = default)
    {
        var query = new GetFieldSchemasQuery(appliesTo);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update field schemas (bulk upsert).
    /// </summary>
    [HttpPut("field-schema")]
    [ProducesResponseType(typeof(IReadOnlyList<FieldSchemaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<FieldSchemaDto>>> UpdateFieldSchemas(
        [FromBody] List<FieldSchemaUpdateDto> schemas,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating {Count} field schemas", schemas.Count);

        var command = new UpdateFieldSchemaCommand(schemas);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    #endregion

    #region Dimensions

    /// <summary>
    /// Get all available dimension keys.
    /// </summary>
    [HttpGet("dimensions")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetDimensionKeys(CancellationToken ct)
    {
        var query = new GetDimensionKeysQuery();
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Get dimension values by enum key.
    /// </summary>
    [HttpGet("dimensions/{enumKey}")]
    [ProducesResponseType(typeof(DimensionListDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DimensionListDto>> GetDimension(
        string enumKey,
        [FromQuery] bool activeOnly = true,
        CancellationToken ct = default)
    {
        var query = new GetDimensionQuery(enumKey, activeOnly);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Update dimension values for an enum key.
    /// </summary>
    [HttpPut("dimensions/{enumKey}")]
    [ProducesResponseType(typeof(DimensionListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DimensionListDto>> UpdateDimension(
        string enumKey,
        [FromBody] List<DimensionValueUpdateDto> values,
        CancellationToken ct)
    {
        _logger.LogInformation("Updating dimension {EnumKey} with {Count} values", enumKey, values.Count);

        var command = new UpdateDimensionCommand(enumKey, values);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    #endregion
}

