namespace PdfTemplateCLI.Application.DTOs;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public double ConfidenceScore { get; set; } // 0.0 to 1.0
    public string Summary { get; set; } = string.Empty;
    public List<ValidationIssue> Issues { get; set; } = new();
    public Dictionary<string, string> ExtractedFields { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public string? RawLlmResponse { get; set; } // Raw response from the LLM for debugging
}

public class ValidationIssue
{
    public string Type { get; set; } = string.Empty; // "Missing", "Incorrect", "Formatting", etc.
    public string Field { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Low"; // Low, Medium, High
}

public class ValidationRequest
{
    public string OriginalDocumentPath { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public string GeneratedOutputPath { get; set; } = string.Empty;
    public string ExpectedOutputPath { get; set; } = string.Empty; // Optional
}

public class BatchValidationResult
{
    public int TotalDocuments { get; set; }
    public int ValidDocuments { get; set; }
    public int InvalidDocuments { get; set; }
    public double AverageConfidenceScore { get; set; }
    public List<DocumentValidationResult> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentValidationResult
{
    public string DocumentPath { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public ValidationResult ValidationResult { get; set; } = new();
}