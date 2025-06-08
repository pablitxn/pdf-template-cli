using PdfTemplateCLI.Domain.Enums;

namespace PdfTemplateCLI.Application.Interfaces;

public interface IDocumentWriter
{
    Task SaveDocumentAsync(string content, string outputPath, DocumentType outputType, CancellationToken cancellationToken = default);
}