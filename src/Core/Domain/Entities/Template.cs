namespace PdfTemplateCLI.Domain.Entities;

public class Template
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public TemplateType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Template() { }

    public static Template Create(string name, string content, string description, TemplateType type)
    {
        return new Template
        {
            Id = Guid.NewGuid(),
            Name = name,
            Content = content,
            Description = description,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string content, string description)
    {
        Name = name;
        Content = content;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TemplateType
{
    Legal,
    Medical,
    Business,
    Technical,
    General
}