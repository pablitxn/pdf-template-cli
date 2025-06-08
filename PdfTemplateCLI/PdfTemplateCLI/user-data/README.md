# User Data Directory

This directory contains unformatted documents that need to be processed and formatted using templates.

## Structure

- **documents/** - Text documents requiring formatting (contracts, letters, notes)
- **images/** - Scanned documents or images containing text
- **reports/** - Unstructured reports and summaries

## Sample Documents

The directory includes example unformatted documents such as:
- Messy contracts without proper structure
- Invoices with inconsistent formatting
- Medical notes in paragraph form
- Sales reports without tables or sections
- Project updates in plain text

## Processing Workflow

1. Place your unformatted document in the appropriate subfolder
2. Run the PDF Template CLI tool
3. Select or let the AI suggest an appropriate template
4. Review the formatted output

## Supported Formats

Input formats:
- .txt - Plain text files
- .pdf - PDF documents (text will be extracted)
- .doc/.docx - Word documents
- .jpg/.png - Images (text will be extracted via OCR)

Output formats:
- .pdf - Professionally formatted PDF
- .docx - Editable Word document
- .html - Web-friendly format
- .txt - Plain text with structure