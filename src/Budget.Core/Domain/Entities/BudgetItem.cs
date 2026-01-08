namespace Budget.Core.Domain.Entities;

/// <summary>
/// Line-item detail within a budget request.
/// </summary>
public class BudgetItem : BaseEntity
{
    public Guid BudgetRequestId { get; set; }
    public BudgetRequest BudgetRequest { get; set; } = null!;
    
    public Guid? SectionId { get; set; }
    public BudgetSection? Section { get; set; }
    
    public int RowNumber { get; set; }
    public string? LineDescription { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    
    // Financial
    public decimal Amount { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? CostCenter { get; set; }
    public string? AccountCode { get; set; }
    
    // Period allocation (optional monthly breakdown)
    public decimal? Jan { get; set; }
    public decimal? Feb { get; set; }
    public decimal? Mar { get; set; }
    public decimal? Apr { get; set; }
    public decimal? May { get; set; }
    public decimal? Jun { get; set; }
    public decimal? Jul { get; set; }
    public decimal? Aug { get; set; }
    public decimal? Sep { get; set; }
    public decimal? Oct { get; set; }
    public decimal? Nov { get; set; }
    public decimal? Dec { get; set; }
    
    // Dynamic fields stored as JSON
    public string? ExtrasJson { get; set; }
}

