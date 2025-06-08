using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace PdfTemplateCLI.Infrastructure.AI;

public class DocumentNormalizerPlugin
{
    [KernelFunction]
    [Description("Normalizes a document content based on a provided template structure")]
    public async Task<string> NormalizeDocumentAsync(
        [Description("The original document content to be normalized")] string documentContent,
        [Description("The template structure that defines the desired format")] string templateStructure,
        KernelArguments? arguments = null)
    {
        var prompt = $@"
You are a document normalization expert. Your task is to transform the given document content to match the structure and format of the provided template.

TEMPLATE STRUCTURE:
{templateStructure}

ORIGINAL DOCUMENT CONTENT:
{documentContent}

INSTRUCTIONS:
1. Analyze the template structure carefully
2. Extract relevant information from the original document
3. Reorganize the content to match the template's structure
4. Maintain the same formatting style as the template
5. Ensure all required sections from the template are present
6. If information is missing, indicate it with [TO BE PROVIDED]
7. Keep the language professional and consistent

Please provide the normalized document following the template structure exactly.
";

        return prompt;
    }

    [KernelFunction]
    [Description("Extracts key information from a document")]
    public async Task<string> ExtractDocumentInfoAsync(
        [Description("The document content to analyze")] string documentContent)
    {
        var prompt = $@"
Extract and summarize the key information from the following document:

{documentContent}

Provide a structured summary including:
- Document type
- Main topic/purpose
- Key sections
- Important dates (if any)
- Parties involved (if applicable)
- Main points or requirements
";

        return prompt;
    }

    [KernelFunction]
    [Description("Validates if a normalized document matches the template requirements")]
    public async Task<string> ValidateNormalizedDocumentAsync(
        [Description("The normalized document to validate")] string normalizedDocument,
        [Description("The template structure to validate against")] string templateStructure)
    {
        var prompt = $@"
Validate if the normalized document correctly follows the template structure.

TEMPLATE:
{templateStructure}

NORMALIZED DOCUMENT:
{normalizedDocument}

Check for:
1. All required sections are present
2. Format consistency with template
3. Missing information (marked as [TO BE PROVIDED])
4. Any structural deviations

Provide a validation report with:
- Compliance score (0-100%)
- Missing sections (if any)
- Format issues (if any)
- Suggestions for improvement
";

        return prompt;
    }
}