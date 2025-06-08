using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PdfTemplateCLI.Application.Interfaces;

namespace PdfTemplateCLI.Infrastructure.AI;

public class SemanticKernelService : INormalizationService
{
    private readonly Kernel _kernel;
    private readonly DocumentNormalizerPlugin _plugin;

    public SemanticKernelService(IConfiguration configuration)
    {
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
        
        var model = configuration["OpenAI:Model"] ?? "gpt-4";

        builder.AddOpenAIChatCompletion(model, apiKey);
        
        _kernel = builder.Build();
        _plugin = new DocumentNormalizerPlugin();
        _kernel.Plugins.AddFromObject(_plugin, "DocumentNormalizer");
    }

    public async Task<string> NormalizeWithTemplateAsync(string content, string templateContent, CancellationToken cancellationToken = default)
    {
        // Escape the template placeholders to prevent Semantic Kernel from interpreting them as functions
        var escapedTemplate = templateContent.Replace("{{", "[[").Replace("}}", "]]");
        
        // Create a prompt that doesn't interpret template placeholders as functions
        var prompt = $@"
You are a document normalization expert. Your task is to transform the given document content to match the structure and format of the provided template.

IMPORTANT: The template contains placeholders in the format [[placeholder_name]]. You must:
1. Replace these placeholders with actual information extracted from the document
2. If information is not available in the document, use [TO BE PROVIDED] instead
3. When outputting the final document, convert [[placeholder]] back to the original format

TEMPLATE STRUCTURE:
{escapedTemplate}

ORIGINAL DOCUMENT CONTENT:
{content}

INSTRUCTIONS:
1. Analyze the template structure carefully
2. Extract relevant information from the original document
3. Fill in all [[placeholders]] with appropriate information from the document
4. Maintain the exact structure and formatting of the template
5. Ensure all sections from the template are present
6. Keep the language professional and consistent
7. Output ONLY the filled template, no explanations

Please provide the normalized document following the template structure exactly.";

        try
        {
            // Use ExecutionSettings to disable function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.3,
                MaxTokens = 4000
            };
            
            var result = await _kernel.InvokePromptAsync<string>(
                prompt, 
                new KernelArguments(executionSettings), 
                cancellationToken: cancellationToken);
            
            return result ?? "Normalization failed";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to normalize document: {ex.Message}", ex);
        }
    }
}