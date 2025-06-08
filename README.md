# PDF Template CLI

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Architecture](https://img.shields.io/badge/Architecture-Hexagonal-orange)](docs/architecture.md)

A powerful command-line tool that uses AI to transform unstructured documents into professionally formatted PDFs using customizable templates.

## 🚀 Features

- **AI-Powered Document Normalization**: Uses Semantic Kernel to intelligently extract and structure information
- **Multi-Format Support**: 
  - **Input**: PDF, Word (DOC/DOCX/RTF/ODT), Images (JPG/PNG/BMP/GIF/TIFF), Text files
  - **Output**: PDF, Word, HTML, Text, Markdown
- **Template-Based Formatting**: Apply pre-defined or custom templates to structure documents
- **Robust Error Handling**: Domain-specific exceptions with detailed logging
- **Progress Indicators**: Visual feedback during document processing
- **Configurable**: Extensive configuration options via appsettings.json
- **Hexagonal Architecture**: Clean separation of concerns with ports and adapters pattern

## 📋 Prerequisites

- .NET 9.0 SDK or later
- API key for your preferred AI provider (OpenAI, Azure OpenAI, etc.)
- Aspose licenses (evaluation versions work for testing)

## 🛠️ Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/pdf-template-cli.git
cd pdf-template-cli
```

2. Configure your AI provider in `src/Adapters/CLI/appsettings.json`:
```json
{
  "SemanticKernel": {
    "Provider": "OpenAI",
    "ApiKey": "YOUR_API_KEY",
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
dotnet run --project src/Adapters/CLI/CLI.csproj
```

## 📁 Project Structure

```
pdf-template-cli/
├── src/
│   ├── Core/
│   │   ├── Domain/          # Core business entities, value objects and domain rules
│   │   └── Application/     # Use cases, interfaces, and application services
│   └── Adapters/
│       ├── Infrastructure/  # External integrations (AI, Aspose, File System)
│       └── CLI/            # Console presentation layer
├── tests/
│   ├── Unit/               # Unit tests for domain and application logic
│   ├── Integration/        # Integration tests for infrastructure
│   └── E2E/               # End-to-end tests with test fixtures
├── docs/                   # Documentation
│   └── architecture.md     # Detailed architecture documentation
└── scripts/               # Utility scripts for testing and validation
```

## 🎯 Usage

### Interactive Mode

Simply run the application and follow the menu:

```bash
dotnet run --project src/Adapters/CLI/CLI.csproj
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
Enter document path: tests/E2E/Fixtures/user-data/documents/messy-invoice.txt
Enter template name: business/invoice
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

Templates are located in: `tests/E2E/Fixtures/templates/`

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
3. Save it and provide the path when processing

## ⚙️ Configuration

Edit `src/Adapters/CLI/appsettings.json` to customize behavior:

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

The tool includes a comprehensive validation system powered by AI:

- **Input Validation**: File size, format, and content checks
- **Output Validation**: AI-powered verification of generated documents using GPT-4
- **Batch Testing**: Automated validation of multiple documents with fixtures
- **Confidence Scoring**: Each validation includes a confidence score (0-100%)
- **Detailed Issue Reporting**: Identifies missing fields, formatting issues, and content accuracy

### Running Validation Tests

From the project root, use the provided scripts:

**Linux/macOS:**
```bash
./run-validation-tests.sh
```

**Windows:**
```powershell
.\run-validation-tests.ps1
```

**Or run directly:**
```bash
dotnet run --project tests/E2E/E2E.csproj
```

### Validation Process

1. **Fixture Processing**: The system processes all test documents in `tests/E2E/Fixtures/user-data/documents/`
2. **Template Application**: Each document is paired with an appropriate template
3. **AI Validation**: GPT-4 analyzes the output to ensure:
   - All information from the original document is preserved
   - The template structure is properly followed
   - No hallucinations or incorrect data
   - Professional formatting is maintained
4. **Report Generation**: Creates a detailed `validation-summary.json` with:
   - Pass/fail status for each test
   - Confidence scores
   - Specific issues found
   - Recommendations for improvement

### Test Fixtures

The E2E tests include various real-world scenarios:
- **Legal**: Unformatted contracts → Legal contract templates
- **Business**: Messy invoices → Professional invoice templates  
- **Medical**: Patient notes → Medical report templates
- **Technical**: Project updates → Technical report templates

⚠️ **Note**: Ensure your OpenAI API key is configured in `src/Adapters/CLI/appsettings.json` before running validation tests.

## 🏗️ Architecture

The project follows Hexagonal Architecture (Ports and Adapters) principles:

### Core (Domain + Application)
- **Domain Layer**: Core business logic, entities, value objects, and domain exceptions
- **Application Layer**: Use cases, DTOs, service interfaces (ports), and application services

### Adapters
- **Infrastructure**: Implementations of application interfaces (adapters)
  - AI integration (Semantic Kernel)
  - Document processing (Aspose)
  - Repositories (In-memory for demo)
- **CLI**: Console presentation layer

For detailed architecture information, see [architecture.md](docs/architecture.md)

### Key Technologies

- **.NET 9.0**: Latest C# features and performance improvements
- **Microsoft Semantic Kernel**: AI orchestration and LLM integration
- **Aspose Suite**: Professional document processing
- **Dependency Injection**: Clean, testable architecture
- **xUnit + Moq + FluentAssertions**: Comprehensive testing

## 📊 Logging

The application provides comprehensive logging:

- Structured console output with progress indicators
- Configurable log levels
- Performance metrics
- Error tracking with detailed context

## 🚦 Error Handling

Robust error handling with custom domain exceptions:

- `DocumentNotFoundException`: When input files don't exist
- `TemplateNotFoundException`: Missing templates
- `InvalidDocumentFormatException`: Unsupported file types
- `DocumentProcessingException`: Processing failures
- `NormalizationException`: AI service errors

All exceptions include error codes and detailed messages for debugging.

## 🧪 Testing

The project includes comprehensive testing:

### Unit Tests
- Domain entities and value objects
- Application services
- Infrastructure components

### Integration Tests
- Document processing workflows
- AI integration
- Repository implementations

### E2E Tests
- Complete document normalization scenarios
- Template validation
- Batch processing

Run all tests:
```bash
dotnet test
```

Run specific test projects:
```bash
dotnet test tests/Unit/Unit.csproj
dotnet test tests/Integration/Integration.csproj
dotnet test tests/E2E/E2E.csproj
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- All tests pass
- Code follows the existing architecture patterns
- New features include appropriate tests

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Aspose](https://www.aspose.com/) for document processing capabilities
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) for AI orchestration
- [xUnit](https://xunit.net/) for testing framework
- [Moq](https://github.com/moq/moq4) for mocking
- [FluentAssertions](https://fluentassertions.com/) for test assertions

## 📧 Support

For issues, questions, or contributions, please:
- Open an issue on GitHub
- Check the [architecture documentation](docs/architecture.md)
- Review the test fixtures for examples

---

Made with ❤️ using .NET and AI