using PdfTemplateCLI.Domain.Common;

namespace PdfTemplateCLI.Application.Interfaces;

public interface IDocumentValidator
{
    Result ValidateFilePath(string filePath);
    Result ValidateFileSize(string filePath, long maxSizeInBytes);
    Result ValidateFileExtension(string filePath, string[] allowedExtensions);
    Result<ValidationSummary> ValidateDocument(string filePath, DocumentValidationOptions options);
}

public class DocumentValidationOptions
{
    public long MaxFileSizeInBytes { get; set; } = 50 * 1024 * 1024; // 50MB default
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public bool CheckFileContent { get; set; } = true;
}

public class ValidationSummary
{
    public bool IsValid { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}