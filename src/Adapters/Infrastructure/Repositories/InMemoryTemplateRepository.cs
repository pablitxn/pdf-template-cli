using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Domain.Entities;

namespace PdfTemplateCLI.Infrastructure.Repositories;

public class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly List<Template> _templates = new();

    public InMemoryTemplateRepository()
    {
        SeedTemplates();
    }

    private void SeedTemplates()
    {
        _templates.Add(Template.Create(
            "legal-contract",
            @"CONTRACT AGREEMENT

This Agreement is entered into on [DATE] between:

PARTY A: [NAME]
Address: [ADDRESS]
Contact: [CONTACT]

PARTY B: [NAME]
Address: [ADDRESS]
Contact: [CONTACT]

TERMS AND CONDITIONS:
1. [TERM 1]
2. [TERM 2]
3. [TERM 3]

PAYMENT TERMS:
[PAYMENT DETAILS]

DURATION:
This agreement shall commence on [START DATE] and terminate on [END DATE].

SIGNATURES:
Party A: _________________ Date: _______
Party B: _________________ Date: _______",
            "Standard legal contract template",
            TemplateType.Legal
        ));

        _templates.Add(Template.Create(
            "business-proposal",
            @"BUSINESS PROPOSAL

Prepared for: [CLIENT NAME]
Prepared by: [YOUR COMPANY]
Date: [DATE]

EXECUTIVE SUMMARY:
[SUMMARY]

PROJECT OVERVIEW:
[PROJECT DESCRIPTION]

OBJECTIVES:
1. [OBJECTIVE 1]
2. [OBJECTIVE 2]
3. [OBJECTIVE 3]

DELIVERABLES:
- [DELIVERABLE 1]
- [DELIVERABLE 2]
- [DELIVERABLE 3]

TIMELINE:
[TIMELINE DETAILS]

BUDGET:
[BUDGET BREAKDOWN]

NEXT STEPS:
[ACTION ITEMS]",
            "Business proposal template",
            TemplateType.Business
        ));
    }

    public Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = _templates.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(template);
    }

    public Task<Template?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var template = _templates.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(template);
    }

    public Task<IEnumerable<Template>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Template>>(_templates);
    }

    public Task<Template> AddAsync(Template template, CancellationToken cancellationToken = default)
    {
        _templates.Add(template);
        return Task.FromResult(template);
    }

    public Task UpdateAsync(Template template, CancellationToken cancellationToken = default)
    {
        var index = _templates.FindIndex(t => t.Id == template.Id);
        if (index >= 0)
        {
            _templates[index] = template;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _templates.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }
}