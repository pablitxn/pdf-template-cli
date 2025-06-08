using Aspose.Pdf;
using Aspose.Pdf.Text;
using PdfTemplateCLI.Application.Interfaces;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class AsposePdfReader : IDocumentReader
{
    private readonly string[] _supportedExtensions = { ".pdf" };

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            using var document = new Document(filePath);
            var textAbsorber = new TextAbsorber();
            document.Pages.Accept(textAbsorber);
            return textAbsorber.Text;
        }, cancellationToken);
    }

    public bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }
}