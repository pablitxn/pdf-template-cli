# PDF Template CLI - Architecture Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture Principles](#architecture-principles)
3. [Solution Structure](#solution-structure)
4. [Layer Responsibilities](#layer-responsibilities)
5. [Core Components](#core-components)
6. [Design Patterns](#design-patterns)
7. [Dependency Flow](#dependency-flow)
8. [Coding Conventions](#coding-conventions)
9. [Error Handling Strategy](#error-handling-strategy)
10. [Logging Architecture](#logging-architecture)
11. [Configuration Management](#configuration-management)
12. [Security Considerations](#security-considerations)
13. [Performance Considerations](#performance-considerations)
14. [Testing Strategy](#testing-strategy)
15. [Future Extensibility](#future-extensibility)

## Overview

PDF Template CLI is built following **Clean Architecture** principles (also known as Onion Architecture or Hexagonal Architecture). This approach ensures:

- **Independence of frameworks**: Business logic doesn't depend on external libraries
- **Testability**: Business rules can be tested without UI, database, or external services
- **Independence of UI**: The UI can change without changing the rest of the system
- **Independence of database**: Business rules are not bound to the database
- **Independence of external services**: Business rules don't know anything about the outside world

## Architecture Principles

### 1. Dependency Inversion Principle (DIP)
- High-level modules do not depend on low-level modules
- Both depend on abstractions (interfaces)
- Abstractions do not depend on details
- Details depend on abstractions

### 2. Separation of Concerns (SoC)
- Each layer has a specific responsibility
- Business logic is isolated from infrastructure concerns
- UI logic is separated from business logic

### 3. Single Responsibility Principle (SRP)
- Each class has one reason to change
- Classes are focused and cohesive

### 4. Open/Closed Principle (OCP)
- Classes are open for extension but closed for modification
- New features are added through new implementations, not by modifying existing code

## Solution Structure

```
PdfTemplateCLI/
├── PdfTemplateCLI.sln
├── docs/                                    # Documentation
├── scripts/                                 # Utility scripts
├── templates/                               # Document templates
├── user-data/                              # User input files
└── PdfTemplateCLI/
    ├── PdfTemplateCLI.csproj
    ├── Program.cs                          # Application entry point
    ├── appsettings.json                    # Configuration
    └── src/
        ├── Domain/                         # Core business logic
        │   ├── Common/                     # Shared domain concepts
        │   │   └── Result.cs              # Result pattern implementation
        │   ├── Entities/                   # Business entities
        │   │   ├── Document.cs
        │   │   └── Template.cs
        │   ├── Enums/                      # Domain enumerations
        │   │   └── DocumentType.cs
        │   ├── Exceptions/                 # Domain-specific exceptions
        │   │   ├── DomainException.cs
        │   │   ├── DocumentExceptions.cs
        │   │   ├── TemplateExceptions.cs
        │   │   └── NormalizationExceptions.cs
        │   └── ValueObjects/               # Value objects
        │       └── FilePath.cs
        │
        ├── Application/                    # Application business logic
        │   ├── Configuration/              # Application settings
        │   │   └── ApplicationOptions.cs
        │   ├── DTOs/                       # Data transfer objects
        │   │   ├── DocumentDto.cs
        │   │   ├── NormalizeDocumentRequest.cs
        │   │   ├── TemplateDto.cs
        │   │   └── ValidationResult.cs
        │   ├── Interfaces/                 # Application contracts
        │   │   ├── IDocumentReader.cs
        │   │   ├── IDocumentRepository.cs
        │   │   ├── IDocumentService.cs
        │   │   ├── IDocumentValidator.cs
        │   │   ├── IDocumentWriter.cs
        │   │   ├── INormalizationService.cs
        │   │   ├── IOutputValidator.cs
        │   │   └── ITemplateRepository.cs
        │   └── Services/                   # Application services
        │       ├── DocumentService.cs
        │       └── DocumentValidator.cs
        │
        ├── Infrastructure/                 # External concerns
        │   ├── AI/                        # AI integration
        │   │   ├── DocumentNormalizerPlugin.cs
        │   │   ├── DocumentValidatorPlugin.cs
        │   │   └── SemanticKernelService.cs
        │   ├── DocumentProcessing/         # Document I/O
        │   │   ├── AsposeDocumentWriter.cs
        │   │   ├── AsposeImageReader.cs
        │   │   ├── AsposePdfReader.cs
        │   │   ├── AsposeTemplateReader.cs
        │   │   ├── AsposeWordReader.cs
        │   │   └── CompositeDocumentReader.cs
        │   ├── Repositories/              # Data persistence
        │   │   ├── InMemoryDocumentRepository.cs
        │   │   └── InMemoryTemplateRepository.cs
        │   └── Validation/                # Output validation
        │       └── SemanticKernelOutputValidator.cs
        │
        └── Presentation/                   # User interface
            └── ConsoleUI/
                ├── ConsoleHandler.cs
                └── ConsoleHelper.cs
```

## Layer Responsibilities

### Domain Layer (Core)
**Purpose**: Contains enterprise-wide business rules and logic

**Responsibilities**:
- Define core business entities (Document, Template)
- Define value objects (FilePath)
- Define domain exceptions
- Encapsulate business rules
- No dependencies on other layers

**Key Classes**:
```csharp
// Entity example
public class Document
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public DocumentStatus Status { get; private set; }
    
    // Business logic encapsulated
    public void SetNormalizedContent(string content)
    {
        // Business rules here
    }
}

// Value Object example
public record FilePath
{
    public string Value { get; }
    
    public FilePath(string value)
    {
        // Validation logic
        Value = value;
    }
}
```

### Application Layer
**Purpose**: Contains application-specific business rules

**Responsibilities**:
- Orchestrate domain objects
- Implement use cases
- Define interfaces for external dependencies
- Data transformation (DTOs)
- Input validation

**Key Components**:
```csharp
// Service interface
public interface IDocumentService
{
    Task<DocumentDto> NormalizeDocumentAsync(NormalizeDocumentRequest request);
}

// Implementation with orchestration
public class DocumentService : IDocumentService
{
    // Orchestrates multiple dependencies to fulfill use case
}
```

### Infrastructure Layer
**Purpose**: Implements external concerns

**Responsibilities**:
- Implement interfaces defined in Application layer
- External service integration (OpenAI, Aspose)
- Data persistence
- File system operations
- Third-party library wrappers

**Key Implementations**:
- `SemanticKernelService`: OpenAI integration
- `AsposeDocumentWriter`: Document generation
- `InMemoryDocumentRepository`: Data persistence

### Presentation Layer
**Purpose**: Handle user interaction

**Responsibilities**:
- User input/output
- Input formatting
- Error presentation
- Progress reporting

## Core Components

### 1. Document Processing Pipeline

```
Input Document → Reader → Normalization → Template Application → Writer → Output
```

**Components**:
- `IDocumentReader`: Abstracts document reading
- `CompositeDocumentReader`: Implements composite pattern for multiple readers
- `INormalizationService`: AI-powered content normalization
- `IDocumentWriter`: Abstracts document writing

### 2. Template System

**Components**:
- `Template`: Domain entity representing a document template
- `ITemplateRepository`: Template storage abstraction
- Template placeholders: `{{field_name}}` syntax

### 3. Validation System

**Components**:
- `IDocumentValidator`: Input validation
- `IOutputValidator`: Output quality validation
- `Result<T>`: Functional error handling

## Design Patterns

### 1. Repository Pattern
**Purpose**: Abstract data persistence

```csharp
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task AddAsync(Document document);
}
```

### 2. Composite Pattern
**Used in**: `CompositeDocumentReader`

```csharp
public class CompositeDocumentReader : IDocumentReader
{
    private readonly IEnumerable<IDocumentReader> _readers;
    
    public async Task<string> ReadDocumentAsync(string filePath)
    {
        var reader = _readers.FirstOrDefault(r => r.IsSupported(filePath));
        return await reader.ReadDocumentAsync(filePath);
    }
}
```

### 3. Strategy Pattern
**Used in**: Document readers/writers for different formats

### 4. Result Pattern
**Purpose**: Functional error handling without exceptions

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
}
```

### 5. Options Pattern
**Purpose**: Strongly-typed configuration

```csharp
services.AddOptions<ApplicationOptions>()
    .Bind(configuration.GetSection("Application"))
    .ValidateDataAnnotations();
```

### 6. Plugin Pattern
**Used in**: Semantic Kernel plugins for AI functionality

## Dependency Flow

```
Presentation → Application → Domain
     ↓             ↓
     └─────→ Infrastructure
```

- **Inward Dependencies**: Dependencies point inward toward the domain
- **No Outward Dependencies**: Domain has no dependencies
- **Interface Segregation**: Small, focused interfaces

## Coding Conventions

### Naming Conventions

1. **Interfaces**: Prefix with 'I'
   ```csharp
   public interface IDocumentService { }
   ```

2. **Async Methods**: Suffix with 'Async'
   ```csharp
   public async Task<Document> GetDocumentAsync() { }
   ```

3. **Private Fields**: Prefix with underscore
   ```csharp
   private readonly ILogger<DocumentService> _logger;
   ```

4. **Constants**: PascalCase
   ```csharp
   public const string SectionName = "Application";
   ```

### File Organization

1. **One Type Per File**: Each class/interface in its own file
2. **Folder Structure Mirrors Namespace**: Consistent organization
3. **Related Types Grouped**: Exceptions, DTOs, etc. in dedicated folders

### Code Style

1. **Explicit Access Modifiers**: Always specify public/private
2. **Readonly Fields**: Use readonly for injected dependencies
3. **Expression-Bodied Members**: For simple properties/methods
4. **Target-Typed New**: Use when type is obvious
   ```csharp
   List<string> items = new();
   ```

## Error Handling Strategy

### 1. Domain Exceptions
Custom exceptions for domain-specific errors:

```csharp
public abstract class DomainException : Exception
{
    public string Code { get; }
}
```

### 2. Exception Hierarchy
```
Exception
└── DomainException
    ├── DocumentNotFoundException
    ├── InvalidDocumentFormatException
    ├── TemplateNotFoundException
    └── NormalizationException
```

### 3. Error Handling Layers

- **Domain**: Throws domain exceptions for business rule violations
- **Application**: Catches and logs exceptions, returns Result<T>
- **Infrastructure**: Wraps external exceptions in domain exceptions
- **Presentation**: Displays user-friendly error messages

## Logging Architecture

### 1. Structured Logging with Serilog

```csharp
_logger.LogInformation("Document processed {DocumentId} in {Duration}ms", 
    documentId, duration);
```

### 2. Log Levels
- **Debug**: Detailed execution flow
- **Information**: Key operations
- **Warning**: Non-critical issues
- **Error**: Exceptions and failures
- **Fatal**: Application crashes

### 3. Log Enrichment
- Timestamp
- Log level
- Source context
- Exception details

## Configuration Management

### 1. Configuration Sources
1. `appsettings.json`: Default configuration
2. Environment variables: Override settings
3. Command-line arguments: Runtime overrides

### 2. Configuration Validation
```csharp
services.AddOptions<ApplicationOptions>()
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### 3. Configuration Sections
- `OpenAI`: AI service settings
- `Application`: General application settings
- `Serilog`: Logging configuration

## Security Considerations

### 1. Input Validation
- File size limits
- Extension whitelist
- Path traversal prevention
- Content validation

### 2. Sensitive Data
- API keys in configuration (not in code)
- No logging of sensitive information
- Secure file operations

### 3. Dependency Security
- Regular package updates
- Security scanning
- License compliance

## Performance Considerations

### 1. Async/Await
- All I/O operations are async
- Proper cancellation token usage
- No blocking calls

### 2. Resource Management
- Proper disposal with `using` statements
- Stream processing for large files
- Memory-efficient document processing

### 3. Caching Strategy
- Template caching (future enhancement)
- Configuration caching
- Compiled regex patterns

## Testing Strategy

### 1. Unit Tests
**Focus**: Domain logic and application services

```csharp
[Fact]
public void Document_SetNormalizedContent_UpdatesStatus()
{
    // Arrange
    var document = Document.Create("test.pdf", "content");
    
    // Act
    document.SetNormalizedContent("normalized");
    
    // Assert
    Assert.Equal(DocumentStatus.Normalized, document.Status);
}
```

### 2. Integration Tests
**Focus**: Infrastructure components

```csharp
[Fact]
public async Task AsposePdfReader_ReadDocument_ReturnsContent()
{
    // Test actual PDF reading
}
```

### 3. Validation Tests
**Focus**: End-to-end document processing

## Future Extensibility

### 1. New Document Formats
1. Create new reader implementing `IDocumentReader`
2. Register in DI container
3. No changes to existing code

### 2. New AI Providers
1. Create new implementation of `INormalizationService`
2. Swap implementation in DI

### 3. New Storage Options
1. Implement `IDocumentRepository` for database
2. Configure in DI

### 4. API Addition
1. Add new Web API project
2. Reuse Application layer services
3. No changes to business logic

### 5. Batch Processing
1. Add new service for batch operations
2. Leverage existing document processing
3. Add queuing infrastructure

## Best Practices

### 1. SOLID Principles
- **S**: Single Responsibility
- **O**: Open/Closed
- **L**: Liskov Substitution
- **I**: Interface Segregation
- **D**: Dependency Inversion

### 2. DRY (Don't Repeat Yourself)
- Shared logic in base classes
- Common functionality in helpers
- Reusable components

### 3. YAGNI (You Aren't Gonna Need It)
- Don't over-engineer
- Build what's needed now
- Design for extensibility

### 4. Clean Code
- Meaningful names
- Small, focused methods
- Clear intent
- Minimal comments (code should be self-documenting)

## Conclusion

The PDF Template CLI architecture provides a solid foundation for a maintainable, testable, and extensible application. By following Clean Architecture principles and established design patterns, the codebase remains flexible and adaptable to changing requirements while maintaining a clear separation of concerns.