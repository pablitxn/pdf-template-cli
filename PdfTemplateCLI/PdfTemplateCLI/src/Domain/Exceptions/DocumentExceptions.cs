namespace PdfTemplateCLI.Domain.Exceptions;

public class DocumentNotFoundException : DomainException
{
    public DocumentNotFoundException(string documentPath) 
        : base($"Document not found: {documentPath}", "DOCUMENT_NOT_FOUND")
    {
    }
}

public class InvalidDocumentFormatException : DomainException
{
    public InvalidDocumentFormatException(string format) 
        : base($"Invalid document format: {format}", "INVALID_FORMAT")
    {
    }
}

public class DocumentProcessingException : DomainException
{
    public DocumentProcessingException(string message, Exception innerException) 
        : base($"Error processing document: {message}", "PROCESSING_ERROR", innerException)
    {
    }
}

public class DocumentTooLargeException : DomainException
{
    public DocumentTooLargeException(long size, long maxSize) 
        : base($"Document size ({size} bytes) exceeds maximum allowed ({maxSize} bytes)", "FILE_TOO_LARGE")
    {
    }
}