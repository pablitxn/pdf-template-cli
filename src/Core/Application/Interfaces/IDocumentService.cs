using PdfTemplateCLI.Application.DTOs;

namespace PdfTemplateCLI.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> NormalizeDocumentAsync(NormalizeDocumentRequest request, CancellationToken cancellationToken = default);
    Task<DocumentDto> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default);
}