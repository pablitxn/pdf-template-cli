using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfTemplateCLI.Application.Configuration;
using PdfTemplateCLI.Application.Interfaces;
using PdfTemplateCLI.Application.Services;
using PdfTemplateCLI.Infrastructure.AI;
using PdfTemplateCLI.Infrastructure.DocumentProcessing;
using PdfTemplateCLI.Infrastructure.Repositories;
using PdfTemplateCLI.Presentation.ConsoleUI;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/pdftemplate-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting PDF Template CLI application");
    
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    var services = new ServiceCollection();
    
    // Add logging
    services.AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddSerilog();
    });

    services.AddSingleton<IConfiguration>(configuration);
    
    // Configure and validate options
    services.AddOptions<ApplicationOptions>()
        .Bind(configuration.GetSection(ApplicationOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
    
    services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
    services.AddSingleton<ITemplateRepository, InMemoryTemplateRepository>();
    services.AddSingleton<INormalizationService, SemanticKernelService>();
    services.AddScoped<IDocumentValidator, DocumentValidator>();

// Register document readers
services.AddScoped<AsposePdfReader>();
services.AddScoped<AsposeWordReader>();
services.AddScoped<AsposeImageReader>();
services.AddScoped<AsposeTemplateReader>();
services.AddScoped<IDocumentReader>(sp =>
{
    var readers = new List<IDocumentReader>
    {
        sp.GetRequiredService<AsposePdfReader>(),
        sp.GetRequiredService<AsposeWordReader>(),
        sp.GetRequiredService<AsposeImageReader>(),
        sp.GetRequiredService<AsposeTemplateReader>()
    };
    return new CompositeDocumentReader(readers);
});

// Register document writer
services.AddScoped<IDocumentWriter, AsposeDocumentWriter>();

    services.AddScoped<IDocumentService, DocumentService>();
    services.AddScoped<ConsoleHandler>();

    var serviceProvider = services.BuildServiceProvider();
    
    // Ensure directories exist
    var appOptions = serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>().Value;
    Directory.CreateDirectory(appOptions.DefaultOutputDirectory);
    Directory.CreateDirectory(appOptions.DocumentProcessing.TemplateCacheDirectory);
    Directory.CreateDirectory("logs");

    var consoleHandler = serviceProvider.GetRequiredService<ConsoleHandler>();
    await consoleHandler.RunAsync();
    
    Log.Information("Application ended successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Console.WriteLine($"Application error: {ex.Message}");
    Environment.Exit(1);
}
finally
{
    Log.CloseAndFlush();
}
