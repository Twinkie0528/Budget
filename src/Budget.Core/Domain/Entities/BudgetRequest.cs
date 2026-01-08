namespace Budget.Core.Domain.Entities;

/// <summary>
/// Header-level budget request entity.
/// </summary>
public class BudgetRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Standard dimension references
    public string? Channel { get; set; }
    public string? Owner { get; set; }
    public string? Frequency { get; set; }
    public string? Vendor { get; set; }
    
    // Financial summary
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Status
    public BudgetRequestStatus Status { get; set; } = BudgetRequestStatus.Draft;
    
    // Period
    public int FiscalYear { get; set; }
    public int? FiscalQuarter { get; set; }
    public int? FiscalMonth { get; set; }
    
    // Dynamic fields stored as JSON
    public string? ExtrasJson { get; set; }
    
    // Navigation
    public Guid? ImportRunId { get; set; }
    public ImportRun? ImportRun { get; set; }
    public ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();
    public ICollection<BudgetSection> Sections { get; set; } = new List<BudgetSection>();
}

public enum BudgetRequestStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Archived = 4
}

