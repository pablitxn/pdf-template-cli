using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfTemplateCLI.Application.Configuration;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Entities;
using PdfTemplateCLI.Domain.Enums;
using PdfTemplateCLI.Domain.Exceptions;

namespace PdfTemplateCLI.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly INormalizationService _normalizationService;
    private readonly IDocumentReader _documentReader;
    private readonly IDocumentWriter _documentWriter;
    private readonly IDocumentValidator _documentValidator;
    private readonly IOptions<ApplicationOptions> _options;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        ITemplateRepository templateRepository,
        INormalizationService normalizationService,
        IDocumentReader documentReader,
        IDocumentWriter documentWriter,
        IDocumentValidator documentValidator,
        IOptions<ApplicationOptions> options,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _templateRepository = templateRepository;
        _normalizationService = normalizationService;
        _documentReader = documentReader;
        _documentWriter = documentWriter;
        _documentValidator = documentValidator;
        _options = options;
        _logger = logger;
    }

    public async Task<DocumentDto> NormalizeDocumentAsync(NormalizeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document normalization for {DocumentPath} with template {TemplateName}", 
            request.DocumentPath, request.TemplateName);
        
        // Validate document
        var validationOptions = new DocumentValidationOptions
        {
            AllowedExtensions = _options.Value.AllowedFileExtensions,
            MaxFileSizeInBytes = _options.Value.MaxFileSizeMB * 1024 * 1024
        };
        
        var validationResult = _documentValidator.ValidateDocument(request.DocumentPath, validationOptions);
        if (validationResult.IsFailure)
        {
            _logger.LogError("Document validation failed: {Error}", validationResult.Error);
            throw new DocumentProcessingException(validationResult.Error, new InvalidOperationException());
        }
        
        if (validationResult.Value.Warnings.Any())
        {
            foreach (var warning in validationResult.Value.Warnings)
            {
                _logger.LogWarning("Document validation warning: {Warning}", warning);
            }
        }
            
        if (!_documentReader.IsSupported(request.DocumentPath))
        {
            var extension = Path.GetExtension(request.DocumentPath);
            _logger.LogError("Unsupported document format: {Extension}", extension);
            throw new InvalidDocumentFormatException(extension);
        }

        try
        {
            // Read document content using Aspose
            _logger.LogDebug("Reading document content from {DocumentPath}", request.DocumentPath);
            var documentContent = await _documentReader.ReadDocumentAsync(request.DocumentPath, cancellationToken);
            _logger.LogDebug("Document read successfully, content length: {Length} characters", documentContent.Length);
            
            // Get template (can be from repository or file path)
            Template? template = null;
            string templateContent;
            
            if (File.Exists(request.TemplateName))
            {
                // If template name is a file path, read it directly
                _logger.LogDebug("Reading template from file: {TemplatePath}", request.TemplateName);
                templateContent = await _documentReader.ReadDocumentAsync(request.TemplateName, cancellationToken);
            }
            else
            {
                // Otherwise, get from repository
                _logger.LogDebug("Looking for template in repository: {TemplateName}", request.TemplateName);
                template = await _templateRepository.GetByNameAsync(request.TemplateName, cancellationToken);
                if (template == null)
                {
                    _logger.LogError("Template not found: {TemplateName}", request.TemplateName);
                    throw new TemplateNotFoundException(request.TemplateName);
                }
                templateContent = template.Content;
            }

            var document = Document.Create(Path.GetFileName(request.DocumentPath), documentContent);
            
            _logger.LogInformation("Starting normalization process");
            var normalizedContent = await _normalizationService.NormalizeWithTemplateAsync(
                documentContent, 
                templateContent, 
                cancellationToken);
            
            document.SetNormalizedContent(normalizedContent);
            _logger.LogInformation("Document normalized successfully");
            
            if (!string.IsNullOrEmpty(request.OutputPath))
            {
                // Determine output type from extension or default to PDF
                var outputType = DetermineOutputType(request.OutputPath);
                _logger.LogInformation("Saving document to {OutputPath} as {OutputType}", request.OutputPath, outputType);
                await _documentWriter.SaveDocumentAsync(normalizedContent, request.OutputPath, outputType, cancellationToken);
                _logger.LogInformation("Document saved successfully");
            }

            await _documentRepository.AddAsync(document, cancellationToken);
            _logger.LogInformation("Document normalization completed for {DocumentPath}", request.DocumentPath);
            
            return MapToDto(document);
        }
        catch (NormalizationException ex)
        {
            _logger.LogError(ex, "Normalization failed for document {DocumentPath}", request.DocumentPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document processing");
            throw new DocumentProcessingException($"Failed to process document: {request.DocumentPath}", ex);
        }
    }

    public async Task<DocumentDto> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document == null)
            throw new InvalidOperationException($"Document with ID {documentId} not found");
        
        return MapToDto(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetAllAsync(cancellationToken);
        return documents.Select(MapToDto);
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            OriginalContent = document.OriginalContent,
            NormalizedContent = document.NormalizedContent,
            CreatedAt = document.CreatedAt,
            NormalizedAt = document.NormalizedAt,
            Status = document.Status.ToString()
        };
    }

    private static DocumentType DetermineOutputType(string outputPath)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => DocumentType.Pdf,
            ".doc" or ".docx" or ".rtf" or ".odt" => DocumentType.Word,
            ".html" or ".htm" => DocumentType.Html,
            ".txt" => DocumentType.Text,
            ".md" => DocumentType.Markdown,
            _ => DocumentType.Pdf // Default to PDF
        };
    }
}