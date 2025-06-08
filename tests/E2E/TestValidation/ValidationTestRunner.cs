using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Application.Services;
using PdfTemplateCLI.Infrastructure.AI;
using PdfTemplateCLI.Infrastructure.DocumentProcessing;
using PdfTemplateCLI.Infrastructure.Repositories;
using PdfTemplateCLI.Infrastructure.Validation;

namespace E2E.TestValidation;

public class ValidationTestRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentService _documentService;
    private readonly IOutputValidator _outputValidator;
    
    public ValidationTestRunner()
    {
        // Find the correct path for appsettings.json
        var currentDirectory = Directory.GetCurrentDirectory();
        var appSettingsPath = Path.Combine(currentDirectory, "tests", "E2E", "appsettings.json");
        
        // If not found, try looking in the current directory (when running from E2E folder)
        if (!File.Exists(appSettingsPath))
        {
            appSettingsPath = Path.Combine(currentDirectory, "appsettings.json");
        }
        
        // If still not found, try the CLI project's appsettings
        if (!File.Exists(appSettingsPath))
        {
            appSettingsPath = Path.Combine(currentDirectory, "src", "Adapters", "CLI", "appsettings.json");
        }
        
        var basePath = Path.GetDirectoryName(appSettingsPath) ?? currentDirectory;
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(Path.GetFileName(appSettingsPath), optional: false)
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging();
        
        // Register configuration and options
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<PdfTemplateCLI.Application.Configuration.ApplicationOptions>(
            configuration.GetSection("Application"));
        
        // Register repositories
        services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
        services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();
        
        // Register services
        services.AddSingleton<INormalizationService, SemanticKernelService>();
        services.AddScoped<IDocumentValidator, PdfTemplateCLI.Application.Services.DocumentValidator>();
        
        // Document readers
        services.AddScoped<AsposePdfReader>();
        services.AddScoped<AsposeWordReader>();
        services.AddScoped<AsposeImageReader>();
        services.AddScoped<AsposeTemplateReader>();
        services.AddScoped<TextFileReader>();
        services.AddScoped<IDocumentReader>(sp =>
        {
            var readers = new List<IDocumentReader>
            {
                sp.GetRequiredService<TextFileReader>(),
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
        // Create timestamped output directory
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var sessionOutputDir = CreateSessionOutputDirectory(timestamp);
        
        var testCases = GetTestCases(sessionOutputDir);
        var results = new List<TestResult>();
        
        Console.WriteLine($"Starting validation tests...");
        Console.WriteLine($"Output directory: {sessionOutputDir}\n");
        
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
        GenerateSummary(results, sessionOutputDir);
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
                Error = $"{ex.GetType().Name}: {ex.Message}{(ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "")}"
            };
        }
    }
    
    private string CreateSessionOutputDirectory(string timestamp)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var baseOutputDir = Path.Combine(currentDir, "tests", "E2E", "test-outputs");
        
        // If running from E2E directory, adjust path
        if (!Directory.Exists(Path.GetDirectoryName(baseOutputDir)))
        {
            baseOutputDir = "test-outputs";
        }
        
        var sessionDir = Path.Combine(baseOutputDir, $"run_{timestamp}");
        Directory.CreateDirectory(sessionDir);
        
        return sessionDir;
    }
    
    private List<TestCase> GetTestCases(string outputDir)
    {
        // Determine the fixtures base path
        var currentDir = Directory.GetCurrentDirectory();
        var fixturesPath = Path.Combine(currentDir, "tests", "E2E", "Fixtures");
        
        // If running from E2E directory, adjust path
        if (!Directory.Exists(fixturesPath))
        {
            fixturesPath = Path.Combine(currentDir, "Fixtures");
        }
        
        return new List<TestCase>
        {
            new TestCase
            {
                Name = "Unformatted Contract → Legal Template",
                InputDocument = Path.Combine(fixturesPath, "user-data/documents/unformatted-contract.txt"),
                Template = Path.Combine(fixturesPath, "templates/legal/rental-agreement.txt"),
                OutputPath = Path.Combine(outputDir, "contract-normalized.pdf"),
                Category = "Legal"
            },
            new TestCase
            {
                Name = "Messy Invoice → Business Template",
                InputDocument = Path.Combine(fixturesPath, "user-data/documents/messy-invoice.txt"),
                Template = Path.Combine(fixturesPath, "templates/business/invoice.txt"),
                OutputPath = Path.Combine(outputDir, "invoice-normalized.pdf"),
                Category = "Business"
            },
            new TestCase
            {
                Name = "Patient Notes → Medical Report",
                InputDocument = Path.Combine(fixturesPath, "user-data/documents/patient-notes.txt"),
                Template = Path.Combine(fixturesPath, "templates/medical/medical-report.txt"),
                OutputPath = Path.Combine(outputDir, "medical-report-normalized.pdf"),
                Category = "Medical"
            },
            new TestCase
            {
                Name = "Project Status → Technical Proposal",
                InputDocument = Path.Combine(fixturesPath, "user-data/reports/project-status.txt"),
                Template = Path.Combine(fixturesPath, "templates/technical/project-proposal.txt"),
                OutputPath = Path.Combine(outputDir, "project-proposal-normalized.pdf"),
                Category = "Technical"
            }
        };
    }
    
    private void GenerateSummary(List<TestResult> results, string outputDir)
    {
        var validResults = results.Where(r => r.ValidationResult != null).ToList();
        
        var fullReport = new
        {
            summary = new
            {
                totalTests = results.Count,
                passed = results.Count(r => r.Success),
                failed = results.Count(r => !r.Success),
                averageConfidence = validResults.Any() 
                    ? validResults.Average(r => r.ValidationResult!.ConfidenceScore)
                    : 0.0
            },
            timestamp = DateTime.UtcNow,
            resultsByCategory = results
                .GroupBy(r => r.TestCase.Category)
                .Select(g => new
                {
                    category = g.Key,
                    total = g.Count(),
                    passed = g.Count(r => r.Success)
                }),
            detailedResults = results.Select(r => new
            {
                test = r.TestCase.Name,
                passed = r.Success,
                confidence = r.ValidationResult?.ConfidenceScore,
                issues = r.ValidationResult?.Issues.Count ?? 0,
                summary = r.ValidationResult?.Summary ?? r.Error,
                llmResponse = r.ValidationResult?.RawLlmResponse,
                validationDetails = r.ValidationResult != null ? new
                {
                    isValid = r.ValidationResult.IsValid,
                    recommendation = r.ValidationResult.Recommendation,
                    extractedFields = r.ValidationResult.ExtractedFields,
                    allIssues = r.ValidationResult.Issues
                } : null
            })
        };
        
        var json = JsonSerializer.Serialize(fullReport, new JsonSerializerOptions { WriteIndented = true });
        
        // Save summary to the session output directory
        var summaryPath = Path.Combine(outputDir, "validation-summary.json");
        File.WriteAllText(summaryPath, json);
        
        // Also save to the legacy location for backward compatibility
        var currentDir = Directory.GetCurrentDirectory();
        var legacyPath = Path.Combine(currentDir, "tests", "E2E", "validation-summary.json");
        if (Directory.Exists(Path.GetDirectoryName(legacyPath)))
        {
            File.WriteAllText(legacyPath, json);
        }
        
        Console.WriteLine("\n=== TEST SUMMARY ===");
        Console.WriteLine($"Total: {fullReport.summary.totalTests}");
        Console.WriteLine($"Passed: {fullReport.summary.passed}");
        Console.WriteLine($"Failed: {fullReport.summary.failed}");
        Console.WriteLine($"Average Confidence: {fullReport.summary.averageConfidence:P2}");
        Console.WriteLine($"\nDetailed report saved to: {summaryPath}");
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