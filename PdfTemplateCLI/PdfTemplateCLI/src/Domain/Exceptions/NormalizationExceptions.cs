namespace PdfTemplateCLI.Domain.Exceptions;

public class NormalizationException : DomainException
{
    public NormalizationException(string message) 
        : base($"Normalization failed: {message}", "NORMALIZATION_ERROR")
    {
    }
    
    public NormalizationException(string message, Exception innerException) 
        : base($"Normalization failed: {message}", "NORMALIZATION_ERROR", innerException)
    {
    }
}

public class AiServiceException : DomainException
{
    public AiServiceException(string message, Exception innerException) 
        : base($"AI service error: {message}", "AI_SERVICE_ERROR", innerException)
    {
    }
}