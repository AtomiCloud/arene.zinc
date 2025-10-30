---
name: feature-module
description: Add new feature modules to Zinc ASP.NET Core 8 API using Domain-Driven Design with Result monad pattern
---

# Feature Module Skill

Use this skill when adding new feature modules to the Zinc ASP.NET Core 8 API project.

## Related Documentation

- **[examples.md](examples.md)** - Complete code examples for each layer
- **[reference.md](reference.md)** - Links to official documentation and guides

## Overview

This skill guides you through adding new feature modules following Domain-Driven Design with a thin-adapter, domain-first flow. The process ensures clear separation of concerns, explicit error handling, and strong configuration hygiene.

## Architecture Layers

Zinc follows a three-layer architecture:

1. **Domain Layer** (`Domain/`) - Pure domain logic, no infrastructure concerns
2. **Data Layer** (`App/Modules/*/Data/`) - EF Core entities, repositories, database mappings
3. **API Layer** (`App/Modules/*/API/V*/`) - Controllers, DTOs, validators, HTTP concerns

## Implementation Flow

### 1. Design Domain Layer

**Location**: `Domain/<Feature>/`

**What to Create**:

- Domain models (Aggregates, Principals, Records)
- Service interface (`I<Feature>Service.cs`)
- Repository interface (`I<Feature>Repository.cs`)
- Domain problems (if needed)

**Key Principles**:

- Keep pure: no EF, HTTP, or DI dependencies
- Use `Result<T>` from `CSharp_Result` for all operations
- Only dependency: `Microsoft.Extensions.Logging`

**Files**:

```
Domain/<Feature>/
├── <Feature>.cs           # Domain models (Aggregate, Principal, Record)
├── IService.cs            # Service interface
├── Repository.cs          # Repository interface
└── Service.cs             # Service implementation
```

