using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Enums;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class TextFileReader : IDocumentReader
{
    private readonly string[] _supportedExtensions = { ".txt", ".text", ".log", ".md" };

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return await File.ReadAllTextAsync(filePath, cancellationToken);
    }

    public bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }

    public DocumentType GetDocumentType()
    {
        return DocumentType.Text;
    }
}