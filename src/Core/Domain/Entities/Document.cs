namespace PdfTemplateCLI.Domain.Entities;

public class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; } = null!;
    public string OriginalContent { get; private set; } = null!;
    public string? NormalizedContent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? NormalizedAt { get; private set; }
    public DocumentStatus Status { get; private set; }

    private Document() { }

    public static Document Create(string fileName, string originalContent)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            OriginalContent = originalContent,
            CreatedAt = DateTime.UtcNow,
            Status = DocumentStatus.Pending
        };
    }

    public void SetNormalizedContent(string normalizedContent)
    {
        NormalizedContent = normalizedContent;
        NormalizedAt = DateTime.UtcNow;
        Status = DocumentStatus.Normalized;
    }

    public void MarkAsFailed()
    {
        Status = DocumentStatus.Failed;
    }
}

public enum DocumentStatus
{
    Pending,
    Processing,
    Normalized,
    Failed
}