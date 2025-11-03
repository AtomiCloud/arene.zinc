# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ASP.NET Core 8 microservice API ("Zinc") using Domain-Driven Design with a Result monad pattern. The project follows a three-layer architecture with dependency injection, OpenTelemetry observability, and YAML-based configuration.

## Project Structure

- `App/` - ASP.NET Core 8 API layer (controllers, startup, migrations, utilities)
  - `App/Modules/` - Feature modules (Users, Projects, SubscriptionTypes, Subscribers, System, Common)
  - `App/StartUp/` - Application bootstrap, Server, database, services, options
  - `App/Migrations/` - EF Core database migrations
  - `App/Templates/Email/` - Handlebars email templates (built with Bun)
- `Domain/` - DDD core layer (aggregates, domain models, service interfaces, repository interfaces)
  - Contains pure domain logic with Result monad abstractions
  - No external dependencies except `CSharp-Result` and `Microsoft.Extensions.Logging`
- `UnitTest/` - xUnit unit tests with FluentAssertions
- `IntTest/` - xUnit integration tests
- `infra/` - Kubernetes manifests, Helm charts, Dockerfiles
- `config/` - Development configuration files
- `scripts/` - Shell scripts for local development
- `tasks/` - Additional Taskfile definitions

## Common Commands

All commands use Task runner via `pls` (alias for `task`):

```bash
# Initial setup
pls setup              # Restore .NET packages, tools, secrets, install Bun deps
pls setup:dotnet       # (if available) Additional .NET-specific setup

# Development
pls dev                # Start Tilt development server with hot reload
pls run                # Run API locally once (LANDSCAPE=corsola)

# Database
pls migration:create -- MigrationName  # Create new EF migration (LANDSCAPE=lapras)

# Email templates
pls email:dev          # Preview email templates in dev mode
pls email:build        # Build email templates to HTML

# Testing
pls unit                                   # Run unit tests
pls unit:cover                             # Run unit tests with coverage (Domain only)
pls int                                    # Run integration tests (LANDSCAPE=tauros)
pls int:cover                              # Run integration tests with coverage (Domain + App)
pls exec -- dotnet test                    # Run all tests
pls exec -- dotnet test --filter "FullyQualifiedName~ProjectTests"  # Run specific tests

# Local cluster
pls tear               # Delete local k3d cluster
pls stop               # Stop Tilt development

# Auth & utilities
pls auth:token         # Obtain M2M auth token
pls lint               # Run pre-commit hooks
pls gen:encryption-key # Generate encryption key

# Build
pls build              # Build without restore
```

**Important**: Always use `pls exec --` prefix when running .NET commands in the development environment. Never run commands like `dotnet test` directly.

## Architecture & Key Patterns

### Three-Layer Architecture

1. **Domain Layer** (`Domain/`)

   - Pure domain logic with no infrastructure concerns
   - Interfaces for services and repositories (e.g., `IProjectService`, `IProjectRepository`)
   - Domain models: Aggregates (full entity), Principals (summary), Records (DTOs)
   - Uses Result monad (`CSharp_Result`) for error handling

2. **Data Layer** (`App/Modules/*/Data/`)

   - EF Core entities (`*Data.cs`)
   - Repository implementations
   - Database mappings and queries

3. **API Layer** (`App/Modules/*/API/V*/`)
   - Controllers extending `AtomiControllerBase`
   - Request/Response models (`*Req`, `*Res`)
   - FluentValidation validators
   - Mappers between API models and domain models

### Service Registration

Domain services are registered in `App/Modules/DomainServices.cs`:

- Add both service interface and implementation
- Add repository interface and implementation
- Use `.AutoTrace<T>()` extension for OpenTelemetry tracing

Example:

```csharp
s.AddScoped<IProjectService, ProjectService>()
  .AutoTrace<IProjectService>();
s.AddScoped<IProjectRepository, ProjectRepository>()
  .AutoTrace<IProjectRepository>();
```

### DbContext and Entities

Main database context is `App/StartUp/Database/MainDbContext.cs`:

- Add DbSet properties for new entities
- Configure relationships in `OnModelCreating`
- Use composite keys where appropriate (e.g., ProjectId + Id)

### Result Monad Pattern

Use `Result<T>` from `CSharp_Result` for all operations that can fail:

- Chain operations: `.Then()`, `.ThenAwait()`, `.DoAwait()`
- Convert nullables: `.ToResult()`, `ToResultOfSeq()`
- Return from controllers: `this.ReturnResult(result)`, `this.ReturnNullableResult(result, error)`

