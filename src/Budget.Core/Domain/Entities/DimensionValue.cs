namespace Budget.Core.Domain.Entities;

/// <summary>
/// Lookup value for dimension dropdowns (channel, owner, frequency, vendor, etc.).
/// </summary>
public class DimensionValue : BaseEntity
{
    /// <summary>
    /// Identifies the dimension type (e.g., "channel", "owner", "frequency", "vendor").
    /// </summary>
    public string EnumKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique code within the enum (e.g., "DIGITAL", "RETAIL").
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Display order.
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Whether this value is available for selection.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional parent for hierarchical dimensions.
    /// </summary>
    public Guid? ParentId { get; set; }
    public DimensionValue? Parent { get; set; }
    public ICollection<DimensionValue> Children { get; set; } = new List<DimensionValue>();
}

