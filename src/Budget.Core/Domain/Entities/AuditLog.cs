namespace Budget.Core.Domain.Entities;

/// <summary>
/// Audit trail entry for tracking changes.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// JSON payload with before/after or relevant context.
    /// </summary>
    public string? PayloadJson { get; set; }
    
    /// <summary>
    /// IP address or client info.
    /// </summary>
    public string? ClientInfo { get; set; }
}

