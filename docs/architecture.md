# PDF Template CLI - Architecture Documentation

## Table of Contents

1. [Overview](#overview)
2. [Hexagonal Architecture](#hexagonal-architecture)
3. [Solution Structure](#solution-structure)
4. [Layer Responsibilities](#layer-responsibilities)
5. [Ports and Adapters](#ports-and-adapters)
6. [Core Components](#core-components)
7. [Design Patterns](#design-patterns)
8. [Dependency Flow](#dependency-flow)
9. [Testing Strategy](#testing-strategy)
10. [Error Handling](#error-handling)
11. [Configuration Management](#configuration-management)
12. [Future Extensibility](#future-extensibility)

## Overview

PDF Template CLI is built following **Hexagonal Architecture** (also known as Ports and Adapters Architecture). This architectural pattern was chosen to create a highly maintainable, testable, and flexible application that can easily adapt to changing requirements and technologies.

### Key Benefits

- **Technology Independence**: Business logic is isolated from external technologies
- **Testability**: Core logic can be tested in isolation without external dependencies
- **Flexibility**: Easy to swap implementations (e.g., different AI providers, storage solutions)
- **Clear Boundaries**: Well-defined interfaces between layers
- **Business Focus**: Domain logic remains pure and focused on business rules

## Hexagonal Architecture

The hexagon represents the application core, with ports (interfaces) defining the boundaries and adapters implementing the connections to the external world.

```
                    ┌─────────────────────────────┐
                    │      Console CLI            │
                    │       (Adapter)             │
                    └─────────────┬───────────────┘
                                  │
                    ┌─────────────▼───────────────┐
                    │         Application         │
                    │      (Use Cases/Ports)      │
                    │  ┌───────────────────────┐  │
                    │  │       Domain          │  │
                    │  │   (Business Logic)    │  │
                    │  └───────────────────────┘  │
                    └─────┬───────────────┬───────┘
                          │               │
                ┌─────────▼───┐     ┌─────▼────────┐
                │ Infrastructure│     │Infrastructure│
                │  (AI Adapter) │     │(File Adapter)│
                └──────────────┘     └──────────────┘
```

## Solution Structure

```
pdf-template-cli/
├── src/
│   ├── Core/                               # The Hexagon (Core Business)
│   │   ├── Domain/                         # Enterprise Business Rules
│   │   │   ├── Common/
│   │   │   │   └── Result.cs              # Functional error handling
│   │   │   ├── Entities/
│   │   │   │   ├── Document.cs            # Core business entity
│   │   │   │   └── Template.cs            # Template entity
│   │   │   ├── Enums/
│   │   │   │   └── DocumentType.cs        # Document type enumeration
│   │   │   ├── Exceptions/                # Domain-specific exceptions
│   │   │   │   ├── DomainException.cs     # Base exception
│   │   │   │   ├── DocumentExceptions.cs  # Document-related exceptions
│   │   │   │   ├── TemplateExceptions.cs  # Template-related exceptions
│   │   │   │   └── NormalizationExceptions.cs
│   │   │   └── ValueObjects/
│   │   │       └── FilePath.cs            # Immutable value object
│   │   │
│   │   └── Application/                    # Application Business Rules
│   │       ├── Configuration/
│   │       │   └── ApplicationOptions.cs   # Configuration DTOs
│   │       ├── DTOs/                       # Data Transfer Objects
│   │       │   ├── DocumentDto.cs
│   │       │   ├── NormalizeDocumentRequest.cs
│   │       │   ├── TemplateDto.cs
│   │       │   ├── ValidationResult.cs
│   │       │   ├── ValidationRequest.cs
│   │       │   └── BatchValidationResult.cs
│   │       ├── Interfaces/                 # Ports (Driven)
│   │       │   ├── IDocumentReader.cs      # Port for reading documents
│   │       │   ├── IDocumentRepository.cs  # Port for data persistence
│   │       │   ├── IDocumentService.cs     # Port for use cases
│   │       │   ├── IDocumentValidator.cs   # Port for validation
│   │       │   ├── IDocumentWriter.cs      # Port for writing documents
│   │       │   ├── INormalizationService.cs # Port for AI normalization
│   │       │   ├── IOutputValidator.cs     # Port for output validation
│   │       │   └── ITemplateRepository.cs  # Port for template storage
│   │       └── Services/                   # Use Case Implementations
│   │           ├── DocumentService.cs      # Main document use cases
│   │           └── DocumentValidator.cs    # Validation use cases
│   │
│   └── Adapters/                           # External World (Outside Hexagon)
│       ├── Infrastructure/                 # Driven Adapters
│       │   ├── AI/                        # AI Integration Adapters
│       │   │   ├── DocumentNormalizerPlugin.cs
│       │   │   ├── DocumentValidatorPlugin.cs
│       │   │   └── SemanticKernelService.cs # Semantic Kernel adapter
│       │   ├── DocumentProcessing/         # Document I/O Adapters
│       │   │   ├── AsposeDocumentWriter.cs # Aspose writing adapter
│       │   │   ├── AsposeImageReader.cs    # Image reading adapter
│       │   │   ├── AsposePdfReader.cs      # PDF reading adapter
│       │   │   ├── AsposeTemplateReader.cs # Template reading adapter
│       │   │   ├── AsposeWordReader.cs     # Word reading adapter
│       │   │   └── CompositeDocumentReader.cs # Composite pattern
│       │   ├── Repositories/              # Persistence Adapters
│       │   │   ├── InMemoryDocumentRepository.cs
│       │   │   └── InMemoryTemplateRepository.cs
│       │   └── Validation/                # Validation Adapters
│       │       └── SemanticKernelOutputValidator.cs
│       │
│       └── CLI/                           # Driving Adapter (UI)
│           ├── Program.cs                 # Entry point & DI setup
│           ├── ConsoleHandler.cs          # Console command handling
│           ├── ConsoleHelper.cs           # Console UI utilities
│           └── appsettings.json          # Configuration
│
└── tests/
    ├── Unit/                              # Unit tests for Core
    ├── Integration/                       # Integration tests for Adapters
    └── E2E/                              # End-to-end tests
```

## Layer Responsibilities

### Domain Layer (Inner Hexagon Core)
**Location**: `src/Core/Domain`

The heart of the application containing pure business logic with zero external dependencies.

**Responsibilities**:
- Define core business entities (Document, Template)
- Encapsulate business rules and invariants
- Define value objects for type safety
- Define domain-specific exceptions
- Provide domain services for complex operations

**Example**:
```csharp
public class Document
{
    private Document(string fileName, string content, DocumentType type)
    {
        Id = Guid.NewGuid();
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        OriginalContent = content ?? throw new ArgumentNullException(nameof(content));
        Type = type;
        Status = DocumentStatus.Created;
        CreatedAt = DateTime.UtcNow;
    }

    public static Document Create(string fileName, string content, DocumentType type)
    {
        // Business rule: File name must have an extension
        if (!Path.HasExtension(fileName))
            throw new InvalidDocumentFormatException("Document must have a file extension");

        return new Document(fileName, content, type);
    }

    public void SetNormalizedContent(string normalizedContent)
    {
        // Business rule: Can only normalize if not already processed
        if (Status != DocumentStatus.Created)
            throw new DocumentProcessingException("Document has already been processed");

        NormalizedContent = normalizedContent;
        Status = DocumentStatus.Normalized;
        ProcessedAt = DateTime.UtcNow;
    }
}
```

### Application Layer (Hexagon Boundary)
**Location**: `src/Core/Application`

Orchestrates the application flow and defines the ports (interfaces) that adapters must implement.

**Responsibilities**:
- Define use cases as application services
- Define ports (interfaces) for external dependencies
- Orchestrate domain objects to fulfill use cases
- Transform data between domain and external world (DTOs)
- Implement application-specific business rules

**Port Example**:
```csharp
// This is a PORT - defines what the application needs from the outside world
public interface IDocumentReader
{
    Task<bool> IsSupportedAsync(string filePath);
    Task<string> ReadDocumentAsync(string filePath);
    DocumentType GetDocumentType();
}

// Another PORT for AI normalization
public interface INormalizationService
{
    Task<string> NormalizeDocumentAsync(
        string content, 
        string templateContent, 
        CancellationToken cancellationToken = default);
}
```

**Use Case Example**:
```csharp
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly INormalizationService _normalizationService;
    private readonly IDocumentReader _documentReader;
    private readonly IDocumentWriter _documentWriter;

    public async Task<DocumentDto> NormalizeDocumentAsync(NormalizeDocumentRequest request)
    {
        // 1. Read the document using the appropriate adapter
        var content = await _documentReader.ReadDocumentAsync(request.DocumentPath);
        
        // 2. Get the template
        var template = await _templateRepository.GetByNameAsync(request.TemplateName);
        
        // 3. Create domain entity
        var document = Document.Create(Path.GetFileName(request.DocumentPath), content, type);
        
        // 4. Normalize using AI adapter
        var normalizedContent = await _normalizationService.NormalizeDocumentAsync(
            content, template.Content);
        
        // 5. Update domain entity
        document.SetNormalizedContent(normalizedContent);
        
        // 6. Save and return DTO
        await _documentRepository.AddAsync(document);
        return DocumentDto.FromEntity(document);
    }
}
```

### Infrastructure Layer (Adapters)
**Location**: `src/Adapters/Infrastructure`

Implements the ports defined by the application layer, connecting to external systems.

**Adapter Categories**:

1. **AI Adapters** - Connect to AI services
   ```csharp
   public class SemanticKernelService : INormalizationService
   {
       // Implements the port using Semantic Kernel
       public async Task<string> NormalizeDocumentAsync(...)
       {
           // Actual implementation using Semantic Kernel
       }
   }
   ```

2. **Document Processing Adapters** - Handle file I/O
   ```csharp
   public class AsposePdfReader : IDocumentReader
   {
       // Implements the port using Aspose.PDF
       public async Task<string> ReadDocumentAsync(string filePath)
       {
           // Actual PDF reading implementation
       }
   }
   ```

3. **Repository Adapters** - Handle data persistence
   ```csharp
   public class InMemoryDocumentRepository : IDocumentRepository
   {
       // Implements the port using in-memory storage
   }
   ```

### CLI Layer (Driving Adapter)
**Location**: `src/Adapters/CLI`

The user interface adapter that drives the application.

**Responsibilities**:
- Handle user input/output
- Configure dependency injection
- Map user commands to application use cases
- Present results to users

## Ports and Adapters

### Ports (Interfaces)

Ports define the contracts between the hexagon and the outside world:

**Driven Ports** (Application → Infrastructure):
- `IDocumentReader` - Reading various document formats
- `IDocumentWriter` - Writing documents in different formats
- `IDocumentRepository` - Persisting documents
- `ITemplateRepository` - Managing templates
- `INormalizationService` - AI normalization
- `IOutputValidator` - Validating generated documents

**Driving Ports** (CLI → Application):
- `IDocumentService` - Main document operations
- `IDocumentValidator` - Validation operations

### Adapters (Implementations)

**Driven Adapters** (Infrastructure):
- `SemanticKernelService` → `INormalizationService`
- `AsposePdfReader` → `IDocumentReader`
- `AsposeDocumentWriter` → `IDocumentWriter`
- `InMemoryDocumentRepository` → `IDocumentRepository`

**Driving Adapters** (CLI):
- `ConsoleHandler` - Processes console commands
- `Program.cs` - Configures DI and starts application

## Core Components

### Document Processing Pipeline

```
┌─────────┐    ┌────────┐    ┌──────────────┐    ┌──────────┐    ┌────────┐    ┌────────┐
│  Input  │───▶│ Reader │───▶│Normalization │───▶│ Template │───▶│ Writer │───▶│ Output │
│Document │    │Adapter │    │   Service    │    │Application│   │Adapter │    │Document│
└─────────┘    └────────┘    └──────────────┘    └──────────┘    └────────┘    └────────┘
```

### Composite Document Reader

Uses the Composite pattern to support multiple document formats:

```csharp
public class CompositeDocumentReader : IDocumentReader
{
    private readonly IEnumerable<IDocumentReader> _readers;

    public async Task<string> ReadDocumentAsync(string filePath)
    {
        var supportedReader = _readers.FirstOrDefault(r => 
            r.IsSupportedAsync(filePath).Result);
            
        if (supportedReader == null)
            throw new InvalidDocumentFormatException($"No reader available for {filePath}");
            
        return await supportedReader.ReadDocumentAsync(filePath);
    }
}
```

## Design Patterns

### 1. Hexagonal Architecture Pattern
The overarching pattern that structures the entire application.

### 2. Repository Pattern
Abstracts data persistence behind interfaces:
```csharp
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task<IEnumerable<Document>> GetAllAsync();
    Task AddAsync(Document document);
    Task UpdateAsync(Document document);
}
```

### 3. Composite Pattern
Used in `CompositeDocumentReader` to handle multiple document formats.

### 4. Strategy Pattern
Different readers/writers act as strategies for document processing.

### 5. Result Pattern
Functional error handling without exceptions:
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

### 6. Dependency Injection
Used throughout for loose coupling and testability.

### 7. Options Pattern
For strongly-typed configuration:
```csharp
services.Configure<ApplicationOptions>(
    configuration.GetSection(ApplicationOptions.SectionName));
```

## Dependency Flow

### Inward Dependencies Only

```
External World → Adapters → Application → Domain
                    ↓           ↓           ↓
                   Port      Use Case    Pure Logic
                (Interface)  (Service)   (No Dependencies)
```

**Key Rules**:
1. Domain has NO external dependencies
2. Application depends only on Domain
3. Infrastructure depends on Application (implements ports)
4. CLI depends on Application (uses ports)
5. Dependencies always point inward

### Dependency Injection Configuration

```csharp
// In Program.cs
services.AddScoped<IDocumentService, DocumentService>();
services.AddScoped<IDocumentReader, CompositeDocumentReader>();
services.AddScoped<INormalizationService, SemanticKernelService>();
services.AddScoped<IDocumentRepository, InMemoryDocumentRepository>();
```

## Testing Strategy

### Unit Tests (Core)
Test domain logic and application services in isolation:

```csharp
[Fact]
public void Document_Create_WithInvalidFileName_ThrowsException()
{
    // Arrange & Act & Assert
    Assert.Throws<InvalidDocumentFormatException>(() => 
        Document.Create("fileWithoutExtension", "content", DocumentType.Text));
}
```

### Integration Tests (Adapters)
Test adapter implementations:

```csharp
[Fact]
public async Task AsposePdfReader_ReadDocument_ReturnsContent()
{
    // Test actual PDF reading with Aspose
}
```

### E2E Tests
Test complete workflows:

```csharp
[Fact]
public async Task NormalizeDocument_CompleteWorkflow_Success()
{
    // Test from console input to final output
}
```

## Error Handling

### Domain Exceptions
```
DomainException (Base)
├── DocumentNotFoundException
├── InvalidDocumentFormatException
├── DocumentProcessingException
├── TemplateNotFoundException
└── NormalizationException
```

### Error Flow
1. Domain throws specific exceptions
2. Application catches and logs
3. Infrastructure wraps external exceptions
4. CLI presents user-friendly messages

## Configuration Management

### Configuration Structure
```json
{
  "Application": {
    "DefaultOutputDirectory": "output",
    "MaxFileSizeMB": 50
  },
  "SemanticKernel": {
    "Provider": "OpenAI",
    "ApiKey": "{{API_KEY}}",
    "Model": "gpt-4"
  }
}
```

### Configuration Flow
1. CLI loads configuration
2. Passes to DI container
3. Injected into services via Options pattern

## Future Extensibility

### Adding New Document Format

1. Create new reader implementing `IDocumentReader`:
```csharp
public class ExcelReader : IDocumentReader
{
    public Task<bool> IsSupportedAsync(string filePath) => 
        Task.FromResult(Path.GetExtension(filePath) == ".xlsx");
        
    public async Task<string> ReadDocumentAsync(string filePath)
    {
        // Excel reading logic
    }
}
```

2. Register in DI:
```csharp
services.AddScoped<IDocumentReader, ExcelReader>();
```

No changes needed in core business logic!

### Adding New AI Provider

1. Create new implementation of `INormalizationService`:
```csharp
public class AnthropicService : INormalizationService
{
    public async Task<string> NormalizeDocumentAsync(...)
    {
        // Anthropic API implementation
    }
}
```

2. Replace registration in DI:
```csharp
services.AddScoped<INormalizationService, AnthropicService>();
```

### Adding Database Persistence

1. Create new repository implementation:
```csharp
public class SqlDocumentRepository : IDocumentRepository
{
    // SQL implementation
}
```

2. Replace in-memory with SQL:
```csharp
services.AddScoped<IDocumentRepository, SqlDocumentRepository>();
```

### Adding Web API

1. Create new Web API project
2. Reference Application project
3. Create controllers that use `IDocumentService`
4. No changes to business logic required!

## Conclusion

The Hexagonal Architecture provides PDF Template CLI with a robust, maintainable, and extensible foundation. The clear separation between business logic and technical details ensures that the application can evolve with changing requirements while maintaining its core integrity. The architecture makes it easy to test, modify, and extend the application without affecting existing functionality.