### Error Handling

Errors implement `IDomainProblem` in `App/Error/V1/`:

- `EntityNotFound`, `ValidationError`, `UniquenessError`, `Forbidden`
- Throw via `throw new DomainProblemException(problem)`
- Automatically mapped to RFC7807 Problem Details in controllers
- Map errors: `Errors.MapAll`, `Errors.MapNone`

### Guards and Authorization

Use guard clauses from `AtomiControllerBase`:

- `Guard(principal, "policy")` - Check single policy
- `GuardOrAll(principal, ["policy1", "policy2"])` - All must pass
- `GuardOrAny(principal, ["policy1", "policy2"])` - Any must pass

Configure policies in `App/Config/settings.yaml` under `Auth:Policies`.

## Configuration

All runtime config in YAML files under `App/Config/`:

- `settings.yaml` - Base configuration
- `settings.<landscape>.yaml` - Environment-specific overrides (lapras, corsola, etc.)
- Environment variable prefix: `Atomi_`
- Required: `LANDSCAPE` environment variable

### Adding New Configuration

1. Define option class in `App/StartUp/Options/`
2. Register in `App/StartUp/Options/OptionsExtensions.cs`
3. Add to YAML configuration
4. Inject via `IOptionsMonitor<T>` in constructors

### Infrastructure Services

**Database** (PostgreSQL via EF Core):

- Add key to `Registry.Databases`
- Configure under `Database:` in YAML
- Migrations run automatically if `AutoMigrate: true`

**Cache** (Redis/Dragonfly):

- Configure under `Cache:` in YAML
- Inject `IConnectionMultiplexer` or use `IDistributedCache`

**Block Storage** (MinIO/S3):

- Configure under `BlockStorage:` in YAML
- Use `IFileRepository` for uploads/downloads
- Use `IFileValidator` for MIME type validation

**HTTP Clients**:

- Add key to `Registry.HttpClients`
- Configure under `HttpClient:` in YAML
- Inject `IHttpClientFactory` and use the key

**SMTP**:

- Configure under `Smtp:` in YAML
- Use `ISmtpClientFactory` and `SmtpEmailMessage`
- Templates use Handlebars in `App/Templates/Email/`

**Auth** (JWT Bearer):

- Configure under `Auth:` in YAML
- Use `[Authorize(Policy = "...")]` on controllers
- Extract token data via `ITokenDataExtractor`

## Adding a New Feature Module

1. Create domain interfaces and models in `Domain/<Feature>/`
2. Create module structure in `App/Modules/<Feature>/`:
   - `API/V1/<Feature>Controller.cs` - Controller
   - `API/V1/<Feature>Model.cs` - Request/Response DTOs
   - `API/V1/<Feature>Validator.cs` - FluentValidation validators
   - `API/V1/<Feature>Mapper.cs` - Mapping extensions
   - `Data/<Feature>Data.cs` - EF entity
   - `Data/<Feature>Repository.cs` - Repository implementation
   - `Data/<Feature>Mapper.cs` - Data layer mappings
3. Add DbSet to `MainDbContext.cs`
4. Register services in `DomainServices.cs`
5. Create migration: `pls migration:create -- Add<Feature>Table`
6. Write tests in `UnitTest/` and `IntTest/`

## Testing

**Framework**: xUnit with FluentAssertions and Moq for mocking

**Two Types of Testing**:

- **Unit Testing**: Isolated testing of domain logic with mocked dependencies (use `unit-testing` skill)
- **Integration Testing**: End-to-end scenario testing through API endpoints (use `integration-testing` skill)

### Unit Testing (Domain Layer)

**Focus**: Isolated testing of individual functions in the Domain layer with all dependencies mocked.

**Key Principles**:

1. **Mirror Domain Structure**: Test files mirror domain/module organization

   - `Domain/Projects/Service.cs` → `UnitTest/Domain/Projects/ServiceTests.cs`

2. **Theory-Based Testing**: All tests use `[Theory]` with multiple data sets

   - Primitive types: Use `[InlineData]`
   - Complex types: Use `[MemberData]` with static generators
   - 3-4 test variations per scenario for triangulation

3. **AAA Pattern**: Every test follows Arrange-Act-Assert structure

   ```csharp
   // Arrange - setup mocks, construct objects, prepare input/expected
   var mockRepo = new Mock<IProjectRepository>();
   var service = new ProjectService(mockRepo.Object);
   var input = new ProjectRecord { Name = "Test" };

   // Act - execute the method under test
   var actual = await service.Create(input);

   // Assert - compare actual with expected using FluentAssertions
   actual.Should().BeSuccess();
   actual.ValueOrDefault().Name.Should().Be("Test");
   ```

