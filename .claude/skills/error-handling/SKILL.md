---
name: error-handling
description: Define and use domain problems for error handling in Zinc ASP.NET Core 8 API with Result monad pattern
---

# Error Handling Skill

Use this skill when defining domain problems and implementing error handling in the Zinc ASP.NET Core 8 API project.

## Related Documentation

- **[examples.md](examples.md)** - Complete code examples for domain problems
- **[reference.md](reference.md)** - Links to official documentation

## Overview

Zinc uses a domain-first error handling approach where business rule violations are modeled as **Problems** that implement `IDomainProblem`. These problems flow through the Result monad and are automatically mapped to RFC 7807 Problem Details in HTTP responses.

## Critical Distinction: Error vs Exception

**Errors** are expected, domain-level problems that represent business rule violations or known failure states:

- User inputs invalid data (validation error)
- Entity not found (NFE - NotFoundException)
- Duplicate entity (uniqueness constraint)
- Unauthorized access (permission denied)

**Exceptions** are unexpected, infrastructure-level failures:

- Database connection lost
- Network timeout
- Out of memory
- Null reference (programming bug)

**Key Principle**: Errors should be returned as Result failures or DomainProblemExceptions. Exceptions should only be thrown for truly exceptional circumstances.

## Result Monad Library

This project uses the **AtomiCloud.Result** NuGet package (namespace: `CarboxylicLithium`) for the Result monad pattern.

**IMPORTANT**: Do NOT create your own Result implementation. Use the installed NuGet package.

**Package**: `AtomiCloud.Result`
**Namespace**: `CarboxylicLithium`

The `Result<T>` type is a struct that wraps either a success value or an exception:

```csharp
// Provided by AtomiCloud.Result NuGet package - DO NOT reimplement
public readonly struct Result<TSucc>
{
  private readonly TSucc? _value;
  private readonly Exception? _exception;
  private readonly bool _isSuccess;
}
```

**Using statements**:

```csharp
using CarboxylicLithium;  // For Result<T>
```

**Key Operations** (from the package):

- `Result.Ok(value)` or `new Result<T>(value)` - Create success
- `new Result<T>(exception)` - Create failure
- `.Then()` - Chain synchronous operations
- `.ThenAwait()` - Chain async operations
- `.IsSuccess()` - Check if successful
- `.Get()` - Extract success value (throws if failure)
- `.FailureOrDefault()` - Extract exception

**Error Mapping Predicates** (provided by AtomiCloud.Result package):

```csharp
using CarboxylicLithium;  // For Errors class

// Errors.MapNone - Let exceptions propagate through Result chain
.Then(x => x.ToRes(), Errors.MapNone)

// Errors.MapAll - Convert all exceptions to Result failures
.Then(x => x.ToRes(), Errors.MapAll)

// Errors.MapIfExceptionIs<T>() - Convert only specific exception types
.Then(x => x.ToRes(), Errors.MapIfExceptionIs<NotFoundException>())
```

## Error Handling Architecture

### Components

1. **Domain Problems** (`App/Error/V1/*.cs`) - Implement `IDomainProblem` interface from AtomiCloud.IDomainProblem NuGet package
2. **Result Monad** (from AtomiCloud.Result NuGet package) - Compose operations that can fail
3. **DomainProblemException** - Transport mechanism for problems
4. **Base Controller** (`MapException()`) - Maps problems to HTTP status codes
5. **ProblemDetailsService** - Renders RFC 7807 Problem Details

**Required NuGet Packages**:

- `AtomiCloud.Result` (namespace: `CarboxylicLithium`) - Result monad and error mapping
- `AtomiCloud.IDomainProblem` (namespace: `CarboxylicBoron`) - IDomainProblem interface

### Flow

```
Domain/Service
  ↓ Returns Result with Problem or throws DomainProblemException
Controller
  ↓ Catches exception and maps to HTTP status via MapException()
  ↓ Three scenarios:
  ↓   1. Domain problem WITH mapping → specific HTTP status
  ↓   2. Domain problem WITHOUT mapping → 400 Bad Request (fallback)
  ↓   3. Unknown exception → AggregateException → 500 Internal Server Error
ProblemDetailsService
  ↓ Renders RFC 7807 Problem Details JSON
HTTP Response
```

## Error Mapper Flow in BaseController

The `MapException()` method in `AtomiControllerBase` (`App/Modules/Common/BaseController.cs:46`) uses a switch pattern to map exceptions to HTTP status codes:

