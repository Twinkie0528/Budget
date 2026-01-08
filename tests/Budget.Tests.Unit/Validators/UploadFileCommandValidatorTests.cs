using Budget.Core.Application.Commands;
using Budget.Core.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Budget.Tests.Unit.Validators;

public class UploadFileCommandValidatorTests
{
    private readonly UploadFileCommandValidator _validator;

    public UploadFileCommandValidatorTests()
    {
        _validator = new UploadFileCommandValidator();
    }

    [Theory]
    [InlineData("test.xlsx")]
    [InlineData("budget.xls")]
    [InlineData("DATA.XLSX")]
    public void Validate_ValidExtension_ShouldPass(string fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new UploadFileCommand(stream, fileName, 1024, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [Theory]
    [InlineData("test.csv")]
    [InlineData("budget.pdf")]
    [InlineData("data.txt")]
    public void Validate_InvalidExtension_ShouldFail(string fileName)
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new UploadFileCommand(stream, fileName, 1024, "text/csv");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [Fact]
    public void Validate_EmptyFile_ShouldFail()
    {
        // Arrange
        using var stream = new MemoryStream();
        var command = new UploadFileCommand(stream, "test.xlsx", 0, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }

    [Fact]
    public void Validate_FileTooLarge_ShouldFail()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1 });
        var command = new UploadFileCommand(stream, "test.xlsx", 100 * 1024 * 1024, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileSizeBytes);
    }
}

