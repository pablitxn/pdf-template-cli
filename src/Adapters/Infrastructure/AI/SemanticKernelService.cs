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
        
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        var model = configuration["OpenAI:Model"] ?? "gpt-4";

        builder.AddOpenAIChatCompletion(model, apiKey);
        
        _kernel = builder.Build();
        _plugin = new DocumentNormalizerPlugin();
        _kernel.Plugins.AddFromObject(_plugin, "DocumentNormalizer");
    }

    public async Task<string> NormalizeWithTemplateAsync(string content, string templateContent, CancellationToken cancellationToken = default)
    {
        var prompt = await _plugin.NormalizeDocumentAsync(content, templateContent);
        
        var result = await _kernel.InvokePromptAsync<string>(prompt, cancellationToken: cancellationToken);
        
        return result ?? "Normalization failed";
    }
}