# PdfTemplateCLI - Document Normalizer

A CLI application that normalizes documents using Semantic Kernel and Aspose for comprehensive document processing.

## Features

- Document normalization using AI (Semantic Kernel)
- Multiple input format support via Aspose (PDF, Word, Images)
- Multiple output format support (PDF, Word, HTML, Text, Markdown)
- Template-based document transformation
- Support for custom template files
- Document history tracking
- Hexagonal architecture implementation

## Project Structure

```
PdfTemplateCLI/
├── src/                    # Source code
├── templates/              # Document templates organized by category
│   ├── legal/             # Legal document templates
│   ├── medical/           # Medical templates
│   ├── business/          # Business templates
│   ├── technical/         # Technical documentation
│   └── educational/       # Educational certificates and reports
├── user-data/             # Input documents to be processed
│   ├── documents/         # Text documents
│   ├── images/           # Scanned documents
│   └── reports/          # Unstructured reports
└── samples/               # Example files
```

## Setup

1. Add your OpenAI API key to `appsettings.json`:
```json
{
  "OpenAI": {
    "ApiKey": "YOUR_ACTUAL_API_KEY"
  }
}
```

2. Build the project:
```bash
dotnet build
```

3. Run the application:
```bash
dotnet run
```

## Plugin Name

The Semantic Kernel plugin is called **DocumentNormalizer** and contains functions for:
- Document normalization
- Information extraction
- Validation against templates

## Architecture

- **Domain**: Core business entities and rules
- **Application**: Business logic and service interfaces
- **Infrastructure**: External integrations (AI, repositories)
- **Presentation**: Console UI for user interaction

## Available Templates

### Predefined Templates
- `legal-contract`: Standard legal contract template
- `business-proposal`: Business proposal template

### Custom Templates
You can also use your own template files by providing the file path instead of a template name.

## Supported Formats

### Input Formats
- **PDF** (.pdf)
- **Word** (.doc, .docx, .rtf, .odt)
- **Images** (.jpg, .jpeg, .png, .bmp, .gif, .tiff)
- **Text** (.txt)

### Output Formats
- **PDF** (.pdf) - Professional PDF documents
- **Word** (.docx, .doc) - Microsoft Word documents
- **HTML** (.html) - Web-ready HTML files
- **Text** (.txt) - Plain text files
- **Markdown** (.md) - Markdown formatted files

## Usage

1. Choose "Normalize a document" from the menu
2. Enter the path to your document (supports PDF, Word, Images, Text)
3. Enter either:
   - A predefined template name (e.g., `legal-contract`)
   - A path to a custom template file (e.g., `samples/templates/custom-template.txt`)
4. Optionally specify an output path with the desired format extension:
   - `output.pdf` for PDF
   - `output.docx` for Word
   - `output.html` for HTML
   - Leave empty to display the result in console
5. The normalized document will be processed and saved in the specified format

## Example Usage

```bash
# Using predefined template
Document path: samples/documents/sample-contract.txt
Template: legal-contract
Output: samples/output/normalized.pdf

# Using custom template file
Document path: samples/documents/invoice.pdf
Template: samples/templates/invoice-template.txt
Output: samples/output/normalized-invoice.docx
```

## Technologies Used

- **Semantic Kernel**: AI orchestration for intelligent document processing
- **Aspose.PDF**: PDF reading and generation
- **Aspose.Words**: Word document processing and conversion
- **Aspose.Imaging**: Image file handling
- **.NET 9**: Latest framework features
- **Dependency Injection**: Clean architecture implementation

## TODO

- Add more predefined templates
- Implement OCR for better image text extraction
- Add batch processing support
- Integrate with AWS for production deployment
- Add template validation
- Support for more document formats