using E2E.TestValidation;

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

// Don't wait for key when running in automated mode