namespace PdfTemplateCLI.Domain.ValueObjects;

public class FilePath
{
    public string Value { get; }

    public FilePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("File path cannot be empty");

        if (!Path.IsPathRooted(value))
            value = Path.GetFullPath(value);

        Value = value;
    }

    public string GetFileName() => Path.GetFileName(Value);
    public string GetDirectory() => Path.GetDirectoryName(Value) ?? string.Empty;
    public string GetExtension() => Path.GetExtension(Value);

    public override string ToString() => Value;

    public static implicit operator string(FilePath filePath) => filePath.Value;
    public static implicit operator FilePath(string value) => new(value);
}