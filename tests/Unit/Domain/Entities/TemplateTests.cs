using FluentAssertions;
using PdfTemplateCLI.Domain.Entities;

namespace PdfTemplateCLI.Tests.Domain.Entities;

public class TemplateTests
{
    [Fact]
    public void Create_ShouldInitializeTemplateWithCorrectValues()
    {
        // Arrange
        var name = "legal-contract";
        var content = "Template content";
        var description = "Legal contract template";
        var type = TemplateType.Legal;

        // Act
        var template = Template.Create(name, content, description, type);

        // Assert
        template.Should().NotBeNull();
        template.Id.Should().NotBeEmpty();
        template.Name.Should().Be(name);
        template.Content.Should().Be(content);
        template.Description.Should().Be(description);
        template.Type.Should().Be(type);
        template.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        template.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateTemplatePropertiesCorrectly()
    {
        // Arrange
        var template = Template.Create("original", "original content", "original desc", TemplateType.General);
        var newName = "updated-name";
        var newContent = "updated content";
        var newDescription = "updated description";

        // Act
        template.Update(newName, newContent, newDescription);

        // Assert
        template.Name.Should().Be(newName);
        template.Content.Should().Be(newContent);
        template.Description.Should().Be(newDescription);
        template.UpdatedAt.Should().NotBeNull();
        template.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        template.Type.Should().Be(TemplateType.General); // Type should not change
    }
}