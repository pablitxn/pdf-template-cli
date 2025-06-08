# Templates Directory

This directory contains pre-formatted document templates that can be used to normalize and format unstructured documents.

## Structure

- **legal/** - Legal document templates (contracts, powers of attorney, rental agreements)
- **medical/** - Medical templates (reports, prescriptions, patient forms)
- **business/** - Business templates (invoices, quotations, proposals)
- **technical/** - Technical templates (bug reports, project proposals, documentation)
- **educational/** - Educational templates (certificates, report cards, transcripts)

## Template Format

All templates use placeholder syntax: `{{field_name}}`

Example:
```
Dear {{customer_name}},
Your invoice for {{amount}} is due on {{due_date}}.
```

## Usage

1. Select an appropriate template based on your document type
2. The AI will analyze your unstructured document
3. Extract relevant information and map it to template fields
4. Generate a properly formatted document

## Adding New Templates

To add a new template:
1. Create a .txt file in the appropriate category folder
2. Use {{field_name}} syntax for variable fields
3. Keep formatting consistent with existing templates