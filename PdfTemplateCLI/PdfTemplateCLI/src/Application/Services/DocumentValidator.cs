using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfTemplateCLI.Application.Configuration;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Common;

namespace PdfTemplateCLI.Application.Services;

public class DocumentValidator : IDocumentValidator
{
    private readonly ILogger<DocumentValidator> _logger;
    private readonly ApplicationOptions _options;
    private readonly HashSet<string> _dangerousExtensions = new()
    {
        ".exe", ".dll", ".bat", ".cmd", ".sh", ".ps1", ".vbs", ".js", ".jar"
    };

    public DocumentValidator(ILogger<DocumentValidator> logger, IOptions<ApplicationOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Result ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogWarning("File path validation failed: empty path");
            return Result.Failure("File path cannot be empty");
        }

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File path validation failed: file not found {FilePath}", filePath);
            return Result.Failure($"File not found: {filePath}");
        }

        // Check for directory traversal attempts
        try
        {
            var fullPath = Path.GetFullPath(filePath);
            if (fullPath != filePath && !filePath.StartsWith("~"))
            {
                _logger.LogDebug("Normalized path from {Original} to {Full}", filePath, fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid file path: {FilePath}", filePath);
            return Result.Failure($"Invalid file path: {ex.Message}");
        }

        return Result.Success();
    }

    public Result ValidateFileSize(string filePath, long maxSizeInBytes)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > maxSizeInBytes)
            {
                var sizeMB = fileInfo.Length / (1024.0 * 1024.0);
                var maxMB = maxSizeInBytes / (1024.0 * 1024.0);
                _logger.LogWarning("File too large: {FilePath} is {Size:F2}MB, max is {Max:F2}MB", 
                    filePath, sizeMB, maxMB);
                return Result.Failure($"File too large: {sizeMB:F2}MB (max: {maxMB:F2}MB)");
            }

            if (fileInfo.Length == 0)
            {
                _logger.LogWarning("Empty file: {FilePath}", filePath);
                return Result.Failure("File is empty");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file size for {FilePath}", filePath);
            return Result.Failure($"Could not check file size: {ex.Message}");
        }
    }

    public Result ValidateFileExtension(string filePath, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        // Check for dangerous extensions
        if (_dangerousExtensions.Contains(extension))
        {
            _logger.LogWarning("Dangerous file extension detected: {Extension}", extension);
            return Result.Failure($"File type not allowed for security reasons: {extension}");
        }

        // If specific extensions are required, check against them
        if (allowedExtensions?.Length > 0)
        {
            var allowed = allowedExtensions.Select(e => e.ToLowerInvariant()).ToHashSet();
            if (!allowed.Contains(extension))
            {
                _logger.LogWarning("File extension {Extension} not in allowed list", extension);
                return Result.Failure($"File type not supported: {extension}");
            }
        }

        return Result.Success();
    }

    public Result<ValidationSummary> ValidateDocument(string filePath, DocumentValidationOptions options)
    {
        var summary = new ValidationSummary
        {
            FilePath = filePath,
            IsValid = true
        };

        // Validate file path
        var pathResult = ValidateFilePath(filePath);
        if (pathResult.IsFailure)
        {
            return Result<ValidationSummary>.Failure(pathResult.Error);
        }

        // Get file info
        try
        {
            var fileInfo = new FileInfo(filePath);
            summary.FileSizeInBytes = fileInfo.Length;
            summary.FileExtension = fileInfo.Extension.ToLowerInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
            return Result<ValidationSummary>.Failure($"Could not read file information: {ex.Message}");
        }

        // Validate file size
        var sizeResult = ValidateFileSize(filePath, options.MaxFileSizeInBytes);
        if (sizeResult.IsFailure)
        {
            return Result<ValidationSummary>.Failure(sizeResult.Error);
        }

        // Validate extension
        var extResult = ValidateFileExtension(filePath, options.AllowedExtensions);
        if (extResult.IsFailure)
        {
            return Result<ValidationSummary>.Failure(extResult.Error);
        }

        // Check file content if requested
        if (options.CheckFileContent)
        {
            try
            {
                // Read first few bytes to ensure file is readable
                using var stream = File.OpenRead(filePath);
                var buffer = new byte[Math.Min(1024, summary.FileSizeInBytes)];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                if (bytesRead == 0)
                {
                    summary.Warnings.Add("File appears to be empty");
                }
                
                // Check for binary content in text files
                if (IsTextExtension(summary.FileExtension))
                {
                    var hasBinaryContent = buffer.Take(bytesRead).Any(b => b == 0);
                    if (hasBinaryContent)
                    {
                        summary.Warnings.Add("Text file contains binary content");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content for {FilePath}", filePath);
                summary.Warnings.Add($"Could not verify file content: {ex.Message}");
            }
        }

        _logger.LogInformation("Document validation completed for {FilePath}: Valid={IsValid}, Size={Size}bytes, Warnings={WarningCount}",
            filePath, summary.IsValid, summary.FileSizeInBytes, summary.Warnings.Count);

        return Result<ValidationSummary>.Success(summary);
    }

    private bool IsTextExtension(string extension)
    {
        var textExtensions = new HashSet<string> { ".txt", ".md", ".json", ".xml", ".html", ".htm", ".csv" };
        return textExtensions.Contains(extension);
    }
}