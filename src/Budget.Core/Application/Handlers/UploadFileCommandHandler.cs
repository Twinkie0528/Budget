using System.Text.Json;
using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using MediatR;

namespace Budget.Core.Application.Handlers;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadResultDto>
{
    private readonly IFileStorage _fileStorage;
    private readonly IImportRunRepository _importRunRepository;
    private readonly IExcelParser _excelParser;
    private readonly ICurrentUser _currentUser;

    public UploadFileCommandHandler(
        IFileStorage fileStorage,
        IImportRunRepository importRunRepository,
        IExcelParser excelParser,
        ICurrentUser currentUser)
    {
        _fileStorage = fileStorage;
        _importRunRepository = importRunRepository;
        _excelParser = excelParser;
        _currentUser = currentUser;
    }

    public async Task<UploadResultDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        // Store the file
        var storagePath = await _fileStorage.SaveAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        // Create import run record
        var importRun = new ImportRun
        {
            FileName = request.FileName,
            StoragePath = storagePath,
            FileSizeBytes = request.FileSizeBytes,
            ContentType = request.ContentType,
            Status = ImportStatus.Uploaded,
            CreatedBy = _currentUser.UserId
        };

        await _importRunRepository.AddAsync(importRun, cancellationToken);

        // Parse the file immediately for preview
        try
        {
            importRun.Status = ImportStatus.Parsing;
            await _importRunRepository.UpdateAsync(importRun, cancellationToken);

            using var fileStream = await _fileStorage.GetAsync(storagePath, cancellationToken);
            if (fileStream == null)
            {
                throw new InvalidOperationException("Failed to read uploaded file");
            }

            var parsedData = await _excelParser.ParseAsync(fileStream, cancellationToken);

            importRun.ParsedHeaderJson = JsonSerializer.Serialize(parsedData.Header);
            importRun.ParsedItemsJson = JsonSerializer.Serialize(parsedData.Items);
            importRun.ValidationErrorsJson = JsonSerializer.Serialize(parsedData.Errors);
            importRun.ParsedRowCount = parsedData.Items.Count;
            importRun.ErrorCount = parsedData.Errors.Count(e => e.Severity == "Error");
            importRun.Status = ImportStatus.Parsed;
            importRun.ParsedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            importRun.Status = ImportStatus.ParseFailed;
            importRun.ErrorMessage = ex.Message;
        }

        await _importRunRepository.UpdateAsync(importRun, cancellationToken);

        return new UploadResultDto(
            importRun.Id,
            importRun.FileName,
            importRun.FileSizeBytes,
            importRun.Status);
    }
}

