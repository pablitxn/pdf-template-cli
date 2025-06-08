using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Application.Services;
using PdfTemplateCLI.Infrastructure.AI;
using PdfTemplateCLI.Infrastructure.DocumentProcessing;
using PdfTemplateCLI.Infrastructure.Repositories;
using PdfTemplateCLI.Infrastructure.Validation;
using System.Text.Json;

namespace PdfTemplateCLI.TestValidation;

public class ValidationTestRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentService _documentService;
    private readonly IOutputValidator _outputValidator;
    
    public ValidationTestRunner()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection();
        
        // Register all services
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
        services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();
        services.AddSingleton<INormalizationService, SemanticKernelService>();
        
        // Document readers
        services.AddScoped<AsposePdfReader>();
        services.AddScoped<AsposeWordReader>();
        services.AddScoped<AsposeImageReader>();
        services.AddScoped<AsposeTemplateReader>();
        services.AddScoped<IDocumentReader>(sp =>
        {
            var readers = new List<IDocumentReader>
            {
                sp.GetRequiredService<AsposePdfReader>(),
                sp.GetRequiredService<AsposeWordReader>(),
                sp.GetRequiredService<AsposeImageReader>(),
                sp.GetRequiredService<AsposeTemplateReader>()
            };
            return new CompositeDocumentReader(readers);
        });
        
        services.AddScoped<IDocumentWriter, AsposeDocumentWriter>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IOutputValidator, SemanticKernelOutputValidator>();
        
        _serviceProvider = services.BuildServiceProvider();
        _documentService = _serviceProvider.GetRequiredService<IDocumentService>();
        _outputValidator = _serviceProvider.GetRequiredService<IOutputValidator>();
    }
    
    public async Task RunAllTestsAsync()
    {
        var testCases = GetTestCases();
        var results = new List<TestResult>();
        
        Console.WriteLine("Starting validation tests...\n");
        
        foreach (var testCase in testCases)
        {
            Console.WriteLine($"Processing: {testCase.Name}");
            var result = await RunSingleTestAsync(testCase);
            results.Add(result);
            
            Console.WriteLine($"  Status: {(result.Success ? "✓ PASSED" : "✗ FAILED")}");
            Console.WriteLine($"  Confidence: {result.ValidationResult?.ConfidenceScore:P2}");
            if (result.Error != null)
            {
                Console.WriteLine($"  Error: {result.Error}");
            }
            Console.WriteLine();
        }
        
        // Generate summary
        GenerateSummary(results);
    }
    
    private async Task<TestResult> RunSingleTestAsync(TestCase testCase)
    {
        try
        {
            // Normalize the document
            var normalizeRequest = new NormalizeDocumentRequest
            {
                DocumentPath = testCase.InputDocument,
                TemplateName = testCase.Template,
                OutputPath = testCase.OutputPath
            };
            
            await _documentService.NormalizeDocumentAsync(normalizeRequest);
            
            // Validate the output
            var validationResult = await _outputValidator.ValidateDocumentOutputAsync(
                testCase.InputDocument,
                testCase.Template,
                testCase.OutputPath);
            
            return new TestResult
            {
                TestCase = testCase,
                Success = validationResult.IsValid,
                ValidationResult = validationResult
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                TestCase = testCase,
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    private List<TestCase> GetTestCases()
    {
        var outputDir = "test-outputs";
        Directory.CreateDirectory(outputDir);
        
        return new List<TestCase>
        {
            new TestCase
            {
                Name = "Unformatted Contract → Legal Template",
                InputDocument = "user-data/documents/unformatted-contract.txt",
                Template = "templates/legal/rental-agreement.txt",
                OutputPath = Path.Combine(outputDir, "contract-normalized.pdf"),
                Category = "Legal"
            },
            new TestCase
            {
                Name = "Messy Invoice → Business Template",
                InputDocument = "user-data/documents/messy-invoice.txt",
                Template = "templates/business/invoice.txt",
                OutputPath = Path.Combine(outputDir, "invoice-normalized.pdf"),
                Category = "Business"
            },
            new TestCase
            {
                Name = "Patient Notes → Medical Report",
                InputDocument = "user-data/documents/patient-notes.txt",
                Template = "templates/medical/medical-report.txt",
                OutputPath = Path.Combine(outputDir, "medical-report-normalized.pdf"),
                Category = "Medical"
            },
            new TestCase
            {
                Name = "Project Status → Technical Proposal",
                InputDocument = "user-data/reports/project-status.txt",
                Template = "templates/technical/project-proposal.txt",
                OutputPath = Path.Combine(outputDir, "project-proposal-normalized.pdf"),
                Category = "Technical"
            }
        };
    }
    
    private void GenerateSummary(List<TestResult> results)
    {
        var summary = new
        {
            Timestamp = DateTime.UtcNow,
            TotalTests = results.Count,
            Passed = results.Count(r => r.Success),
            Failed = results.Count(r => !r.Success),
            AverageConfidence = results
                .Where(r => r.ValidationResult != null)
                .Average(r => r.ValidationResult.ConfidenceScore),
            ResultsByCategory = results
                .GroupBy(r => r.TestCase.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Passed = g.Count(r => r.Success)
                }),
            DetailedResults = results.Select(r => new
            {
                Test = r.TestCase.Name,
                Passed = r.Success,
                Confidence = r.ValidationResult?.ConfidenceScore,
                Issues = r.ValidationResult?.Issues.Count ?? 0,
                Summary = r.ValidationResult?.Summary ?? r.Error
            })
        };
        
        var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("validation-summary.json", json);
        
        Console.WriteLine("\n=== TEST SUMMARY ===");
        Console.WriteLine($"Total: {summary.TotalTests}");
        Console.WriteLine($"Passed: {summary.Passed}");
        Console.WriteLine($"Failed: {summary.Failed}");
        Console.WriteLine($"Average Confidence: {summary.AverageConfidence:P2}");
        Console.WriteLine("\nDetailed report saved to: validation-summary.json");
    }
    
    private class TestCase
    {
        public string Name { get; set; } = "";
        public string InputDocument { get; set; } = "";
        public string Template { get; set; } = "";
        public string OutputPath { get; set; } = "";
        public string Category { get; set; } = "";
    }
    
    private class TestResult
    {
        public TestCase TestCase { get; set; } = new();
        public bool Success { get; set; }
        public ValidationResult? ValidationResult { get; set; }
        public string? Error { get; set; }
    }
}