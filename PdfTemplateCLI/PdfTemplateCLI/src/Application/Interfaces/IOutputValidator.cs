namespace PdfTemplateCLI.Application.Interfaces;

public interface IOutputValidator
{
    Task<ValidationResult> ValidateDocumentOutputAsync(
        string originalDocument, 
        string templateUsed, 
        string generatedOutputPath,
        CancellationToken cancellationToken = default);
    
    Task<BatchValidationResult> ValidateBatchAsync(
        IEnumerable<ValidationRequest> requests,
        CancellationToken cancellationToken = default);
}