See [examples.md#domain-layer](examples.md#domain-layer) for code examples.

### 2. Implement Data Layer

**Location**: `App/Modules/<Feature>/Data/`

**What to Create**:

- EF Core entity (`<Feature>Data.cs`)
- Mapper extensions (`<Feature>Mapper.cs`)
- Repository implementation (`<Feature>Repository.cs`)

**Key Principles**:

- Map between EF entities and domain models
- Catch infrastructure exceptions and convert to domain problems
- Use `MainDbContext` for database operations

**Files**:

```
App/Modules/<Feature>/Data/
├── <Feature>Data.cs       # EF Core entity
├── <Feature>Mapper.cs     # Entity ↔ Domain mappings
└── <Feature>Repository.cs # Repository implementation
```

**DbContext Updates**:

- Add `DbSet<T>` to `App/StartUp/Database/MainDbContext.cs`
- Configure relationships in `OnModelCreating`
- Create migration: `pls migration:create -- Add<Feature>Table`

See [examples.md#data-layer](examples.md#data-layer) for code examples.

### 3. Expose API Layer

**Location**: `App/Modules/<Feature>/API/V1/`

**What to Create**:

- Request/Response DTOs (`<Feature>Model.cs`)
- FluentValidation validators (`<Feature>Validator.cs`)
- Mapper extensions (`<Feature>Mapper.cs`)
- Controller (`<Feature>Controller.cs`)

**Key Principles**:

- Keep controllers thin: validate, guard, call service, map, return
- Extend `AtomiControllerBase` for guard helpers
- Use versioned routes: `api/v{version:apiVersion}/<feature>`
- Return results via `this.ReturnResult()` or `this.ReturnNullableResult()`

**Files**:

```
App/Modules/<Feature>/API/V1/
├── <Feature>Model.cs      # Request/Response DTOs
├── <Feature>Validator.cs  # FluentValidation validators
├── <Feature>Mapper.cs     # DTO ↔ Domain mappings
└── <Feature>Controller.cs # API controller
```

See [examples.md#api-layer](examples.md#api-layer) for code examples.

### 4. Wire Dependency Injection

**Location**: `App/Modules/DomainServices.cs`

**What to Register**:

- Service interface and implementation
- Repository interface and implementation
- Add `.AutoTrace<T>()` for OpenTelemetry tracing

**Pattern**:

```csharp
s.AddScoped<I<Feature>Service, <Feature>Service>()
  .AutoTrace<I<Feature>Service>();
s.AddScoped<I<Feature>Repository, <Feature>Repository>()
  .AutoTrace<I<Feature>Repository>();
```

See [examples.md#dependency-injection](examples.md#dependency-injection) for code examples.

### 5. Add Configuration (if needed)

**When**: Only if the feature needs infrastructure services (HttpClient, BlockStorage, Cache, SMTP, Database)

**Steps**:

1. Add constant to appropriate Registry in `App/StartUp/Registry/`
2. Add matching YAML configuration in `App/Config/settings.yaml`
3. Always reference via Registry constant (never raw strings)

**Infrastructure Options**:

- **Database**: `Registry.Databases` → `Database:` in YAML
- **Cache**: `Registry.Caches` → `Cache:` in YAML
- **BlockStorage**: `Registry.BlockStorages` → `BlockStorage:` in YAML
- **HttpClient**: `Registry.HttpClients` → `HttpClient:` in YAML
- **SMTP**: Configure under `Smtp:` in YAML

See [examples.md#configuration](examples.md#configuration) for code examples.

### 6. Write Tests

**Locations**:

- Unit tests: `UnitTest/Domain/<Feature>/ServiceTests.cs`
- Integration tests: `IntTest/<Feature>/`

**What to Test**:

- Domain service methods (all scenarios)
- Validators (all validation rules)
- Mappers (bidirectional conversions)
- Controllers (integration tests)

**Framework**: xUnit + FluentAssertions + Moq

**Run Tests**:

```bash
pls exec -- dotnet test
pls exec -- dotnet test --filter "FullyQualifiedName~<Feature>Tests"
```

**Coverage Targets**:

- Domain Services: 100%
- Validators: 100%
- Repositories: 90%+
- Controllers: 80%+

For detailed testing guidelines, use the `testing` skill.

## Implementation Checklist

Before considering a feature complete, verify:

- [ ] Domain interfaces defined in `Domain/<Feature>/`
- [ ] Domain problems defined (if needed)
- [ ] EF entity and mappings in `App/Modules/<Feature>/Data/`
- [ ] Repository implementation in `App/Modules/<Feature>/Data/`
- [ ] DbSet added to `MainDbContext.cs`
- [ ] Migration created: `pls migration:create -- Add<Feature>Table`
- [ ] API DTOs in `App/Modules/<Feature>/API/V1/`
- [ ] FluentValidation validators in `App/Modules/<Feature>/API/V1/`
- [ ] Controller in `App/Modules/<Feature>/API/V1/`
- [ ] Services registered in `App/Modules/DomainServices.cs`
- [ ] Infrastructure keys added to Registry (if needed)
- [ ] YAML configuration added (if needed)
- [ ] Unit tests written and passing
- [ ] Integration tests written and passing
- [ ] Coverage targets met
- [ ] Code follows `.editorconfig` style
- [ ] Commit follows Conventional Commits format

## Common Patterns

### Result Monad Composition

Chain operations using Result monad:

```csharp
return await repository.GetById(id)
  .Then(entity => entity?.ToDomain())
  .Then(domain => domain?.Process())
  .ThenAwait(result => repository.Update(result));
```

### Error Handling

Return domain problems for business rule violations:

```csharp
if (string.IsNullOrEmpty(record.Name))
  return new ValidationError("Name is required");

// Or throw for exceptional cases
throw new DomainProblemException(new EntityConflict("Duplicate name"));
```

### Guards and Authorization

Use guard helpers from `AtomiControllerBase`:

```csharp
// Single policy
Guard(principal, "read:widgets");

// All policies must pass
GuardOrAll(principal, ["read:widgets", "read:sensitive"]);

// Any policy can pass
GuardOrAny(principal, ["admin", "super:admin"]);
```

### Nullable Results

Handle nullable results with fallback errors:

```csharp
var result = await service.GetById(id);
return this.ReturnNullableResult(
  result,
  new EntityNotFound("Widget not found", typeof(Widget), id)
);
```

## Tips

- **Keep controllers thin**: Validate → Guard → Call Service → Map → Return
- **Prefer small PRs**: Follow the layer order for easier reviews
- **Use constants**: Never hardcode strings; use Registry constants
- **Test early**: Write tests alongside implementation
- **Follow patterns**: Study existing modules like Users, Projects, SubscriptionTypes

## Troubleshooting

### Migration fails

- Check entity configuration in `MainDbContext.OnModelCreating`
- Verify foreign key relationships
- Run: `pls migration:create -- Fix<Issue>`

### DI registration errors

- Ensure both interface and implementation are registered
- Check for circular dependencies
- Verify `.AutoTrace<T>()` is on the interface, not implementation

### Validation not working

- Register validator in controller via DI
- Call `validator.ValidateAsyncResult(model, "error message")`
- Check FluentValidation rules syntax

### Controller returns 500

- Check Result monad error handling
- Verify domain problems implement `IDomainProblem`
- Look for unhandled exceptions in logs

## Quick Start

1. **Read** [examples.md](examples.md) for complete code templates
2. **Reference** [reference.md](reference.md) for official documentation
3. **Follow** the 6-step implementation flow
4. **Test** with `pls exec -- dotnet test`
5. **Commit** using Conventional Commits format
