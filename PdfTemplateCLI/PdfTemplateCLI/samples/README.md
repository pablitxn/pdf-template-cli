# Sample Files for Testing

This directory contains sample files for testing the PDF Template CLI application.

## Directories

- **documents/**: Input documents in various formats (PDF, Word, Text, Images)
- **templates/**: Template files that define the desired structure
- **output/**: Directory for saving normalized documents

## Usage Examples

1. **Using predefined template:**
   ```
   Document path: samples/documents/sample-contract.txt
   Template: legal-contract
   Output: samples/output/normalized-contract.pdf
   ```

2. **Using custom template file:**
   ```
   Document path: samples/documents/sample-contract.txt
   Template: samples/templates/custom-template.txt
   Output: samples/output/normalized-custom.docx
   ```

## Supported Output Formats

- `.pdf` - PDF document
- `.docx` - Word document
- `.html` - HTML file
- `.txt` - Plain text
- `.md` - Markdown