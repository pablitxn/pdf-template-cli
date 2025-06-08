using FluentAssertions;
using PdfTemplateCLI.Domain.Entities;
using PdfTemplateCLI.Infrastructure.Repositories;

namespace PdfTemplateCLI.Tests.Infrastructure.Repositories;

public class InMemoryDocumentRepositoryTests
{
    private readonly InMemoryDocumentRepository _repository;

    public InMemoryDocumentRepositoryTests()
    {
        _repository = new InMemoryDocumentRepository();
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocumentSuccessfully()
    {
        // Arrange
        var document = Document.Create("test.pdf", "content");

        // Act
        var result = await _repository.AddAsync(document);

        // Assert
        result.Should().Be(document);
        
        var retrieved = await _repository.GetByIdAsync(document.Id);
        retrieved.Should().Be(document);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        var doc1 = Document.Create("doc1.pdf", "content1");
        var doc2 = Document.Create("doc2.pdf", "content2");
        
        await _repository.AddAsync(doc1);
        await _repository.AddAsync(doc2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(doc1);
        result.Should().Contain(doc2);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingDocument_ShouldUpdateSuccessfully()
    {
        // Arrange
        var document = Document.Create("test.pdf", "original content");
        await _repository.AddAsync(document);
        
        document.SetNormalizedContent("normalized content");

        // Act
        await _repository.UpdateAsync(document);

        // Assert
        var updated = await _repository.GetByIdAsync(document.Id);
        updated.Should().NotBeNull();
        updated!.NormalizedContent.Should().Be("normalized content");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingDocument_ShouldNotThrow()
    {
        // Arrange
        var document = Document.Create("test.pdf", "content");

        // Act
        var act = () => _repository.UpdateAsync(document);

        // Assert
        await act.Should().NotThrowAsync();
    }
}