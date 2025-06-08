namespace PdfTemplateCLI.Domain.Exceptions;

public class TemplateNotFoundException : DomainException
{
    public TemplateNotFoundException(string templateName) 
        : base($"Template not found: {templateName}", "TEMPLATE_NOT_FOUND")
    {
    }
}

public class InvalidTemplateException : DomainException
{
    public InvalidTemplateException(string reason) 
        : base($"Invalid template: {reason}", "INVALID_TEMPLATE")
    {
    }
}

public class TemplatePlaceholderException : DomainException
{
    public TemplatePlaceholderException(string placeholder) 
        : base($"Failed to replace placeholder: {placeholder}", "PLACEHOLDER_ERROR")
    {
    }
}