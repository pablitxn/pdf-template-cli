using PdfTemplateCLI.Application.Interfaces;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class CompositeDocumentReader : IDocumentReader
{
    private readonly IEnumerable<IDocumentReader> _readers;

    public CompositeDocumentReader(IEnumerable<IDocumentReader> readers)
    {
        _readers = readers;
    }

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var reader = _readers.FirstOrDefault(r => r.IsSupported(filePath));
        
        if (reader == null)
        {
            throw new NotSupportedException($"File type not supported: {Path.GetExtension(filePath)}");
        }

        return await reader.ReadDocumentAsync(filePath, cancellationToken);
    }

    public bool IsSupported(string filePath)
    {
        return _readers.Any(r => r.IsSupported(filePath));
    }
}