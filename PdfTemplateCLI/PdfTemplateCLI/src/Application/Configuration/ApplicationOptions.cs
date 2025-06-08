using System.ComponentModel.DataAnnotations;

namespace PdfTemplateCLI.Application.Configuration;

public class ApplicationOptions
{
    public const string SectionName = "Application";
    
    [Required]
    public string DefaultOutputDirectory { get; set; } = "output";
    
    [Range(1, 100)]
    public int MaxFileSizeMB { get; set; } = 50;
    
    [Range(1, 300)]
    public int ProcessingTimeoutSeconds { get; set; } = 120;
    
    public bool EnableDetailedLogging { get; set; } = false;
    
    public bool AutoOpenGeneratedFiles { get; set; } = false;
    
    public string[] AllowedFileExtensions { get; set; } = new[]
    {
        ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt",
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff"
    };
    
    public DocumentProcessingOptions DocumentProcessing { get; set; } = new();
}

public class DocumentProcessingOptions
{
    public bool PreserveFormatting { get; set; } = true;
    
    public bool ExtractImages { get; set; } = false;
    
    public int MaxConcurrentDocuments { get; set; } = 3;
    
    public string TemplateCacheDirectory { get; set; } = "cache/templates";
}