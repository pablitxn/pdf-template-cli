using FluentAssertions;
using PdfTemplateCLI.Infrastructure.AI;

namespace PdfTemplateCLI.Tests.Infrastructure.AI;

public class DocumentNormalizerPluginTests
{
    private readonly DocumentNormalizerPlugin _plugin;

    public DocumentNormalizerPluginTests()
    {
        _plugin = new DocumentNormalizerPlugin();
    }

    [Fact]
    public async Task NormalizeDocumentAsync_ShouldReturnPromptWithCorrectStructure()
    {
        // Arrange
        var documentContent = "This is the original document";
        var templateStructure = "Template structure here";

        // Act
        var result = await _plugin.NormalizeDocumentAsync(documentContent, templateStructure);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("TEMPLATE STRUCTURE:");
        result.Should().Contain(templateStructure);
        result.Should().Contain("ORIGINAL DOCUMENT CONTENT:");
        result.Should().Contain(documentContent);
        result.Should().Contain("INSTRUCTIONS:");
    }

    [Fact]
    public async Task ExtractDocumentInfoAsync_ShouldReturnPromptForExtraction()
    {
        // Arrange
        var documentContent = "Document content for extraction";

        // Act
        var result = await _plugin.ExtractDocumentInfoAsync(documentContent);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Extract and summarize the key information");
        result.Should().Contain(documentContent);
        result.Should().Contain("Document type");
        result.Should().Contain("Main topic/purpose");
        result.Should().Contain("Key sections");
    }

    [Fact]
    public async Task ValidateNormalizedDocumentAsync_ShouldReturnValidationPrompt()
    {
        // Arrange
        var normalizedDocument = "Normalized document content";
        var templateStructure = "Template structure";

        // Act
        var result = await _plugin.ValidateNormalizedDocumentAsync(normalizedDocument, templateStructure);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Validate if the normalized document");
        result.Should().Contain("TEMPLATE:");
        result.Should().Contain(templateStructure);
        result.Should().Contain("NORMALIZED DOCUMENT:");
        result.Should().Contain(normalizedDocument);
        result.Should().Contain("Compliance score");
        result.Should().Contain("Missing sections");
    }
}