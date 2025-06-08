using Aspose.Pdf;
using Aspose.Words;
using Aspose.Words.Saving;
using Aspose.Words.Fields;
using Aspose.Words.Replacing;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Enums;
using Document = Aspose.Words.Document;
using PdfSaveOptions = Aspose.Words.Saving.PdfSaveOptions;
using System.Text.RegularExpressions;

namespace PdfTemplateCLI.Infrastructure.DocumentProcessing;

public class AsposeDocumentWriter : IDocumentWriter
{
    public async Task SaveDocumentAsync(string content, string outputPath, DocumentType outputType, CancellationToken cancellationToken = default)
    {
        await Task.Run(() =>
        {
            switch (outputType)
            {
                case DocumentType.Pdf:
                    SaveAsPdf(content, outputPath);
                    break;
                case DocumentType.Word:
                    SaveAsWord(content, outputPath);
                    break;
                case DocumentType.Html:
                    SaveAsHtml(content, outputPath);
                    break;
                case DocumentType.Text:
                    SaveAsText(content, outputPath);
                    break;
                case DocumentType.Markdown:
                    SaveAsMarkdown(content, outputPath);
                    break;
                default:
                    throw new NotSupportedException($"Output type {outputType} is not supported");
            }
        }, cancellationToken);
    }

    private void SaveAsPdf(string content, string outputPath)
    {
        // Create a Word document first, then convert to PDF for better formatting
        var doc = new Document();
        var builder = new DocumentBuilder(doc);
        
        // Parse and add content with formatting
        AddFormattedContent(builder, content);
        
        // Save as PDF with options
        var saveOptions = new PdfSaveOptions
        {
            Compliance = PdfCompliance.Pdf17,
            OptimizeOutput = true
        };
        
        doc.Save(outputPath, saveOptions);
    }

    private void SaveAsWord(string content, string outputPath)
    {
        var doc = new Document();
        var builder = new DocumentBuilder(doc);
        
        AddFormattedContent(builder, content);
        
        // Determine format based on extension
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        var saveFormat = extension switch
        {
            ".docx" => Aspose.Words.SaveFormat.Docx,
            ".doc" => Aspose.Words.SaveFormat.Doc,
            ".rtf" => Aspose.Words.SaveFormat.Rtf,
            ".odt" => Aspose.Words.SaveFormat.Odt,
            _ => Aspose.Words.SaveFormat.Docx
        };
        
        doc.Save(outputPath, saveFormat);
    }

    private void SaveAsHtml(string content, string outputPath)
    {
        var doc = new Document();
        var builder = new DocumentBuilder(doc);
        
        AddFormattedContent(builder, content);
        
        var saveOptions = new Aspose.Words.Saving.HtmlSaveOptions
        {
            PrettyFormat = true,
            ExportImagesAsBase64 = true
        };
        
        doc.Save(outputPath, saveOptions);
    }

    private void SaveAsText(string content, string outputPath)
    {
        File.WriteAllText(outputPath, content);
    }

    private void SaveAsMarkdown(string content, string outputPath)
    {
        // For markdown, we'll save as text with .md extension
        File.WriteAllText(outputPath, content);
    }

    private void AddFormattedContent(DocumentBuilder builder, string content)
    {
        // Check if content contains template placeholders (e.g., {{field}})
        if (Regex.IsMatch(content, @"\{\{.*?\}\}"))
        {
            ProcessTemplateContent(builder, content);
            return;
        }
        
        // Split content into paragraphs and process each
        var paragraphs = content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var paragraph in paragraphs)
        {
            // Check for headers (lines starting with #)
            if (paragraph.TrimStart().StartsWith("#"))
            {
                var headerLevel = paragraph.TakeWhile(c => c == '#').Count();
                var headerText = paragraph.TrimStart('#').Trim();
                
                switch (headerLevel)
                {
                    case 1:
                        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading1;
                        break;
                    case 2:
                        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading2;
                        break;
                    case 3:
                        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Heading3;
                        break;
                    default:
                        builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
                        break;
                }
                
                builder.Writeln(headerText);
                builder.ParagraphFormat.StyleIdentifier = StyleIdentifier.Normal;
            }
            // Check for bullet points
            else if (paragraph.TrimStart().StartsWith("-") || paragraph.TrimStart().StartsWith("*"))
            {
                builder.ListFormat.ApplyBulletDefault();
                var lines = paragraph.Split('\n');
                foreach (var line in lines)
                {
                    var bulletText = line.TrimStart('-', '*', ' ');
                    builder.Writeln(bulletText);
                }
                builder.ListFormat.RemoveNumbers();
            }
            // Check for numbered lists
            else if (System.Text.RegularExpressions.Regex.IsMatch(paragraph.TrimStart(), @"^\d+\."))
            {
                builder.ListFormat.ApplyNumberDefault();
                var lines = paragraph.Split('\n');
                foreach (var line in lines)
                {
                    var numberedText = System.Text.RegularExpressions.Regex.Replace(line.TrimStart(), @"^\d+\.\s*", "");
                    builder.Writeln(numberedText);
                }
                builder.ListFormat.RemoveNumbers();
            }
            else
            {
                // Regular paragraph
                builder.Writeln(paragraph);
            }
        }
    }
    
    private void ProcessTemplateContent(DocumentBuilder builder, string content)
    {
        // Parse the content as JSON-like structure if it contains field replacements
        var jsonMatch = Regex.Match(content, @"\{[\s\S]*\}");
        if (jsonMatch.Success)
        {
            try
            {
                // Extract field replacements from JSON content
                var fieldPattern = @"""([^""]+)""\s*:\s*""([^""]*)""";
                var matches = Regex.Matches(jsonMatch.Value, fieldPattern);
                
                var replacements = new Dictionary<string, string>();
                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        replacements[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
                
                // If we have a template content section, use it
                var templateMatch = Regex.Match(content, @"""template""\s*:\s*""([\s\S]*?)""(?=\s*[,}])");
                if (templateMatch.Success)
                {
                    var templateContent = Regex.Unescape(templateMatch.Groups[1].Value);
                    
                    // Replace placeholders in template
                    foreach (var replacement in replacements)
                    {
                        var placeholder = $"{{{{{replacement.Key}}}}}";
                        templateContent = templateContent.Replace(placeholder, replacement.Value);
                    }
                    
                    // Write the processed template content
                    var lines = templateContent.Split(new[] { "\\n", "\n" }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        builder.Writeln(line);
                    }
                }
                else
                {
                    // Just write the content as is
                    builder.Write(content);
                }
            }
            catch
            {
                // If parsing fails, just write the content as is
                builder.Write(content);
            }
        }
        else
        {
            builder.Write(content);
        }
    }
}