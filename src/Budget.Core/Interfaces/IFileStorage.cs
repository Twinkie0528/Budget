namespace Budget.Core.Interfaces;

/// <summary>
/// File storage abstraction for uploaded files.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Stores a file and returns the storage path.
    /// </summary>
    Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken ct = default);
    
    /// <summary>
    /// Retrieves a file stream by storage path.
    /// </summary>
    Task<Stream?> GetAsync(string storagePath, CancellationToken ct = default);
    
    /// <summary>
    /// Deletes a file by storage path.
    /// </summary>
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
    
    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default);
}

