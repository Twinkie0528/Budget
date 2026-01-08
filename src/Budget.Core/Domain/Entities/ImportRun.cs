namespace Budget.Core.Domain.Entities;

/// <summary>
/// Tracks an import operation from file upload to commit.
/// </summary>
public class ImportRun : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ContentType { get; set; }
    
    public ImportStatus Status { get; set; } = ImportStatus.Uploaded;
    public string? ErrorMessage { get; set; }
    
    // Parsing results (stored as JSON for preview)
    public string? ParsedHeaderJson { get; set; }
    public string? ParsedItemsJson { get; set; }
    public string? ValidationErrorsJson { get; set; }
    public int? ParsedRowCount { get; set; }
    public int? ErrorCount { get; set; }
    
    // Processing timestamps
    public DateTime? ParsedAt { get; set; }
    public DateTime? CommittedAt { get; set; }
    
    // Result
    public Guid? BudgetRequestId { get; set; }
    public BudgetRequest? BudgetRequest { get; set; }
}

public enum ImportStatus
{
    Uploaded = 0,
    Parsing = 1,
    Parsed = 2,
    ParseFailed = 3,
    Committing = 4,
    Committed = 5,
    CommitFailed = 6
}