```csharp
private ActionResult MapException(Exception e)
{
  return e switch
  {
    // Domain problems wrapped in DomainProblemException
    DomainProblemException d => d.Problem switch
    {
      EntityNotFound => this.Error(HttpStatusCode.NotFound, d.Problem),
      UnknownFileType unknownFileType => this.Error(HttpStatusCode.NotAcceptable, unknownFileType),
      ValidationError validationError => this.Error(HttpStatusCode.BadRequest, validationError),
      Unauthorized unauthorizedError => this.Error(HttpStatusCode.Forbidden, unauthorizedError),
      Unauthenticated unauthenticatedError => this.Error(HttpStatusCode.Unauthorized, unauthenticatedError),
      EntityConflict entityConflict => this.Error(HttpStatusCode.Conflict, entityConflict),
      MultipleEntityNotFound multipleEntityNotFound => this.Error(HttpStatusCode.NotFound, multipleEntityNotFound),

      // Domain problem with no specific mapping → 400 Bad Request
      _ => this.Error(HttpStatusCode.BadRequest, d.Problem),
    },

    // Known domain exception (NotFoundException) → mapped to EntityNotFound → 404
    NotFoundException nfe => this.Error(HttpStatusCode.NotFound,
      new EntityNotFound(nfe.Message, nfe.Type, nfe.RequestIdentifier)),

    // Unknown exception → throw AggregateException (500 Internal Server Error)
    _ => throw new AggregateException("Unhandled Exception", e),
  };
}
```

**Three Mapping Scenarios**:

1. **Domain problem with mapping** (like EntityNotFound, ValidationError):

   - Problem implements `IDomainProblem`
   - Wrapped in `DomainProblemException`
   - Has explicit case in `MapException()` switch
   - **Result**: Mapped to specific HTTP status code (404, 400, 403, etc.)

2. **Domain problem without mapping**:

   - Problem implements `IDomainProblem`
   - Wrapped in `DomainProblemException`
   - No explicit case in switch (hits `_ =>` in inner switch)
   - **Result**: Mapped to 400 Bad Request (fallback)

3. **Unknown exception** (infrastructure failure):
   - Any exception not matching known types
   - Hits outer `_ =>` case
   - **Result**: Throws `AggregateException` → 500 Internal Server Error
   - **Purpose**: Enforces strict exception handling - missing mappings are obvious

## When to Define Domain Problems

Create domain problems for:

- **Business rule violations** - Validation failures, state conflicts
- **Resource not found** - Entities that don't exist
- **Authorization failures** - Policy violations, insufficient permissions
- **Conflict conditions** - Unique constraint violations, race conditions
- **External service failures** - API errors, timeouts

**Do NOT create problems for**:

- Infrastructure exceptions (EF exceptions, network errors) - Catch and convert to problems at boundaries
- Programming errors (null reference, index out of range) - Let these bubble up as unhandled exceptions

## Defining Domain Problems

### IDomainProblem Interface

**IMPORTANT**: Do NOT create your own `IDomainProblem` interface. Use the interface provided by the **AtomiCloud.IDomainProblem** NuGet package.

**Package**: `AtomiCloud.IDomainProblem`
**Namespace**: `CarboxylicBoron`

**Using statement**:

```csharp
using CarboxylicBoron;  // For IDomainProblem interface
```

All problems must implement the `IDomainProblem` interface from the package:

```csharp
// Provided by AtomiCloud.IDomainProblem NuGet package - DO NOT reimplement
public interface IDomainProblem
{
  [JsonIgnore, JsonSchemaIgnore] string Id { get; }         // Stable identifier (e.g., "validation_error")
  [JsonIgnore, JsonSchemaIgnore] string Title { get; }      // Human-readable title
  [JsonIgnore, JsonSchemaIgnore] string Detail { get; }     // Detailed explanation
  [JsonIgnore, JsonSchemaIgnore] string Namespace { get; }  // API namespace
}
```

### Problem Structure

```csharp
// App/Error/V1/ProblemName.cs
namespace App.Error.V1;

using CarboxylicBoron;  // For IDomainProblem interface from AtomiCloud.IDomainProblem package
using System.ComponentModel;
using System.Text.Json.Serialization;

[Description("This error represents...")]
public class ProblemName : IDomainProblem  // Interface from AtomiCloud.IDomainProblem NuGet package
{
  public ProblemName() { }

  public ProblemName(string detail, /* other params */)
  {
    this.Detail = detail;
    // ... set other properties
  }

  [JsonIgnore, JsonSchemaIgnore]
  public string Id => "problem_identifier";

  [JsonIgnore, JsonSchemaIgnore]
  public string Title => "Human Readable Title";

  [JsonIgnore, JsonSchemaIgnore]
  public string Version => "v1";

  [JsonIgnore, JsonSchemaIgnore]
  public string Namespace => "atomi.zinc";

  public string Detail { get; } = string.Empty;

  // Custom properties for additional context
  [Description("Additional context property")]
  public string CustomProperty { get; } = string.Empty;
}
```