4. **Test Naming**: `{MethodName}_{Scenario}_{ShouldExpectedBehavior}`

   - Example: `Create_WithValidRecord_ShouldReturnProjectPrincipal`

5. **Coverage Targets**:
   - Domain Services: 100%
   - Domain Models: 100%
   - Repositories: 90%+
   - Validators: 100%

### Integration Testing (Full Stack)

**Focus**: End-to-end user flows and business scenarios through real API endpoints.

**Key Principles**:

1. **Scenario-Based**: Test complete user journeys, not isolated endpoints

   - Example: User registers → verifies email → logs in → updates profile

2. **Multi-Step Flows**: One test can use multiple endpoints together

   - Create → Get → Update → Delete

3. **Business Edge Cases**: Test real-world scenarios from business requirements

   - Validation errors, conflicts, permissions, state transitions

4. **Test Naming**: `{ScenarioName}_{Context}_{ShouldExpectedOutcome}`

   - Example: `CreateProject_WithValidData_ShouldReturnCreatedProjectAndBeRetrievable`

5. **Environment**:
   - **Landscape**: `tauros` - automatically set by `pls int` task
   - **Database**: In-memory EF Core database (bypasses PostgreSQL)
   - **Configuration**: `App/Config/settings.tauros.yaml` with disabled OTEL, auth, and external services
   - **Test Factory**: `IntTest/Infrastructure/TestWebApplicationFactory.cs` creates custom WebApplication
   - **Database Cleanup**: Use `IAsyncLifetime` to clear database between tests

### Run Tests:

```bash
pls unit                                                         # Run unit tests
pls unit:cover                                                   # Run unit tests with coverage (Domain only)
pls int                                                          # Run integration tests
pls int:cover                                                    # Run integration tests with coverage (Domain + App)
pls exec -- dotnet test                                          # All tests
pls exec -- dotnet test --filter "FullyQualifiedName~ServiceTests"  # Specific class
```

**Example Tests**:

- Unit: `UnitTest/Domain/Projects/ServiceTests.cs`, `UnitTest/Domain/Marketing/Subscribers/ServiceTests.cs`
- Integration: `IntTest/Scenarios/Projects/ProjectManagementTests.cs`

## Transactions

Use `ITransactionManager` for multi-operation transactions:

```csharp
await transactionManager.Start(async () =>
{
  await repository1.Create(...);
  await repository2.Update(...);
  return Result.Ok(result);
});
```

## Validation

FluentValidation validators:

- Create `*Validator.cs` classes inheriting from `AbstractValidator<T>`
- Use in controllers: `validator.ValidateAsyncResult(model, "Error message")`
- Returns `ValidationError` problem on failure

## Code Style

Follow `.editorconfig`:

- 2-space indentation
- PascalCase for types, methods, properties
- camelCase for locals, parameters
- Prefer `var` and explicit braces
- Organize usings (System first, then others alphabetically)

## Scripting Preferences

- **Never use Python** - Always use shell tools (grep, awk, sed, etc.) for scripting and data processing
- Use zsh for interactive shell commands
- Prefer native Unix tools over external scripting languages

## Commits

Follow Conventional Commits (enforced by `.gitlint`):

- Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `build`, `ci`, `config`, `dep`, `amend`
- Format: `<type>(<scope>): <description>`
- Never include co-authored-by Claude in commits (per user preferences)

## Development Workflow

1. Set LANDSCAPE environment variable (e.g., `export LANDSCAPE=corsola`)
2. Run `pls setup` on first clone
3. Start development: `pls dev` (uses Tilt for hot reload)
4. Make changes to code
5. Create migration if schema changed: `pls migration:create -- MigrationName`
6. Run tests: `pls exec -- dotnet test`
7. Lint before commit: `pls lint`

## Key Files to Reference

- `AGENTS.md` - Detailed repository guidelines (check for additional context)
- `App/StartUp/Server.cs` - Application bootstrap and pipeline configuration
- `App/Modules/DomainServices.cs` - Service registration
- `App/StartUp/Database/MainDbContext.cs` - Database context and entity configuration
- `App/Modules/Common/BaseController.cs` - Controller base with guards and result helpers
- `Taskfile.yaml` - Available task commands
