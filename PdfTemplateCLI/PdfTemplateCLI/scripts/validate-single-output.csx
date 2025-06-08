#!/usr/bin/env dotnet-script

if (Args.Count < 3)
{
    Console.WriteLine("Usage: validate-single-output.csx <original-doc> <template> <generated-output>");
    Console.WriteLine("Example: validate-single-output.csx user-data/documents/messy-invoice.txt templates/business/invoice.txt output.pdf");
    return;
}

var originalDoc = Args[0];
var template = Args[1];
var generatedOutput = Args[2];

Console.WriteLine($"Validating output...");
Console.WriteLine($"Original: {originalDoc}");
Console.WriteLine($"Template: {template}");
Console.WriteLine($"Output: {generatedOutput}");

// Simple validation logic for quick testing
var validationChecks = new List<(string check, bool passed, string details)>();

// Check 1: Files exist
validationChecks.Add(("Original document exists", File.Exists(originalDoc), originalDoc));
validationChecks.Add(("Template exists", File.Exists(template), template));
validationChecks.Add(("Generated output exists", File.Exists(generatedOutput), generatedOutput));

// Check 2: Output file size
if (File.Exists(generatedOutput))
{
    var fileInfo = new FileInfo(generatedOutput);
    var sizeOk = fileInfo.Length > 1024; // At least 1KB
    validationChecks.Add(("Output file has content", sizeOk, $"Size: {fileInfo.Length} bytes"));
}

// Check 3: Template placeholders (basic check)
if (File.Exists(template) && File.Exists(generatedOutput))
{
    var templateContent = File.ReadAllText(template);
    var placeholderCount = System.Text.RegularExpressions.Regex.Matches(templateContent, @"\{\{(\w+)\}\}").Count;
    validationChecks.Add(("Template has placeholders", placeholderCount > 0, $"Found {placeholderCount} placeholders"));
}

// Display results
Console.WriteLine("\n=== VALIDATION RESULTS ===");
foreach (var (check, passed, details) in validationChecks)
{
    var status = passed ? "✓" : "✗";
    var color = passed ? ConsoleColor.Green : ConsoleColor.Red;
    
    Console.ForegroundColor = color;
    Console.Write($"{status} ");
    Console.ResetColor();
    Console.WriteLine($"{check} - {details}");
}

var allPassed = validationChecks.All(c => c.passed);
Console.WriteLine($"\nOverall: {(allPassed ? "PASSED" : "FAILED")}");

// Generate simple JSON report
var report = new
{
    timestamp = DateTime.UtcNow,
    files = new { originalDoc, template, generatedOutput },
    checks = validationChecks.Select(c => new { c.check, c.passed, c.details }),
    overallResult = allPassed ? "PASSED" : "FAILED"
};

var reportPath = $"validation-{DateTime.Now:yyyyMMdd-HHmmss}.json";
File.WriteAllText(reportPath, System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"\nReport saved to: {reportPath}");