using Budget.Core.Interfaces;

namespace Budget.Infrastructure.Storage;

/// <summary>
/// SharePoint storage via Microsoft Graph API.
/// TODO: Implement when SharePoint integration is required.
/// </summary>
public class SharePointGraphStorage : IFileStorage
{
    // TODO: Inject Microsoft Graph client
    // private readonly GraphServiceClient _graphClient;
    // private readonly string _siteId;
    // private readonly string _driveId;

    public SharePointGraphStorage()
    {
        // TODO: Initialize with Graph client and configuration
    }

    public Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken ct = default)
    {
        // TODO: Implement SharePoint upload
        // var uploadSession = await _graphClient.Sites[_siteId].Drives[_driveId]
        //     .Root.ItemWithPath(path).CreateUploadSession().Request().PostAsync();
        throw new NotImplementedException("SharePoint storage not yet implemented. Configure LocalPath for development.");
    }

    public Task<Stream?> GetAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Implement SharePoint download
        // var stream = await _graphClient.Sites[_siteId].Drives[_driveId]
        //     .Root.ItemWithPath(storagePath).Content.Request().GetAsync();
        throw new NotImplementedException("SharePoint storage not yet implemented. Configure LocalPath for development.");
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Implement SharePoint delete
        // await _graphClient.Sites[_siteId].Drives[_driveId]
        //     .Root.ItemWithPath(storagePath).Request().DeleteAsync();
        throw new NotImplementedException("SharePoint storage not yet implemented. Configure LocalPath for development.");
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Implement SharePoint exists check
        throw new NotImplementedException("SharePoint storage not yet implemented. Configure LocalPath for development.");
    }
}

