using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PdfTemplateCLI.Application.Configuration;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Application.Services;
using PdfTemplateCLI.Domain.Entities;

namespace PdfTemplateCLI.Tests.Application.Services;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;
    private readonly Mock<ITemplateRepository> _templateRepositoryMock;
    private readonly Mock<INormalizationService> _normalizationServiceMock;
    private readonly Mock<IDocumentReader> _documentReaderMock;
    private readonly Mock<IDocumentWriter> _documentWriterMock;
    private readonly Mock<IDocumentValidator> _documentValidatorMock;
    private readonly Mock<IOptions<ApplicationOptions>> _optionsMock;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
        _templateRepositoryMock = new Mock<ITemplateRepository>();
        _normalizationServiceMock = new Mock<INormalizationService>();
        _documentReaderMock = new Mock<IDocumentReader>();
        _documentWriterMock = new Mock<IDocumentWriter>();
        _documentValidatorMock = new Mock<IDocumentValidator>();
        _optionsMock = new Mock<IOptions<ApplicationOptions>>();
        _loggerMock = new Mock<ILogger<DocumentService>>();
        
        _optionsMock.Setup(x => x.Value).Returns(new ApplicationOptions());
        
        _documentService = new DocumentService(
            _documentRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _normalizationServiceMock.Object,
            _documentReaderMock.Object,
            _documentWriterMock.Object,
            _documentValidatorMock.Object,
            _optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task NormalizeDocumentAsync_WithValidRequest_ShouldReturnNormalizedDocument()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Test document content");
        
        var request = new NormalizeDocumentRequest
        {
            DocumentPath = tempFile,
            TemplateName = "legal-contract",
            OutputPath = null
        };

        var template = Template.Create("legal-contract", "Template content", "Description", TemplateType.Legal);
        _templateRepositoryMock.Setup(x => x.GetByNameAsync("legal-contract", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _normalizationServiceMock.Setup(x => x.NormalizeWithTemplateAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Normalized content");

        _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document doc, CancellationToken ct) => doc);

        // Act
        var result = await _documentService.NormalizeDocumentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().Be(Path.GetFileName(tempFile));
        result.NormalizedContent.Should().Be("Normalized content");
        result.Status.Should().Be("Normalized");
        
        _templateRepositoryMock.Verify(x => x.GetByNameAsync("legal-contract", It.IsAny<CancellationToken>()), Times.Once);
        _normalizationServiceMock.Verify(x => x.NormalizeWithTemplateAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()), Times.Once);
        _documentRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Once);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public async Task NormalizeDocumentAsync_WithInvalidTemplate_ShouldThrowException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Test content");
        
        var request = new NormalizeDocumentRequest
        {
            DocumentPath = tempFile,
            TemplateName = "non-existent",
            OutputPath = null
        };

        _templateRepositoryMock.Setup(x => x.GetByNameAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Template?)null);

        // Act
        var act = () => _documentService.NormalizeDocumentAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template 'non-existent' not found");

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public async Task NormalizeDocumentAsync_WithOutputPath_ShouldSaveNormalizedContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var outputFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Test content");
        
        var request = new NormalizeDocumentRequest
        {
            DocumentPath = tempFile,
            TemplateName = "legal-contract",
            OutputPath = outputFile
        };

        var template = Template.Create("legal-contract", "Template", "Description", TemplateType.Legal);
        _templateRepositoryMock.Setup(x => x.GetByNameAsync("legal-contract", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _normalizationServiceMock.Setup(x => x.NormalizeWithTemplateAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Normalized content");

        _documentRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document doc, CancellationToken ct) => doc);

        // Act
        await _documentService.NormalizeDocumentAsync(request);

        // Assert
        var savedContent = await File.ReadAllTextAsync(outputFile);
        savedContent.Should().Be("Normalized content");

        // Cleanup
        File.Delete(tempFile);
        File.Delete(outputFile);
    }

    [Fact]
    public async Task GetDocumentAsync_WithExistingId_ShouldReturnDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create("test.pdf", "content");
        var documentIdProperty = document.GetType().GetProperty("Id");
        documentIdProperty?.SetValue(document, documentId);

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _documentService.GetDocumentAsync(documentId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(documentId);
        result.FileName.Should().Be("test.pdf");
    }

    [Fact]
    public async Task GetDocumentAsync_WithNonExistingId_ShouldThrowException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var act = () => _documentService.GetDocumentAsync(documentId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Document with ID {documentId} not found");
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            Document.Create("doc1.pdf", "content1"),
            Document.Create("doc2.pdf", "content2")
        };

        _documentRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _documentService.GetAllDocumentsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(d => d.FileName).Should().BeEquivalentTo(new[] { "doc1.pdf", "doc2.pdf" });
    }
}