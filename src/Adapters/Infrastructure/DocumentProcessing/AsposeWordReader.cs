using Aspose.Words;
using PdfTemplateCLI.Application.Interfaces;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class AsposeWordReader : IDocumentReader
{
    private readonly string[] _supportedExtensions = { ".doc", ".docx", ".rtf", ".odt" };

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var document = new Document(filePath);
            return document.ToString(SaveFormat.Text);
        }, cancellationToken);
    }

    public bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }
}