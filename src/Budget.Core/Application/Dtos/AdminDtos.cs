using Budget.Core.Domain.Entities;

namespace Budget.Core.Application.Dtos;

/// <summary>
/// Field schema DTO.
/// </summary>
public record FieldSchemaDto(
    Guid Id,
    string FieldKey,
    string Label,
    FieldType FieldType,
    bool IsRequired,
    FieldAppliesTo AppliesTo,
    string? EnumKey,
    int SortOrder,
    string? Description,
    string? DefaultValue,
    string? ValidationPattern,
    int? MaxLength,
    bool IsActive
);

/// <summary>
/// Create/update field schema request.
/// </summary>
public record FieldSchemaUpdateDto
{
    public Guid? Id { get; init; }
    public string FieldKey { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public FieldType FieldType { get; init; }
    public bool IsRequired { get; init; }
    public FieldAppliesTo AppliesTo { get; init; }
    public string? EnumKey { get; init; }
    public int SortOrder { get; init; }
    public string? Description { get; init; }
    public string? DefaultValue { get; init; }
    public string? ValidationPattern { get; init; }
    public int? MaxLength { get; init; }
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Dimension value DTO.
/// </summary>
public record DimensionValueDto(
    Guid Id,
    string EnumKey,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive,
    Guid? ParentId
);

/// <summary>
/// Create/update dimension value request.
/// </summary>
public record DimensionValueUpdateDto
{
    public Guid? Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public Guid? ParentId { get; init; }
}

/// <summary>
/// List of dimension values for an enum key.
/// </summary>
public record DimensionListDto(
    string EnumKey,
    IReadOnlyList<DimensionValueDto> Values
);

/// <summary>
/// Audit log DTO.
/// </summary>
public record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    string UserId,
    string? UserName,
    DateTime Timestamp,
    object? Payload
);

