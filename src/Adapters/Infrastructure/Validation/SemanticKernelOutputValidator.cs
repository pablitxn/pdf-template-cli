using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PdfTemplateCLI.Application.DTOs;
using PdfTemplateCLI.Application.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PdfTemplateCLI.Infrastructure.Validation;

public class SemanticKernelOutputValidator : IOutputValidator
{
    private readonly Kernel _kernel;
    private readonly IDocumentReader _documentReader;
    
    public SemanticKernelOutputValidator(IConfiguration configuration, IDocumentReader documentReader)
    {
        _documentReader = documentReader;
        
        var builder = Kernel.CreateBuilder();
        // Try to get API key from configuration first, then from environment variable
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured. Please set it in appsettings.json or as OPENAI_API_KEY environment variable.");
        }
        
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-4-turbo-preview",
            apiKey: apiKey);
        
        _kernel = builder.Build();
    }
    
    public async Task<ValidationResult> ValidateDocumentOutputAsync(
        string originalDocument, 
        string templateUsed, 
        string generatedOutputPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Read all documents
            var originalContent = await _documentReader.ReadDocumentAsync(originalDocument, cancellationToken);
            var templateContent = await _documentReader.ReadDocumentAsync(templateUsed, cancellationToken);
            var generatedContent = await _documentReader.ReadDocumentAsync(generatedOutputPath, cancellationToken);
            
            // Escape template placeholders to prevent function interpretation
            var escapedTemplate = templateContent.Replace("{{", "[[").Replace("}}", "]]");
            var escapedGenerated = generatedContent.Replace("{{", "[[").Replace("}}", "]]");
            
            // Create validation prompt
            var prompt = $@"You are a document validation expert. Your task is to validate if a generated document correctly follows a template and contains all the information from the original document.

IMPORTANT: Templates use [[placeholder]] format (shown as double brackets to avoid conflicts). These are NOT function calls.

ORIGINAL DOCUMENT (unformatted):
{originalContent}

TEMPLATE USED:
{escapedTemplate}

GENERATED DOCUMENT:
{escapedGenerated}

Please analyze and provide a JSON response with the following structure:
{{
    ""isValid"": true/false,
    ""confidenceScore"": 0.0-1.0,
    ""summary"": ""Brief summary of validation results"",
    ""issues"": [
        {{
            ""type"": ""Missing|Incorrect|Formatting|Other"",
            ""field"": ""field name or section"",
            ""description"": ""what is wrong"",
            ""severity"": ""Low|Medium|High""
        }}
    ],
    ""extractedFields"": {{
        ""fieldName"": ""value extracted from generated document""
    }},
    ""recommendation"": ""What should be improved""
}}

Consider:
1. All information from the original document should be present
2. The template structure should be followed
3. Placeholders should be properly replaced
4. Formatting should be professional
5. No information should be invented or missing";

            // Use ExecutionSettings to disable function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.3,
                MaxTokens = 2000
            };
            
            var response = await _kernel.InvokePromptAsync(
                prompt, 
                new KernelArguments(executionSettings), 
                cancellationToken: cancellationToken);
            var rawResponse = response.ToString();
            
            // Clean the response - remove markdown code blocks if present
            var jsonResponse = rawResponse;
            if (jsonResponse.Contains("```json"))
            {
                jsonResponse = jsonResponse.Substring(jsonResponse.IndexOf("```json") + 7);
                jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
            }
            else if (jsonResponse.Contains("```"))
            {
                jsonResponse = jsonResponse.Substring(jsonResponse.IndexOf("```") + 3);
                jsonResponse = jsonResponse.Substring(0, jsonResponse.LastIndexOf("```"));
            }
            jsonResponse = jsonResponse.Trim();
            
            // Parse the JSON response
            var validationResult = JsonSerializer.Deserialize<ValidationResult>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ValidationResult { IsValid = false, Summary = "Failed to parse validation response" };
            
            validationResult.ValidatedAt = DateTime.UtcNow;
            validationResult.RawLlmResponse = rawResponse;
            return validationResult;
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                IsValid = false,
                ConfidenceScore = 0,
                Summary = $"Validation failed: {ex.Message}",
                Issues = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Type = "Error",
                        Field = "System",
                        Description = ex.Message,
                        Severity = "High"
                    }
                }
            };
        }
    }
    
    public async Task<BatchValidationResult> ValidateBatchAsync(
        IEnumerable<ValidationRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DocumentValidationResult>();
        var tasks = new List<Task<DocumentValidationResult>>();
        
        foreach (var request in requests)
        {
            tasks.Add(ValidateSingleDocumentAsync(request, cancellationToken));
        }
        
        var completedResults = await Task.WhenAll(tasks);
        results.AddRange(completedResults);
        
        var validCount = results.Count(r => r.ValidationResult.IsValid);
        var avgConfidence = results.Any() 
            ? results.Average(r => r.ValidationResult.ConfidenceScore) 
            : 0;
        
        return new BatchValidationResult
        {
            TotalDocuments = results.Count,
            ValidDocuments = validCount,
            InvalidDocuments = results.Count - validCount,
            AverageConfidenceScore = avgConfidence,
            Results = results,
            CompletedAt = DateTime.UtcNow
        };
    }
    
    private async Task<DocumentValidationResult> ValidateSingleDocumentAsync(
        ValidationRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await ValidateDocumentOutputAsync(
            request.OriginalDocumentPath,
            request.TemplatePath,
            request.GeneratedOutputPath,
            cancellationToken);
        
        return new DocumentValidationResult
        {
            DocumentPath = request.OriginalDocumentPath,
            TemplateName = Path.GetFileName(request.TemplatePath),
            ValidationResult = validationResult
        };
    }
}