namespace PdfTemplateCLI.Application.Interfaces;

public interface IDocumentReader
{
    Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default);
    bool IsSupported(string filePath);
}