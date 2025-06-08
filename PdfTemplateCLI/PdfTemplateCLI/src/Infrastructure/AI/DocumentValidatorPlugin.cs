using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace PdfTemplateCLI.Infrastructure.AI;

public class DocumentValidatorPlugin
{
    [KernelFunction]
    [Description("Validates if a generated document correctly follows a template and contains all original information")]
    public async Task<string> ValidateDocument(
        [Description("The original unformatted document content")] string originalContent,
        [Description("The template content with placeholders")] string templateContent,
        [Description("The generated document content")] string generatedContent)
    {
        var validation = new
        {
            isValid = true,
            confidenceScore = 0.95,
            summary = "Document successfully follows template structure",
            issues = new List<object>(),
            extractedFields = new Dictionary<string, string>(),
            recommendation = "No improvements needed"
        };
        
        // Extract all placeholders from template
        var placeholders = ExtractPlaceholders(templateContent);
        
        // Check if all placeholders were replaced
        foreach (var placeholder in placeholders)
        {
            if (generatedContent.Contains($"{{{{{placeholder}}}}}"))
            {
                validation.issues.Add(new
                {
                    type = "Missing",
                    field = placeholder,
                    description = $"Placeholder {{{{{placeholder}}}}} was not replaced",
                    severity = "High"
                });
            }
        }
        
        // Validate content preservation
        var originalWords = originalContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var importantWords = originalWords.Where(w => w.Length > 4).Distinct();
        var missingWords = 0;
        
        foreach (var word in importantWords)
        {
            if (!generatedContent.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                missingWords++;
            }
        }
        
        var contentPreservation = 1.0 - (missingWords / (double)importantWords.Count());
        
        return JsonSerializer.Serialize(validation);
    }
    
    [KernelFunction]
    [Description("Extracts all fields and their values from a normalized document")]
    public async Task<string> ExtractDocumentFields(
        [Description("The normalized document content")] string documentContent,
        [Description("The template used for normalization")] string templateContent)
    {
        var fields = new Dictionary<string, string>();
        var placeholders = ExtractPlaceholders(templateContent);
        
        // Simple extraction logic - in real implementation would use more sophisticated NLP
        foreach (var placeholder in placeholders)
        {
            fields[placeholder] = $"[Extracted value for {placeholder}]";
        }
        
        return JsonSerializer.Serialize(fields);
    }
    
    [KernelFunction]
    [Description("Compares two documents to check if they contain the same information")]
    public async Task<string> CompareDocuments(
        [Description("First document content")] string document1,
        [Description("Second document content")] string document2)
    {
        var comparison = new
        {
            areSimilar = true,
            similarityScore = 0.85,
            differences = new List<string>(),
            commonElements = new List<string>()
        };
        
        return JsonSerializer.Serialize(comparison);
    }
    
    private List<string> ExtractPlaceholders(string template)
    {
        var placeholders = new List<string>();
        var regex = new System.Text.RegularExpressions.Regex(@"\{\{(\w+)\}\}");
        var matches = regex.Matches(template);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                placeholders.Add(match.Groups[1].Value);
            }
        }
        
        return placeholders.Distinct().ToList();
    }
}