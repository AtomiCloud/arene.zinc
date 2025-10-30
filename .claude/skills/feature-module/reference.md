# Feature Module Reference

Links to official documentation, guides, and related resources.

## Internal Documentation

### Guides

- **[New Feature Walkthrough](../../../docs/developer/guides/NewFeatureWalkthrough.md)** - Step-by-step guide for adding features
- **[Define Errors](../../../docs/developer/guides/DefineErrors.md)** - How to create domain problems
- **[Guides README](../../../docs/developer/guides/README.md)** - Index of all guides

### Concepts

- **[Result Monad](../../../docs/developer/concepts/Result.md)** - Result pattern and composition
- **[Problem Details](../../../docs/developer/concepts/Problem.md)** - Domain problem pattern
- **[Guards](../../../docs/developer/concepts/Guards.md)** - Authorization guards

### Architecture

- **[Architecture & Startup](../../../docs/developer/ArchitectureAndStartup.md)** - System architecture overview
- **[Project Structure](../../../docs/developer/ProjectStructure.md)** - Directory organization
- **[DDD Notes](../../../docs/developer/DDD_Notes.md)** - Domain-Driven Design principles

### Infrastructure

- **[Registry & Keys](../../../docs/developer/infra/RegistryAndKeys.md)** - Configuration constants
- **[Database](../../../docs/developer/infra/Database.md)** - EF Core and PostgreSQL
- **[Cache](../../../docs/developer/infra/Cache.md)** - Redis/Dragonfly setup
- **[Block Storage](../../../docs/developer/infra/BlockStorage.md)** - MinIO/S3 file storage
- **[HttpClient](../../../docs/developer/infra/HttpClient.md)** - HTTP client configuration
- **[SMTP](../../../docs/developer/infra/Smtp.md)** - Email sending
- **[CORS](../../../docs/developer/infra/Cors.md)** - Cross-origin configuration
- **[Auth Policies](../../../docs/developer/infra/AuthPolicies.md)** - Authorization setup
- **[Telemetry](../../../docs/developer/infra/Telemetry.md)** - OpenTelemetry observability

### Other

- **[Transactions](../../../docs/developer/Transactions.md)** - Transaction management
- **[Validation](../../../docs/developer/validation/Validation.md)** - FluentValidation
- **[Testing](../../../docs/developer/testing/Testing.md)** - xUnit testing guide
- **[Migrations](../../../docs/developer/migrations/Migrations.md)** - EF Core migrations
- **[File Uploads](../../../docs/developer/files/Uploads.md)** - File upload handling
- **[Coding Style](../../../docs/developer/CodingStyle.md)** - Code style guidelines
- **[Commit Conventions](../../../docs/developer/CommitConventions.md)** - Git commit format
- **[Dev & Tasks](../../../docs/developer/DevAndTasks.md)** - Development commands

## External Documentation

### ASP.NET Core

- **[ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)** - Official Microsoft docs
- **[Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)** - ORM documentation
- **[Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)** - DI in ASP.NET Core
- **[Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)** - Configuration system
- **[Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)** - Logging in ASP.NET Core

### Testing

- **[xUnit Documentation](https://xunit.net/)** - xUnit testing framework
- **[FluentAssertions](https://fluentassertions.com/)** - Assertion library
- **[Moq Documentation](https://github.com/moq/moq4)** - Mocking framework

### Validation

- **[FluentValidation](https://docs.fluentvalidation.net/)** - Validation library
- **[Data Annotations](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)** - Model validation

### API Design

- **[API Versioning](https://github.com/dotnet/aspnet-api-versioning)** - API versioning in ASP.NET Core
- **[Problem Details (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807)** - HTTP API problem details
- **[REST API Guidelines](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)** - Microsoft REST API guidelines

### Result Monad

- **[CSharp-Result (CarboxylicLithium)](https://github.com/AtomiCloud/CSharp-Result)** - Result monad library
- **[Railway-Oriented Programming](https://fsharpforfunandprofit.com/rop/)** - Result pattern explanation

### OpenTelemetry

- **[OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)** - Observability SDK
- **[.NET Diagnostics](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/)** - Diagnostics and tracing

### Domain-Driven Design

- **[Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)** - Original DDD book
- **[DDD Reference](https://www.domainlanguage.com/ddd/reference/)** - DDD pattern reference
- **[Implementing DDD](https://vaughnvernon.com/books/)** - Vaughn Vernon's implementation guide

## Reference Implementations

### In This Codebase

Study these modules for patterns:

- **`App/Modules/Projects/`** - Simple CRUD with GUID keys
- **`App/Modules/Users/`** - String identifiers and namespace conflicts
- **`App/Modules/Subscribers/`** - Complex nested objects
- **`App/Modules/SubscriptionTypes/`** - Composite keys

### Tests

- **`UnitTest/Domain/Projects/ServiceTests.cs`** - Domain service unit tests
- **`UnitTest/Domain/User/ServiceTests.cs`** - Testing with string IDs
- **`UnitTest/Domain/Marketing/Subscribers/ServiceTests.cs`** - Complex object tests

## Tools & Commands

### Task Runner (pls)

All commands use `pls` (alias for `task`):

```bash
pls setup                  # Initial setup
pls dev                    # Start development server
pls run                    # Run API once
pls migration:create       # Create EF migration
pls exec -- dotnet test    # Run tests
pls lint                   # Run linters
```

### .NET CLI

Always use via `pls exec --`:

```bash
pls exec -- dotnet build
pls exec -- dotnet test
pls exec -- dotnet test --filter "FullyQualifiedName~WidgetTests"
```

### EF Core Migrations

```bash
pls migration:create -- AddWidgetsTable
pls migration:create -- UpdateWidgetSchema
```

## Related Skills

- **[testing](../testing/SKILL.md)** - Comprehensive testing guidelines for xUnit, FluentAssertions, and Moq

## Configuration Files

- **`.editorconfig`** - Code style rules
- **`.gitlint`** - Commit message linting
- **`Taskfile.yaml`** - Task definitions
- **`App/Config/settings.yaml`** - Runtime configuration
- **`App/Config/settings.<landscape>.yaml`** - Environment-specific overrides

## Quick Links

- [Project README](../../../README.md)
- [CLAUDE.md](../../../CLAUDE.md) - Claude Code instructions
- [AGENTS.md](../../../AGENTS.md) - Repository guidelines
