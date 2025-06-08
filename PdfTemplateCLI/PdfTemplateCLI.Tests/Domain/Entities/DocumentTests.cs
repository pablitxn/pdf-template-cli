using FluentAssertions;
using PdfTemplateCLI.Domain.Entities;

namespace PdfTemplateCLI.Tests.Domain.Entities;

public class DocumentTests
{
    [Fact]
    public void Create_ShouldInitializeDocumentWithCorrectValues()
    {
        // Arrange
        var fileName = "test-document.pdf";
        var originalContent = "This is test content";

        // Act
        var document = Document.Create(fileName, originalContent);

        // Assert
        document.Should().NotBeNull();
        document.Id.Should().NotBeEmpty();
        document.FileName.Should().Be(fileName);
        document.OriginalContent.Should().Be(originalContent);
        document.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.Status.Should().Be(DocumentStatus.Pending);
        document.NormalizedContent.Should().BeNull();
        document.NormalizedAt.Should().BeNull();
    }

    [Fact]
    public void SetNormalizedContent_ShouldUpdateDocumentCorrectly()
    {
        // Arrange
        var document = Document.Create("test.pdf", "original content");
        var normalizedContent = "normalized content";

        // Act
        document.SetNormalizedContent(normalizedContent);

        // Assert
        document.NormalizedContent.Should().Be(normalizedContent);
        document.NormalizedAt.Should().NotBeNull();
        document.NormalizedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        document.Status.Should().Be(DocumentStatus.Normalized);
    }

    [Fact]
    public void MarkAsFailed_ShouldUpdateStatusToFailed()
    {
        // Arrange
        var document = Document.Create("test.pdf", "original content");

        // Act
        document.MarkAsFailed();

        // Assert
        document.Status.Should().Be(DocumentStatus.Failed);
        document.NormalizedContent.Should().BeNull();
        document.NormalizedAt.Should().BeNull();
    }
}