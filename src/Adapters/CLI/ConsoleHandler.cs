using Microsoft.Extensions.Logging;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Exceptions;

namespace PdfTemplateCLI.Presentation.ConsoleUI;

public class ConsoleHandler
{
    private readonly IDocumentService _documentService;
    private readonly ITemplateRepository _templateRepository;
    private readonly ILogger<ConsoleHandler> _logger;

    public ConsoleHandler(
        IDocumentService documentService, 
        ITemplateRepository templateRepository,
        ILogger<ConsoleHandler> logger)
    {
        _documentService = documentService;
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        ConsoleHelper.WriteHeader("PDF Template CLI - Document Normalizer");
        Console.WriteLine("Powered by Aspose for document processing");
        Console.WriteLine();
        ConsoleHelper.WriteInfo("Supported formats:");
        Console.WriteLine("  • Input: PDF, Word (DOC/DOCX/RTF/ODT), Images (JPG/PNG/BMP/GIF/TIFF)");
        Console.WriteLine("  • Output: PDF, Word, HTML, Text, Markdown");
        Console.WriteLine();

        while (true)
        {
            try
            {
                ShowMenu();
                var choice = ConsoleHelper.ReadInput("Your choice");

                switch (choice)
                {
                    case "1":
                        await NormalizeDocumentAsync();
                        break;
                    case "2":
                        await ListTemplatesAsync();
                        break;
                    case "3":
                        await ViewDocumentHistoryAsync();
                        break;
                    case "4":
                        ConsoleHelper.WriteInfo("Thank you for using PDF Template CLI!");
                        return;
                    default:
                        ConsoleHelper.WriteWarning("Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in console handler");
                ConsoleHelper.ShowException(ex, showDetails: false);
                
                if (ConsoleHelper.Confirm("\nShow error details?", defaultValue: false))
                {
                    ConsoleHelper.ShowException(ex, showDetails: true);
                }
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }

    private void ShowMenu()
    {
        Console.WriteLine("Choose an option:");
        Console.WriteLine("1. Normalize a document");
        Console.WriteLine("2. List available templates");
        Console.WriteLine("3. View document history");
        Console.WriteLine("4. Exit");
        Console.Write("\nYour choice: ");
    }

    private async Task NormalizeDocumentAsync()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Normalize Document");

        // Get document path
        var documentPath = ConsoleHelper.ReadInput("Enter document path");
        
        if (!File.Exists(documentPath))
        {
            ConsoleHelper.WriteError($"File '{documentPath}' not found.");
            return;
        }

        // Show file info
        ConsoleHelper.WriteInfo("Document information:");
        ConsoleHelper.ShowFileInfo(documentPath);
        Console.WriteLine();

        // Get template
        var templateName = ConsoleHelper.ReadInput(
            "Enter template name or template file path\n" +
            "  (e.g., legal-contract, business-proposal, or /path/to/template.pdf)");

        // Get output path
        var outputPath = ConsoleHelper.ReadInput(
            "Enter output path with extension or leave empty to display\n" +
            "  (e.g., output.pdf, result.docx, report.html)");

        try
        {
            var request = new NormalizeDocumentRequest
            {
                DocumentPath = documentPath,
                TemplateName = templateName,
                OutputPath = string.IsNullOrWhiteSpace(outputPath) ? null : outputPath
            };

            DocumentDto result = null!;
            
            // Process with progress indicator
            await ConsoleHelper.ShowProgressAsync("Reading document", async () =>
            {
                await Task.Delay(100); // Allow UI to update
            });
            
            await ConsoleHelper.ShowProgressAsync("Applying template and normalizing", async () =>
            {
                result = await _documentService.NormalizeDocumentAsync(request);
            });

            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                await ConsoleHelper.ShowProgressAsync("Saving output", async () =>
                {
                    await Task.Delay(100); // Allow UI to update
                });
            }
            
            Console.WriteLine();
            ConsoleHelper.WriteSuccess("Document normalized successfully!");
            ConsoleHelper.WriteInfo($"Document ID: {result.Id}");
            ConsoleHelper.WriteInfo($"Status: {result.Status}");
            
            if (!string.IsNullOrWhiteSpace(outputPath))
            {
                ConsoleHelper.WriteSuccess($"Output saved to: {outputPath}");
                
                if (ConsoleHelper.Confirm("Open the output file?", defaultValue: false))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = outputPath,
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        ConsoleHelper.WriteWarning("Could not open the file automatically.");
                    }
                }
            }
            else
            {
                Console.WriteLine("\n--- Normalized Content ---");
                Console.WriteLine(result.NormalizedContent);
                Console.WriteLine("--- End of Content ---");
            }
        }
        catch (DocumentNotFoundException ex)
        {
            _logger.LogError(ex, "Document not found");
            ConsoleHelper.WriteError($"Document not found: {ex.Message}");
        }
        catch (TemplateNotFoundException ex)
        {
            _logger.LogError(ex, "Template not found");
            ConsoleHelper.WriteError($"Template not found: {ex.Message}");
            ConsoleHelper.WriteInfo("Use option 2 to see available templates.");
        }
        catch (InvalidDocumentFormatException ex)
        {
            _logger.LogError(ex, "Invalid document format");
            ConsoleHelper.WriteError($"Invalid document format: {ex.Message}");
            ConsoleHelper.WriteInfo("Supported formats: PDF, Word (DOC/DOCX/RTF/ODT), Images (JPG/PNG/BMP/GIF/TIFF)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error normalizing document");
            ConsoleHelper.ShowException(ex, showDetails: false);
        }
    }

    private async Task ListTemplatesAsync()
    {
        Console.Clear();
        ConsoleHelper.WriteHeader("Available Templates");

        try
        {
            var templates = await _templateRepository.GetAllAsync();
            
            if (!templates.Any())
            {
                ConsoleHelper.WriteWarning("No templates available.");
                return;
            }
            
            foreach (var template in templates)
            {
                ConsoleHelper.WriteInfo($"Template: {template.Name}");
                Console.WriteLine($"  Type: {template.Type}");
                Console.WriteLine($"  Description: {template.Description}");
                Console.WriteLine($"  Created: {template.CreatedAt:yyyy-MM-dd}");
                Console.WriteLine();
            }
            
            ConsoleHelper.WriteStep("You can also use custom template files by providing their full path.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing templates");
            ConsoleHelper.WriteError($"Error loading templates: {ex.Message}");
        }
    }

    private async Task ViewDocumentHistoryAsync()
    {
        Console.Clear();
        Console.WriteLine("=== Document History ===\n");

        var documents = await _documentService.GetAllDocumentsAsync();
        
        if (!documents.Any())
        {
            Console.WriteLine("No documents processed yet.");
            return;
        }

        foreach (var doc in documents.OrderByDescending(d => d.CreatedAt))
        {
            Console.WriteLine($"ID: {doc.Id}");
            Console.WriteLine($"File: {doc.FileName}");
            Console.WriteLine($"Status: {doc.Status}");
            Console.WriteLine($"Created: {doc.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            if (doc.NormalizedAt.HasValue)
            {
                Console.WriteLine($"Normalized: {doc.NormalizedAt.Value:yyyy-MM-dd HH:mm:ss}");
            }
            Console.WriteLine(new string('-', 50));
        }
    }
}