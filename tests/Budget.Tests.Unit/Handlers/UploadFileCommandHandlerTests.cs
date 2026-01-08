using Budget.Core.Application.Commands;
using Budget.Core.Application.Dtos;
using Budget.Core.Application.Handlers;
using Budget.Core.Domain.Entities;
using Budget.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace Budget.Tests.Unit.Handlers;

public class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IImportRunRepository> _importRunRepositoryMock;
    private readonly Mock<IExcelParser> _excelParserMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _fileStorageMock = new Mock<IFileStorage>();
        _importRunRepositoryMock = new Mock<IImportRunRepository>();
        _excelParserMock = new Mock<IExcelParser>();
        _currentUserMock = new Mock<ICurrentUser>();

        _currentUserMock.Setup(x => x.UserId).Returns("test-user");

        _handler = new UploadFileCommandHandler(
            _fileStorageMock.Object,
            _importRunRepositoryMock.Object,
            _excelParserMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFile_ShouldSaveAndParse()
    {
        // Arrange
        var storagePath = "2024/01/test-file.xlsx";
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        
        var command = new UploadFileCommand(stream, "test.xlsx", 1024, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        _fileStorageMock
            .Setup(x => x.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(storagePath);

        _fileStorageMock
            .Setup(x => x.GetAsync(storagePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

        _excelParserMock
            .Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ParsedBudgetData
            {
                Header = new ParsedHeaderDto { Title = "Test Budget" },
                Items = new List<ParsedItemDto>(),
                Errors = new List<ValidationErrorDto>()
            });

        _importRunRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ImportRun>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportRun ir, CancellationToken _) => ir);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be("test.xlsx");
        result.FileSizeBytes.Should().Be(1024);
        result.Status.Should().Be(ImportStatus.Parsed);

        _fileStorageMock.Verify(x => x.SaveAsync(It.IsAny<Stream>(), "test.xlsx", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _importRunRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ImportRun>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ParsingFails_ShouldSetParseFailedStatus()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadFileCommand(stream, "bad.xlsx", 1024, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

        _fileStorageMock
            .Setup(x => x.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("path/to/file");

        _fileStorageMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

        _excelParserMock
            .Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Parse error"));

        _importRunRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ImportRun>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImportRun ir, CancellationToken _) => ir);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ImportStatus.ParseFailed);
    }
}

