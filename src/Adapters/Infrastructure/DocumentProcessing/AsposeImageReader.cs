using Aspose.Imaging;
using Aspose.Imaging.FileFormats.Pdf;
using PdfTemplateCLI.Application.Interfaces;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class AsposeImageReader : IDocumentReader
{
    private readonly string[] _supportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff" };
    private readonly INormalizationService _normalizationService;

    public AsposeImageReader(INormalizationService normalizationService)
    {
        _normalizationService = normalizationService;
    }

    public async Task<string> ReadDocumentAsync(string filePath, CancellationToken cancellationToken = default)
    {
        // For images, we'll use OCR or AI to extract text
        // For now, we'll return a prompt for the AI to analyze the image
        var fileName = Path.GetFileName(filePath);
        
        // In a real implementation, you'd use OCR here
        // For now, we'll use the AI service to analyze the image
        var prompt = $"Please analyze the image '{fileName}' and extract all text content from it. If it's a document image, preserve the structure and formatting as much as possible.";
        
        return await Task.FromResult($"[Image file: {fileName}]\n{prompt}");
    }

    public bool IsSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return _supportedExtensions.Contains(extension);
    }
}