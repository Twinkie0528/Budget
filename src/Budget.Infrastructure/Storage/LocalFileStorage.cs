using Budget.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace Budget.Infrastructure.Storage;

/// <summary>
/// Local file system storage for development.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _basePath = options.Value.LocalPath ?? "./data/storage";
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken ct = default)
    {
        // Generate unique path: year/month/guid_filename
        var now = DateTime.UtcNow;
        var relativePath = Path.Combine(
            now.Year.ToString(),
            now.Month.ToString("D2"),
            $"{Guid.NewGuid():N}_{SanitizeFileName(fileName)}");

        var fullPath = Path.Combine(_basePath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, ct);

        return relativePath;
    }

    public Task<Stream?> GetAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}

public class FileStorageOptions
{
    public const string Section = "FileStorage";
    public string? LocalPath { get; set; }
    public string? StorageType { get; set; } = "Local"; // Local, SharePoint
}

