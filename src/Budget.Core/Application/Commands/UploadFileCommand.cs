using Budget.Core.Application.Dtos;
using MediatR;

namespace Budget.Core.Application.Commands;

/// <summary>
/// Command to upload an Excel file for import.
/// </summary>
public record UploadFileCommand(
    Stream FileStream,
    string FileName,
    long FileSizeBytes,
    string? ContentType
) : IRequest<UploadResultDto>;

