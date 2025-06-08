using PdfTemplateCLI.TestValidation;

Console.WriteLine("PDF Template CLI - Validation Test Runner");
Console.WriteLine("========================================\n");

try
{
    var runner = new ValidationTestRunner();
    await runner.RunAllTestsAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error running validation tests: {ex.Message}");
    Environment.Exit(1);
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();