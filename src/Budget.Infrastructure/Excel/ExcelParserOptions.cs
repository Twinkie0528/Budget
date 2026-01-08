namespace Budget.Infrastructure.Excel;

/// <summary>
/// Configuration for Excel parsing positions.
/// </summary>
public class ExcelParserOptions
{
    public const string Section = "ExcelParser";
    
    /// <summary>
    /// Sheet name or index (1-based) containing the budget data.
    /// </summary>
    public string SheetName { get; set; } = "Budget";
    public int SheetIndex { get; set; } = 1;
    
    /// <summary>
    /// Header section configuration (key/value pairs in specific cells).
    /// </summary>
    public HeaderConfig Header { get; set; } = new();
    
    /// <summary>
    /// Detail rows configuration.
    /// </summary>
    public DetailConfig Detail { get; set; } = new();
}

public class HeaderConfig
{
    /// <summary>
    /// Cell mappings for header fields. Key = field name, Value = cell address (e.g., "B2").
    /// </summary>
    public Dictionary<string, string> CellMappings { get; set; } = new()
    {
        ["Title"] = "B2",
        ["RequestNumber"] = "B3",
        ["Description"] = "B4",
        ["Channel"] = "B5",
        ["Owner"] = "B6",
        ["Frequency"] = "B7",
        ["Vendor"] = "B8",
        ["FiscalYear"] = "B9",
        ["FiscalQuarter"] = "B10",
        ["Currency"] = "B11"
    };
}

public class DetailConfig
{
    /// <summary>
    /// Row number where detail data starts (1-based).
    /// </summary>
    public int StartRow { get; set; } = 14;
    
    /// <summary>
    /// Row number where detail data ends (0 = read until empty row).
    /// </summary>
    public int EndRow { get; set; } = 0;
    
    /// <summary>
    /// Column mappings for detail fields. Key = field name, Value = column letter.
    /// </summary>
    public Dictionary<string, string> ColumnMappings { get; set; } = new()
    {
        ["LineDescription"] = "A",
        ["Category"] = "B",
        ["SubCategory"] = "C",
        ["Quantity"] = "D",
        ["UnitPrice"] = "E",
        ["Amount"] = "F",
        ["CostCenter"] = "G",
        ["AccountCode"] = "H",
        ["Jan"] = "I",
        ["Feb"] = "J",
        ["Mar"] = "K",
        ["Apr"] = "L",
        ["May"] = "M",
        ["Jun"] = "N",
        ["Jul"] = "O",
        ["Aug"] = "P",
        ["Sep"] = "Q",
        ["Oct"] = "R",
        ["Nov"] = "S",
        ["Dec"] = "T"
    };
}