### Naming Conventions

- **File name**: `ProblemName.cs` (PascalCase, descriptive)
- **Problem ID**: `snake_case` (stable, never changes)
- **Location**: `App/Error/V1/` (versioned by API version)

### Example Problems

See [examples.md#domain-problems](examples.md#domain-problems) for complete examples:

- `ValidationError` - Input validation failures
- `EntityNotFound` - Resource not found
- `EntityConflict` - Unique constraint violations
- `Unauthorized` - Authorization failures
- `FileTooLarge` - File upload size violations

## Using Domain Problems

### Option 1: Return Result with Problem

Preferred for expected failures:

```csharp
using CarboxylicLithium;  // For Result<T> from AtomiCloud.Result package

public async Task<Result<Widget>> Create(WidgetRecord record)
{
  if (string.IsNullOrEmpty(record.Name))
  {
    return new DomainProblemException(
      new ValidationError("Name is required")
    ).ToResult<Widget>();
  }

  // ... continue processing
  return Result.Ok(widget);  // Result.Ok from AtomiCloud.Result package
}
```

### Option 2: Throw DomainProblemException

For exceptional failures that should interrupt flow:

```csharp
public async Task<Result<Widget>> Create(WidgetRecord record)
{
  var existing = await repository.GetByName(record.Name);
  if (existing is not null)
  {
    throw new DomainProblemException(
      new EntityConflict("Widget with this name already exists")
    );
  }

  // ... continue processing
}
```

### Option 3: Extension Method

Shorthand for throwing:

```csharp
var problem = new EntityNotFound("Widget not found", typeof(Widget), id);
return problem.ToException();
```

## Controller Integration

### Returning Results

Use base controller helpers to handle Result types:

```csharp
// For Result<T> that always has a value
var result = await service.DoSomething();
return this.ReturnResult(result);

// For Result<T?> that might be null
var result = await service.GetById(id);
return this.ReturnNullableResult(
  result,
  new EntityNotFound("Widget not found", typeof(Widget), id.ToString())
);

// For operations returning Unit (delete)
var result = await service.Delete(id);
return this.ReturnUnitNullableResult(
  result,
  new EntityNotFound("Widget not found", typeof(Widget), id.ToString())
);
```

### Manual Error Handling

For explicit control:

```csharp
var result = await service.DoSomething();

if (result.IsFailure())
{
  var error = result.FailureOrDefault();
  return this.ReturnResult(Result.Fail<WidgetRes>(error));
}

return Ok(result.ValueOrDefault());
```

## Mapping Problems to HTTP Status Codes

Mapping happens in `App/Modules/Common/BaseController.cs:MapException()`:

| Problem Type               | HTTP Status        | When to Use                       |
| -------------------------- | ------------------ | --------------------------------- |
| `ValidationError`          | 400 Bad Request    | Input validation failures         |
| `InvalidJson`              | 400 Bad Request    | Malformed JSON payloads           |
| `EntityNotFound`           | 404 Not Found      | Resource doesn't exist            |
| `Unauthenticated`          | 401 Unauthorized   | Missing/invalid auth token        |
| `Unauthorized`             | 403 Forbidden      | Insufficient permissions          |
| `EntityConflict`           | 409 Conflict       | Unique constraint violations      |
| `UnknownFileType`          | 406 Not Acceptable | Unsupported MIME types            |
| `MultipleEntityNotFound`   | 404 Not Found      | Batch operation failures          |
| Custom problems (unmapped) | 400 Bad Request    | Default for unrecognized problems |

### Adding Custom Mappings

To map custom problems to specific status codes, add a case to `MapException()`:

```csharp
// App/Modules/Common/BaseController.cs
private ActionResult MapException(Exception e)
{
  return e switch
  {
    DomainProblemException d => d.Problem switch
    {
      // ... existing mappings ...
      YourCustomProblem customProblem => this.Error(HttpStatusCode.UnprocessableEntity, customProblem),
      _ => this.Error(HttpStatusCode.BadRequest, d.Problem),
    },
    // ... rest of switch
  };
}
```

## Catching Infrastructure Exceptions

Convert infrastructure exceptions to problems at boundaries (repositories):

```csharp
// In Repository
public async Task<Result<WidgetPrincipal>> Create(WidgetRecord record)
{
  try
  {
    var widget = record.ToData();
    db.Widgets.Add(widget);
    await db.SaveChangesAsync();
    return Result.Ok(widget.ToPrincipal());
  }
  catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
  {
    return new DomainProblemException(new EntityConflict(
      "User with this username already exists",
      typeof(User),
      record.Name
    ));
  }
}
```

## Result Monad Error Propagation

### Chaining with Error Mapping

Use `Errors.MapAll` to convert all exceptions to Result failures:

```csharp
using CarboxylicLithium;  // For Result<T> and Errors from AtomiCloud.Result package

var result = await service.GetById(id)
  .Then(widget => widget?.Process(), Errors.MapAll)  // Errors.MapAll from package
  .ThenAwait(processed => repository.Update(processed), Errors.MapAll);

return this.ReturnResult(result);
```

Use `Errors.MapNone` to let exceptions propagate:

```csharp
using CarboxylicLithium;  // For Errors.MapNone from package

var result = await service.GetById(id)
  .Then(widget => widget?.ToRes(), Errors.MapNone)  // Errors.MapNone from package
  .ThenAwait(res => service.Enrich(res), Errors.MapNone);

return this.ReturnResult(result);
```

### Selective Error Mapping

Use custom mapping functions:

```csharp
var result = await service.GetById(id)
  .Then(
    widget => widget?.Process(),
    error => error switch
    {
      NotFoundException => new EntityNotFound(...),
      _ => error
    }
  );
```

Or map specific exception types:

```csharp
using CarboxylicLithium;  // For Errors.MapIfExceptionIs from package

var result = await service.GetById(id)
  .Then(widget => widget?.Process(), Errors.MapIfExceptionIs<NotFoundException>());
```

## RFC 7807 Problem Details

Problems are automatically rendered as RFC 7807 Problem Details:

```json
{
  "type": "https://api.example.com/problems/validation_error",
  "title": "Validation Error",
  "status": 400,
  "detail": "Name is required",
  "instance": "/api/v1/widgets",
  "data": {
    "field": "name",
    "errors": ["Name is required"]
  }
}
```

### Customizing Problem Details

Problems can include custom data:

```csharp
public record ValidationError : IDomainProblem
{
  [JsonIgnore, JsonSchemaIgnore]
  public string Id => "validation_error";

  [JsonIgnore, JsonSchemaIgnore]
  public string Title => "Validation Error";

  [JsonIgnore, JsonSchemaIgnore]
  public string Version => "v1";

  [JsonIgnore, JsonSchemaIgnore]
  public string Namespace => "atomi.zinc";

  public string Detail { get; init; } = string.Empty;

  // Custom properties become part of "data" in response
  [Description("Field-level validation errors")]
  public Dictionary<string, List<string>> Errors { get; init; } = new();
}
```

## Common Patterns

### Null to Error

Convert nullable results to errors:

```csharp
var result = await repository.GetById(id);
if (result.IsSuccess())
{
  var entity = result.Get();
  if (entity == null)
  {
    return this.ReturnNullableResult(
      Result.Fail<WidgetRes?>(new NotFoundException(...)),
      new EntityNotFound("Widget not found", typeof(Widget), id.ToString())
    );
  }
}
```

### Guard Clauses

Use guards for authorization:

```csharp
var result = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
  .ThenAwait(_ => service.GetById(userId, id))
  .Then(x => x?.ToRes(), Errors.MapAll);

return this.ReturnNullableResult(result, new EntityNotFound(...));
```

### Validation Results

Convert FluentValidation results:

```csharp
var result = await validator
  .ValidateAsyncResult(req, "Invalid CreateWidgetReq")
  .ThenAwait(x => service.Create(x.ToRecord()))
  .Then(x => x.ToRes(), Errors.MapNone);

return this.ReturnResult(result);
```

## Built-in Problems

Zinc includes these standard problems in `App/Error/V1/`:

- `ValidationError` - Input validation failures
- `EntityNotFound` - Resource not found
- `EntityConflict` - Unique constraint violations
- `Unauthorized` - Authorization failures (403)
- `Unauthenticated` - Authentication failures (401)
- `InvalidJson` - Malformed JSON
- `FileTooLarge` - File size violations
- `InvalidFileType` - Unsupported file types
- `InvalidFileExt` - Invalid file extensions
- `InvalidFileUpload` - General file upload errors
- `UnknownFileType` - Unknown MIME types
- `LikeRaceCondition` - Optimistic concurrency failures
- `MultipleEntityNotFound` - Batch operation failures

See [examples.md#built-in-problems](examples.md#built-in-problems) for usage examples.

## Best Practices

### DO

- ✅ Use descriptive, specific problem names
- ✅ Include relevant context in problem properties
- ✅ Keep problem IDs stable (never change them)
- ✅ Return Result for expected failures
- ✅ Throw DomainProblemException for exceptional failures
- ✅ Catch infrastructure exceptions and convert to problems
- ✅ Use guard clauses for authorization
- ✅ Test error paths in unit tests
- ✅ Use `Errors.MapNone` to let domain problems propagate
- ✅ Use `Errors.MapAll` to convert all exceptions
- ✅ Add explicit mappings in BaseController for custom problems

### DON'T

- ❌ Create problems for infrastructure concerns
- ❌ Change problem IDs after release
- ❌ Return generic Exception types
- ❌ Mix Result and throw approaches inconsistently
- ❌ Let infrastructure exceptions leak to controllers
- ❌ Use hardcoded status codes in controllers
- ❌ Forget to map custom problems to status codes in BaseController
- ❌ Confuse Errors (domain problems) with Exceptions (infrastructure failures)

## Testing Domain Problems

### Testing Problem Creation

```csharp
[Fact]
public void ValidationError_WithMessage_ShouldSetProperties()
{
  // Arrange
  var message = "Name is required";

  // Act
  var error = new ValidationError(message);

  // Assert
  error.Id.Should().Be("validation_error");
  error.Title.Should().Be("Validation Error");
  error.Version.Should().Be("v1");
  error.Detail.Should().Be(message);
}
```

### Testing Error Returns

```csharp
[Fact]
public async Task Create_WithInvalidInput_ShouldReturnValidationError()
{
  // Arrange
  var record = new WidgetRecord { Name = "" };

  // Act
  var result = await service.Create(record);

  // Assert
  result.IsFailure().Should().BeTrue();
  var exception = result.FailureOrDefault();
  exception.Should().BeOfType<DomainProblemException>();
  var problem = ((DomainProblemException)exception).Problem;
  problem.Should().BeOfType<ValidationError>();
}
```

### Testing Exception Throws

```csharp
[Fact]
public async Task Create_WithDuplicateName_ShouldThrowConflict()
{
  // Arrange
  mockRepo.Setup(r => r.GetByName("Test"))
    .ReturnsAsync(existingWidget);

  // Act & Assert
  await Assert.ThrowsAsync<DomainProblemException>(
    async () => await service.Create(new WidgetRecord { Name = "Test" })
  );
}
```

## Troubleshooting

### Problem not mapping to correct status code

- Check mapping in `BaseController.MapException()` (line 46+)
- Verify problem implements `IDomainProblem`
- Ensure controller uses `this.ReturnResult()` or related helpers
- Check that problem is wrapped in `DomainProblemException`

### Custom data not appearing in response

- Check `ProblemDetailsService.CreateProblemDetails`
- Verify custom properties are public
- Ensure JSON serialization is configured
- Don't use `[JsonIgnore]` on properties you want serialized

### Exceptions not caught

- Wrap infrastructure code in try-catch in repositories
- Convert to `DomainProblemException` at boundaries
- Use `Errors.MapAll` in Result chains to catch all exceptions

### Result chains breaking on error

- Use `Errors.MapAll` or custom error mapping
- Check that all steps return `Result<T>`
- Verify error types are compatible
- Ensure DomainProblemException is used for domain problems

### AggregateException being thrown (500 errors)

- This means an exception reached MapException without a mapping
- Add explicit case to `MapException()` switch for your exception type
- Or convert to DomainProblemException at the source

## Required NuGet Packages

**IMPORTANT**: Ensure these packages are installed in your project:

```xml
<!-- .csproj -->
<PackageReference Include="AtomiCloud.Result" Version="1.12.1" />
<PackageReference Include="AtomiCloud.IDomainProblem" Version="1.7.1" />
```

**Using statements**:

```csharp
using CarboxylicLithium;  // For Result<T>, Errors, error mapping predicates
using CarboxylicBoron;    // For IDomainProblem interface
```

## Quick Start

1. **Verify** NuGet packages are installed (AtomiCloud.Result, AtomiCloud.IDomainProblem)
2. **Add** using statements (`using CarboxylicLithium;` and `using CarboxylicBoron;`)
3. **Read** [examples.md](examples.md) for complete code examples
4. **Reference** [reference.md](reference.md) for official documentation
5. **Define** problems in `App/Error/V1/` implementing `IDomainProblem` from AtomiCloud.IDomainProblem package
6. **Use** `Result<T>` from AtomiCloud.Result package for composition with error mapping predicates
7. **Map** custom problems in `BaseController.MapException()`
8. **Return** via `this.ReturnResult()` or related helpers in controllers
9. **Test** both success and error paths
