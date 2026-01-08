using Budget.Core.Application.Commands;
using FluentValidation;

namespace Budget.Core.Application.Validators;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .Must(HaveValidExtension).WithMessage($"File must be an Excel file ({string.Join(", ", AllowedExtensions)})");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("File is empty")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage($"File size exceeds maximum allowed ({MaxFileSizeBytes / 1024 / 1024} MB)");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required");
    }

    private static bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }
}

public class CommitImportCommandValidator : AbstractValidator<CommitImportCommand>
{
    public CommitImportCommandValidator()
    {
        RuleFor(x => x.ImportRunId)
            .NotEmpty().WithMessage("Import run ID is required");
    }
}

