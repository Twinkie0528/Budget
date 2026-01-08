namespace Budget.Core.Domain.Entities;

/// <summary>
/// Dynamic field definition for extensible headers/items.
/// </summary>
public class FieldSchema : BaseEntity
{
    public string FieldKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType FieldType { get; set; } = FieldType.Text;
    public bool IsRequired { get; set; }
    public FieldAppliesTo AppliesTo { get; set; } = FieldAppliesTo.Header;
    
    // For dropdown fields, reference to DimensionValue.EnumKey
    public string? EnumKey { get; set; }
    
    // UI hints
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationPattern { get; set; }
    public int? MaxLength { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public enum FieldType
{
    Text = 0,
    Number = 1,
    Decimal = 2,
    Date = 3,
    DateTime = 4,
    Boolean = 5,
    Dropdown = 6,
    MultiSelect = 7,
    TextArea = 8
}

public enum FieldAppliesTo
{
    Header = 0,
    Item = 1,
    Both = 2
}

