using System.Linq.Expressions;
using Budget.Core.Domain.Entities;

namespace Budget.Core.Interfaces;

/// <summary>
/// Generic repository interface.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}

/// <summary>
/// Budget request repository with specific queries.
/// </summary>
public interface IBudgetRequestRepository : IRepository<BudgetRequest>
{
    Task<BudgetRequest?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<BudgetRequest> Items, int TotalCount)> GetPagedAsync(
        BudgetRequestFilter filter,
        int page,
        int pageSize,
        CancellationToken ct = default);
}

/// <summary>
/// Import run repository.
/// </summary>
public interface IImportRunRepository : IRepository<ImportRun>
{
    Task<ImportRun?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Audit log repository (not BaseEntity).
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}

/// <summary>
/// Field schema repository.
/// </summary>
public interface IFieldSchemaRepository : IRepository<FieldSchema>
{
    Task<IReadOnlyList<FieldSchema>> GetActiveAsync(FieldAppliesTo? appliesTo = null, CancellationToken ct = default);
}

/// <summary>
/// Dimension value repository.
/// </summary>
public interface IDimensionValueRepository : IRepository<DimensionValue>
{
    Task<IReadOnlyList<DimensionValue>> GetByEnumKeyAsync(string enumKey, bool activeOnly = true, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetEnumKeysAsync(CancellationToken ct = default);
}

/// <summary>
/// Filter criteria for budget requests.
/// </summary>
public class BudgetRequestFilter
{
    public string? SearchTerm { get; set; }
    public BudgetRequestStatus? Status { get; set; }
    public string? Channel { get; set; }
    public string? Owner { get; set; }
    public int? FiscalYear { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

