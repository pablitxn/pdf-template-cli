namespace PdfTemplateCLI.Application.DTOs;

public class NormalizeDocumentRequest
{
    public string DocumentPath { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string? OutputPath { get; set; }
}