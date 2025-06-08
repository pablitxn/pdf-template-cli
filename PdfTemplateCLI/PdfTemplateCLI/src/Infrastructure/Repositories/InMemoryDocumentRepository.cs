using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Entities;

namespace PdfTemplateCLI.Infrastructure.Repositories;

public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly List<Document> _documents = new();

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = _documents.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(document);
    }

    public Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Document>>(_documents);
    }

    public Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        _documents.Add(document);
        return Task.FromResult(document);
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        var index = _documents.FindIndex(d => d.Id == document.Id);
        if (index >= 0)
        {
            _documents[index] = document;
        }
        return Task.CompletedTask;
    }
}