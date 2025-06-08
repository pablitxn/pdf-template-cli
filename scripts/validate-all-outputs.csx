#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Extensions.Configuration.Json, 9.0.0"
#r "nuget: Microsoft.Extensions.DependencyInjection, 9.0.0"
#r "nuget: Microsoft.SemanticKernel, 1.31.0"
#r "nuget: Aspose.PDF, 24.12.0"
#r "nuget: Aspose.Words, 24.12.0"

#load "../src/Application/DTOs/ValidationResult.cs"
#load "../src/Application/Interfaces/IOutputValidator.cs"
#load "../src/Application/Interfaces/IDocumentReader.cs"
#load "../src/Infrastructure/DocumentProcessing/CompositeDocumentReader.cs"
#load "../src/Infrastructure/Validation/SemanticKernelOutputValidator.cs"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Infrastructure.Validation;
using PdfTemplateCLI.Infrastructure.DocumentProcessing;
using System.Text.Json;

// Setup
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("../appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<AsposePdfReader>();
services.AddScoped<AsposeWordReader>();
services.AddScoped<AsposeTemplateReader>();
services.AddScoped<IDocumentReader>(sp =>
{
    var readers = new List<IDocumentReader>
    {
        sp.GetRequiredService<AsposePdfReader>(),
        sp.GetRequiredService<AsposeWordReader>(),
        sp.GetRequiredService<AsposeTemplateReader>()
    };
    return new CompositeDocumentReader(readers);
});
services.AddScoped<IOutputValidator, SemanticKernelOutputValidator>();

var serviceProvider = services.BuildServiceProvider();
var validator = serviceProvider.GetRequiredService<IOutputValidator>();

// Define test cases
var testCases = new List<(string userDoc, string template, string category)>
{
    ("user-data/documents/unformatted-contract.txt", "templates/legal/rental-agreement.txt", "Legal"),
    ("user-data/documents/messy-invoice.txt", "templates/business/invoice.txt", "Business"),
    ("user-data/documents/patient-notes.txt", "templates/medical/medical-report.txt", "Medical"),
    ("user-data/reports/project-status.txt", "templates/technical/project-proposal.txt", "Technical"),
    ("user-data/reports/sales-summary.txt", "templates/business/quotation.txt", "Business"),
    ("user-data/documents/rental-notes.txt", "templates/legal/rental-agreement.txt", "Legal")
};

// Create output directory
var outputDir = "validation-results";
Directory.CreateDirectory(outputDir);

// Process each test case
var validationRequests = new List<ValidationRequest>();
foreach (var (userDoc, template, category) in testCases)
{
    var outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(userDoc)}_normalized.pdf");
    
    // Note: In real scenario, you would run the normalization process here
    // For this validation script, we assume the files already exist
    
    validationRequests.Add(new ValidationRequest
    {
        OriginalDocumentPath = userDoc,
        TemplatePath = template,
        GeneratedOutputPath = outputFile
    });
    
    Console.WriteLine($"Added validation for: {Path.GetFileName(userDoc)} with {Path.GetFileName(template)}");
}

// Run batch validation
Console.WriteLine("\nStarting batch validation...");
var batchResult = await validator.ValidateBatchAsync(validationRequests);

// Generate summary report
var report = new
{
    Summary = new
    {
        TotalDocuments = batchResult.TotalDocuments,
        ValidDocuments = batchResult.ValidDocuments,
        InvalidDocuments = batchResult.InvalidDocuments,
        AverageConfidence = $"{batchResult.AverageConfidenceScore:P2}",
        Timestamp = batchResult.CompletedAt
    },
    DetailedResults = batchResult.Results.Select(r => new
    {
        Document = Path.GetFileName(r.DocumentPath),
        Template = r.TemplateName,
        Valid = r.ValidationResult.IsValid,
        Confidence = $"{r.ValidationResult.ConfidenceScore:P2}",
        Summary = r.ValidationResult.Summary,
        IssueCount = r.ValidationResult.Issues.Count,
        HighSeverityIssues = r.ValidationResult.Issues.Count(i => i.Severity == "High")
    }),
    IssuesByType = batchResult.Results
        .SelectMany(r => r.ValidationResult.Issues)
        .GroupBy(i => i.Type)
        .Select(g => new { Type = g.Key, Count = g.Count() }),
    Recommendations = batchResult.Results
        .Where(r => !string.IsNullOrEmpty(r.ValidationResult.Recommendation))
        .Select(r => new 
        { 
            Document = Path.GetFileName(r.DocumentPath), 
            Recommendation = r.ValidationResult.Recommendation 
        })
};

// Save detailed report
var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(Path.Combine(outputDir, "validation-report.json"), reportJson);

// Print summary
Console.WriteLine("\n=== VALIDATION SUMMARY ===");
Console.WriteLine($"Total Documents: {report.Summary.TotalDocuments}");
Console.WriteLine($"Valid: {report.Summary.ValidDocuments}");
Console.WriteLine($"Invalid: {report.Summary.InvalidDocuments}");
Console.WriteLine($"Average Confidence: {report.Summary.AverageConfidence}");

Console.WriteLine("\n=== ISSUES BY TYPE ===");
foreach (var issue in report.IssuesByType)
{
    Console.WriteLine($"{issue.Type}: {issue.Count}");
}

Console.WriteLine("\n=== DETAILED RESULTS ===");
foreach (var result in report.DetailedResults)
{
    Console.WriteLine($"\n{result.Document}:");
    Console.WriteLine($"  Template: {result.Template}");
    Console.WriteLine($"  Valid: {result.Valid}");
    Console.WriteLine($"  Confidence: {result.Confidence}");
    Console.WriteLine($"  Issues: {result.IssueCount} (High: {result.HighSeverityIssues})");
}

Console.WriteLine($"\nFull report saved to: {Path.Combine(outputDir, "validation-report.json")}");