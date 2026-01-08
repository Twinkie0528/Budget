using System.Net;
using System.Net.Http.Headers;
using Budget.Core.Application.Dtos;
using Budget.Core.Domain.Entities;
using ClosedXML.Excel;
using FluentAssertions;
using System.Text.Json;

namespace Budget.Tests.Integration.Controllers;

[Collection("Integration")]
public class ImportsControllerTests
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ImportsControllerTests(WebApplicationFactoryFixture factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task Upload_ValidExcelFile_ShouldReturnSuccess()
    {
        // Arrange
        var fileContent = CreateTestExcelFile();
        using var content = new MultipartFormDataContent();
        var fileStreamContent = new ByteArrayContent(fileContent);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(fileStreamContent, "file", "test-budget.xlsx");

        // Act
        var response = await _client.PostAsync("/api/imports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UploadResultDto>(responseContent, _jsonOptions);
        
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test-budget.xlsx");
        result.ImportRunId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Upload_InvalidFile_ShouldReturnBadRequest()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        var fileStreamContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileStreamContent, "file", "test.csv");

        // Act
        var response = await _client.PostAsync("/api/imports/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPreview_AfterUpload_ShouldReturnParsedData()
    {
        // Arrange - Upload file first
        var fileContent = CreateTestExcelFile();
        using var uploadContent = new MultipartFormDataContent();
        var fileStreamContent = new ByteArrayContent(fileContent);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        uploadContent.Add(fileStreamContent, "file", "test-budget.xlsx");

        var uploadResponse = await _client.PostAsync("/api/imports/upload", uploadContent);
        var uploadResult = JsonSerializer.Deserialize<UploadResultDto>(
            await uploadResponse.Content.ReadAsStringAsync(), _jsonOptions);

        // Act
        var previewResponse = await _client.GetAsync($"/api/imports/{uploadResult!.ImportRunId}/preview");

        // Assert
        previewResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var previewContent = await previewResponse.Content.ReadAsStringAsync();
        var preview = JsonSerializer.Deserialize<ImportPreviewDto>(previewContent, _jsonOptions);
        
        preview.Should().NotBeNull();
        preview!.Header.Should().NotBeNull();
        preview.Header!.Title.Should().Be("Test Budget 2024");
    }

    private static byte[] CreateTestExcelFile()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Budget");

        // Header data
        worksheet.Cell("A2").Value = "Title";
        worksheet.Cell("B2").Value = "Test Budget 2024";
        worksheet.Cell("A3").Value = "Request Number";
        worksheet.Cell("B3").Value = "BR-TEST-001";
        worksheet.Cell("A4").Value = "Description";
        worksheet.Cell("B4").Value = "Test budget description";
        worksheet.Cell("A5").Value = "Channel";
        worksheet.Cell("B5").Value = "Digital";
        worksheet.Cell("A6").Value = "Owner";
        worksheet.Cell("B6").Value = "Test Owner";
        worksheet.Cell("A9").Value = "Fiscal Year";
        worksheet.Cell("B9").Value = 2024;
        worksheet.Cell("A11").Value = "Currency";
        worksheet.Cell("B11").Value = "USD";

        // Detail data header row
        worksheet.Cell("A13").Value = "Description";
        worksheet.Cell("B13").Value = "Category";
        worksheet.Cell("C13").Value = "Sub-Category";
        worksheet.Cell("F13").Value = "Amount";

        // Detail rows
        worksheet.Cell("A14").Value = "Line item 1";
        worksheet.Cell("B14").Value = "Marketing";
        worksheet.Cell("C14").Value = "Digital Ads";
        worksheet.Cell("F14").Value = 10000;

        worksheet.Cell("A15").Value = "Line item 2";
        worksheet.Cell("B15").Value = "Operations";
        worksheet.Cell("C15").Value = "Software";
        worksheet.Cell("F15").Value = 5000;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

