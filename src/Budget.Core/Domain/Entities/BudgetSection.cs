namespace Budget.Core.Domain.Entities;

/// <summary>
/// Grouping/section within a budget request (e.g., channel groups).
/// </summary>
public class BudgetSection : BaseEntity
{
    public Guid BudgetRequestId { get; set; }
    public BudgetRequest BudgetRequest { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int SortOrder { get; set; }
    
    // Optional hierarchy
    public Guid? ParentSectionId { get; set; }
    public BudgetSection? ParentSection { get; set; }
    public ICollection<BudgetSection> ChildSections { get; set; } = new List<BudgetSection>();
    
    public ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();
    
    // Section-level totals (computed)
    public decimal SubTotal { get; set; }
}

