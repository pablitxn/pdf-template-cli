using FluentAssertions;
using PdfTemplateCLI.Domain.Entities;
using PdfTemplateCLI.Infrastructure.Repositories;

namespace PdfTemplateCLI.Tests.Infrastructure.Repositories;

public class InMemoryTemplateRepositoryTests
{
    private readonly InMemoryTemplateRepository _repository;

    public InMemoryTemplateRepositoryTests()
    {
        _repository = new InMemoryTemplateRepository();
    }

    [Fact]
    public async Task Constructor_ShouldSeedDefaultTemplates()
    {
        // Act
        var templates = await _repository.GetAllAsync();

        // Assert
        templates.Should().NotBeEmpty();
        templates.Should().Contain(t => t.Name == "legal-contract");
        templates.Should().Contain(t => t.Name == "business-proposal");
    }

    [Fact]
    public async Task GetByNameAsync_WithExistingTemplate_ShouldReturnTemplate()
    {
        // Act
        var result = await _repository.GetByNameAsync("legal-contract");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("legal-contract");
        result.Type.Should().Be(TemplateType.Legal);
    }

    [Fact]
    public async Task GetByNameAsync_WithNonExistingTemplate_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByNameAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldBeCaseInsensitive()
    {
        // Act
        var result = await _repository.GetByNameAsync("LEGAL-CONTRACT");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("legal-contract");
    }

    [Fact]
    public async Task AddAsync_ShouldAddTemplateSuccessfully()
    {
        // Arrange
        var template = Template.Create("custom-template", "content", "description", TemplateType.General);

        // Act
        var result = await _repository.AddAsync(template);

        // Assert
        result.Should().Be(template);
        
        var retrieved = await _repository.GetByIdAsync(template.Id);
        retrieved.Should().Be(template);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingTemplate_ShouldUpdateSuccessfully()
    {
        // Arrange
        var template = Template.Create("test-template", "original", "original desc", TemplateType.General);
        await _repository.AddAsync(template);
        
        template.Update("test-template", "updated content", "updated desc");

        // Act
        await _repository.UpdateAsync(template);

        // Assert
        var updated = await _repository.GetByIdAsync(template.Id);
        updated.Should().NotBeNull();
        updated!.Content.Should().Be("updated content");
        updated.Description.Should().Be("updated desc");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTemplate()
    {
        // Arrange
        var template = Template.Create("to-delete", "content", "desc", TemplateType.General);
        await _repository.AddAsync(template);

        // Act
        await _repository.DeleteAsync(template.Id);

        // Assert
        var result = await _repository.GetByIdAsync(template.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldNotThrow()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var act = () => _repository.DeleteAsync(nonExistingId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}