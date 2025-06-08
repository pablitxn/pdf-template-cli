namespace PdfTemplateCLI.Application.DTOs;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalContent { get; set; } = string.Empty;
    public string? NormalizedContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NormalizedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}