namespace PdfTemplateCLI.Application.Interfaces;

public interface INormalizationService
{
    Task<string> NormalizeWithTemplateAsync(string content, string templateContent, CancellationToken cancellationToken = default);
}