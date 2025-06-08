using FluentAssertions;
using PdfTemplateCLI.Domain.ValueObjects;

namespace PdfTemplateCLI.Tests.Domain.ValueObjects;

public class FilePathTests
{
    [Fact]
    public void Constructor_WithEmptyPath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => new FilePath("");
        act.Should().Throw<ArgumentException>()
            .WithMessage("File path cannot be empty");
    }

    [Fact]
    public void Constructor_WithWhitespacePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => new FilePath("   ");
        act.Should().Throw<ArgumentException>()
            .WithMessage("File path cannot be empty");
    }

    [Fact]
    public void Constructor_WithRelativePath_ShouldConvertToAbsolutePath()
    {
        // Arrange
        var relativePath = "test.pdf";

        // Act
        var filePath = new FilePath(relativePath);

        // Assert
        filePath.Value.Should().NotBe(relativePath);
        Path.IsPathRooted(filePath.Value).Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithAbsolutePath_ShouldKeepAbsolutePath()
    {
        // Arrange
        var absolutePath = "/home/user/test.pdf";

        // Act
        var filePath = new FilePath(absolutePath);

        // Assert
        filePath.Value.Should().Be(absolutePath);
    }

    [Fact]
    public void GetFileName_ShouldReturnCorrectFileName()
    {
        // Arrange
        var path = "/home/user/documents/test.pdf";
        var filePath = new FilePath(path);

        // Act
        var fileName = filePath.GetFileName();

        // Assert
        fileName.Should().Be("test.pdf");
    }

    [Fact]
    public void GetDirectory_ShouldReturnCorrectDirectory()
    {
        // Arrange
        var path = "/home/user/documents/test.pdf";
        var filePath = new FilePath(path);

        // Act
        var directory = filePath.GetDirectory();

        // Assert
        directory.Should().Be("/home/user/documents");
    }

    [Fact]
    public void GetExtension_ShouldReturnCorrectExtension()
    {
        // Arrange
        var path = "/home/user/test.pdf";
        var filePath = new FilePath(path);

        // Act
        var extension = filePath.GetExtension();

        // Assert
        extension.Should().Be(".pdf");
    }

    [Fact]
    public void ImplicitOperatorToString_ShouldReturnValue()
    {
        // Arrange
        var path = "/home/user/test.pdf";
        var filePath = new FilePath(path);

        // Act
        string result = filePath;

        // Assert
        result.Should().Be(path);
    }

    [Fact]
    public void ImplicitOperatorFromString_ShouldCreateFilePath()
    {
        // Arrange
        var path = "/home/user/test.pdf";

        // Act
        FilePath filePath = path;

        // Assert
        filePath.Value.Should().Be(path);
    }
}