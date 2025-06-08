using Aspose.Pdf;
using Aspose.Pdf.Text;
using Aspose.Words;
using PdfTemplateCLI.Application.Interfaces;
using Document = Aspose.Pdf.Document;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class AsposeTemplateReader : IDocumentReader
{
    private readonly string[] _supportedExtensions = { ".txt", ".pdf", ".doc", ".docx", ".rtf", ".odt" };

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (extension == ".txt")
            {
                return File.ReadAllText(filePath);
            }
            else if (extension == ".pdf")
            {
                return ReadPdfTemplate(filePath);
            }
            else if (_supportedExtensions.Contains(extension))
            {
                return ReadWordTemplate(filePath);
            }
            
            throw new NotSupportedException($"Template format not supported: {extension}");
        }, cancellationToken);
    }

    public bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }
    
    private string ReadPdfTemplate(string filePath)
    {
        using var document = new Document(filePath);
        var textAbsorber = new TextAbsorber();
        document.Pages.Accept(textAbsorber);
        return textAbsorber.Text;
    }
    
    private string ReadWordTemplate(string filePath)
    {
        var doc = new Aspose.Words.Document(filePath);
        return doc.GetText();
    }

    public PdfTemplateCLI.Domain.Enums.DocumentType GetDocumentType()
    {
        return PdfTemplateCLI.Domain.Enums.DocumentType.Text;
    }
}