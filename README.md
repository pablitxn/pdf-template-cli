# PDF Template CLI

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

A powerful command-line tool that uses AI to transform unstructured documents into professionally formatted PDFs using customizable templates.

## 🚀 Features

- **AI-Powered Document Normalization**: Uses OpenAI GPT-4 to intelligently extract and structure information
- **Multi-Format Support**: 
  - **Input**: PDF, Word (DOC/DOCX/RTF/ODT), Images (JPG/PNG/BMP/GIF/TIFF), Text files
  - **Output**: PDF, Word, HTML, Text, Markdown
- **Template-Based Formatting**: Apply pre-defined or custom templates to structure documents
- **Robust Error Handling**: Domain-specific exceptions with detailed logging
- **Progress Indicators**: Visual feedback during document processing
- **Configurable**: Extensive configuration options via appsettings.json
- **Clean Architecture**: SOLID principles, dependency injection, and separation of concerns

## 📋 Prerequisites

- .NET 9.0 SDK or later
- OpenAI API key
- Aspose licenses (evaluation versions work for testing)

## 🛠️ Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/PdfTemplateCLI.git
cd PdfTemplateCLI/PdfTemplateCLI/PdfTemplateCLI
```

2. Configure your OpenAI API key in `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY",
    "Model": "gpt-4"
  }
}
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## 📁 Project Structure

```
PdfTemplateCLI/
├── src/
│   ├── Domain/              # Core business entities and rules
│   ├── Application/         # Business logic and interfaces
│   ├── Infrastructure/      # External integrations (AI, Aspose)
│   └── Presentation/        # Console UI
├── templates/               # Document templates by category
│   ├── legal/              # Legal documents (contracts, agreements)
│   ├── medical/            # Medical forms and reports
│   ├── business/           # Invoices, quotations, proposals
│   ├── technical/          # Bug reports, project documentation
│   └── educational/        # Certificates, report cards
├── user-data/              # Input documents to process
│   ├── documents/          # Text documents
│   ├── images/            # Scanned documents
│   └── reports/           # Unstructured reports
└── output/                 # Generated documents
```

## 🎯 Usage

### Interactive Mode

Simply run the application and follow the menu:

```bash
dotnet run
```

Menu options:
1. **Normalize a document** - Process a document with a template
2. **List available templates** - View pre-defined templates
3. **View document history** - See processed documents
4. **Exit**

### Document Processing Workflow

1. **Select your document**: Provide the path to your unstructured document
2. **Choose a template**: Use a pre-defined template name or provide a custom template path
3. **Specify output** (optional): Choose output format and location
4. **Review results**: Document will be processed and saved

### Example

```
Enter document path: user-data/documents/messy-invoice.txt
Enter template name: business/invoice.txt
Enter output path: output/invoice-normalized.pdf
```

## 📝 Templates

### Pre-defined Templates

The system includes various templates for common document types:

- **Legal**: Contracts, rental agreements, power of attorney
- **Medical**: Medical reports, prescriptions
- **Business**: Invoices, quotations, proposals
- **Technical**: Bug reports, project proposals
- **Educational**: Certificates, report cards

### Template Format

Templates use placeholder syntax for dynamic fields:

```
INVOICE

Company: {{company_name}}
Date: {{invoice_date}}
Invoice #: {{invoice_number}}

Bill To:
{{customer_name}}
{{customer_address}}

Items:
{{item_list}}

Total: {{total_amount}}
```

### Custom Templates

You can create your own templates:
1. Create a text file with your desired format
2. Use `{{field_name}}` for variable fields
3. Save it anywhere and provide the full path when processing

## ⚙️ Configuration

Edit `appsettings.json` to customize behavior:

```json
{
  "Application": {
    "DefaultOutputDirectory": "output",
    "MaxFileSizeMB": 50,
    "ProcessingTimeoutSeconds": 120,
    "EnableDetailedLogging": false,
    "AutoOpenGeneratedFiles": false,
    "AllowedFileExtensions": [
      ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt",
      ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff"
    ],
    "DocumentProcessing": {
      "PreserveFormatting": true,
      "ExtractImages": false,
      "MaxConcurrentDocuments": 3,
      "TemplateCacheDirectory": "cache/templates"
    }
  }
}
```

## 🔍 Validation System

The tool includes a comprehensive validation system:

- **Input Validation**: File size, format, and content checks
- **Output Validation**: AI-powered verification of generated documents
- **Batch Testing**: Automated validation of multiple documents

Run validation tests:
```bash
dotnet run TestValidation/Program.cs
```

## 🏗️ Architecture

The project follows Clean Architecture principles:

- **Domain Layer**: Core business logic, entities, and domain rules
- **Application Layer**: Use cases, DTOs, and service interfaces
- **Infrastructure Layer**: External dependencies (Aspose, OpenAI, file system)
- **Presentation Layer**: Console UI and user interaction

### Key Technologies

- **.NET 9.0**: Latest C# features and performance improvements
- **Semantic Kernel**: AI orchestration and OpenAI integration
- **Aspose Suite**: Professional document processing
- **Serilog**: Structured logging
- **Dependency Injection**: Clean, testable architecture

## 📊 Logging

The application uses Serilog for comprehensive logging:

- Console output with colored, structured logs
- Daily rotating file logs in the `logs/` directory
- Configurable log levels
- Performance metrics and error tracking

## 🚦 Error Handling

Robust error handling with custom domain exceptions:

- `DocumentNotFoundException`: When input files don't exist
- `TemplateNotFoundException`: Missing templates
- `InvalidDocumentFormatException`: Unsupported file types
- `DocumentProcessingException`: Processing failures
- `NormalizationException`: AI service errors

## 🧪 Testing

The project includes:
- Unit tests for domain logic
- Integration tests for document processing
- Validation framework for output verification

Run tests:
```bash
dotnet test
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Aspose](https://www.aspose.com/) for document processing capabilities
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) for AI orchestration
- [OpenAI](https://openai.com/) for GPT-4 API
- [Serilog](https://serilog.net/) for structured logging

## 📧 Support

For issues, questions, or contributions, please:
- Open an issue on GitHub
- Check existing documentation
- Review the [samples](samples/) directory for examples

---

Made with ❤️ using .NET and AI