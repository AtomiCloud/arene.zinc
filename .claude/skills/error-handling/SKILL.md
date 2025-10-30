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

## Error Handling Architecture

### Components

1. **Domain Problems** (`App/Error/V1/*.cs`) - Implement `IDomainProblem` interface
2. **Result Monad** (`CSharp_Result`) - Compose operations that can fail
3. **DomainProblemException** - Transport mechanism for problems
4. **Base Controller** - Maps problems to HTTP status codes
5. **ProblemDetailsService** - Renders RFC 7807 Problem Details

### Flow

```
Domain/Service
  ↓ Returns Result with Problem or throws DomainProblemException
Controller
  ↓ Catches exception and maps to HTTP status
ProblemDetailsService
  ↓ Renders RFC 7807 Problem Details JSON
HTTP Response
```

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

All problems must implement:

```csharp
public interface IDomainProblem
{
  string Id { get; }       // Stable identifier (e.g., "validation_error")
  string Title { get; }     // Human-readable title
  string Version { get; }   // API version (e.g., "v1")
  string Detail { get; }    // Detailed explanation
}
```

### Problem Structure

```csharp
// App/Error/V1/ProblemName.cs
namespace App.Error.V1;

public record ProblemName(/* properties */) : IDomainProblem
{
  public string Id => "problem_identifier";
  public string Title => "Human Readable Title";
  public string Version => "v1";
  public string Detail => $"Detailed message with {Property}";
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
public async Task<Result<Widget>> Create(WidgetRecord record)
{
  if (string.IsNullOrEmpty(record.Name))
  {
    return new ValidationError("Name is required");
  }

  // ... continue processing
  return Result.Ok(widget);
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
throw problem.ToException();
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

Mapping happens in `App/Modules/Common/BaseController.cs`:

| Problem Type      | HTTP Status           | When to Use                       |
| ----------------- | --------------------- | --------------------------------- |
| `ValidationError` | 400 Bad Request       | Input validation failures         |
| `InvalidJson`     | 400 Bad Request       | Malformed JSON payloads           |
| `EntityNotFound`  | 404 Not Found         | Resource doesn't exist            |
| `Unauthenticated` | 401 Unauthorized      | Missing/invalid auth token        |
| `Unauthorized`    | 403 Forbidden         | Insufficient permissions          |
| `EntityConflict`  | 409 Conflict          | Unique constraint violations      |
| `FileTooLarge`    | 413 Payload Too Large | File size exceeds limit           |
| Custom problems   | 400 Bad Request       | Default for unrecognized problems |

### Adding Custom Mappings

To map custom problems to specific status codes:

```csharp
// App/Modules/Common/BaseController.cs
protected int ProblemToStatusCode(IDomainProblem problem) =>
  problem switch
  {
    ValidationError => StatusCodes.Status400BadRequest,
    EntityNotFound => StatusCodes.Status404NotFound,
    YourCustomProblem => StatusCodes.Status422UnprocessableEntity,
    _ => StatusCodes.Status400BadRequest
  };
```

## Catching Infrastructure Exceptions

Convert infrastructure exceptions to problems at boundaries:

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
  catch (DbUpdateException ex)
  {
    if (ex.InnerException?.Message.Contains("unique constraint") == true)
    {
      throw new DomainProblemException(
        new EntityConflict($"Widget with name '{record.Name}' already exists")
      );
    }
    throw;
  }
}
```

## Result Monad Error Propagation

### Chaining with Error Mapping

Use `Errors.MapAll` to propagate all errors:

```csharp
var result = await service.GetById(id)
  .Then(widget => widget?.Process(), Errors.MapAll)
  .ThenAwait(processed => repository.Update(processed), Errors.MapAll);

return this.ReturnResult(result);
```

### Selective Error Mapping

Use `Errors.MapNone` or custom mapping:

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
  public string Id => "validation_error";
  public string Title => "Validation Error";
  public string Version => "v1";
  public string Detail { get; init; } = string.Empty;

  // Custom properties become part of "data" in response
  public Dictionary<string, List<string>> Errors { get; init; } = new();
}
```

## Common Patterns

### Null to Error

Convert nullable results to errors:

```csharp
var result = await repository.GetById(id)
  .NullToError(id.ToString());

// If result is null, returns NotFoundException
// Otherwise returns Result<T> with value
```

### Guard Clauses

Use guards for authorization:

```csharp
var principal = await GetPrincipal();
Guard(principal, "read:widgets");  // Throws Unauthorized if fails
```

### Validation Results

Convert FluentValidation results:

```csharp
var validationResult = await validator.ValidateAsyncResult(req, "Invalid data");
if (validationResult.IsFailure())
{
  return this.ReturnResult(Result.Fail<WidgetRes>(validationResult.FailureOrDefault()));
}
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

### DON'T

- ❌ Create problems for infrastructure concerns
- ❌ Change problem IDs after release
- ❌ Return generic Exception types
- ❌ Mix Result and throw approaches inconsistently
- ❌ Let infrastructure exceptions leak to controllers
- ❌ Use hardcoded status codes in controllers
- ❌ Forget to map custom problems to status codes

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
  result.FailureOrDefault().Should().BeOfType<ValidationError>();
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

- Check mapping in `BaseController.ProblemToStatusCode`
- Verify problem implements `IDomainProblem`
- Ensure controller uses `this.ReturnResult()`

### Custom data not appearing in response

- Check `ProblemDetailsService.CreateProblemDetails`
- Verify custom properties are public
- Ensure JSON serialization is configured

### Exceptions not caught

- Wrap infrastructure code in try-catch
- Convert to `DomainProblemException` at boundaries
- Use `Errors.MapAll` in Result chains

### Result chains breaking on error

- Use `Errors.MapAll` or custom error mapping
- Check that all steps return `Result<T>`
- Verify error types are compatible

## Quick Start

1. **Read** [examples.md](examples.md) for complete code examples
2. **Reference** [reference.md](reference.md) for official documentation
3. **Define** problems in `App/Error/V1/`
4. **Use** Result monad for composition
5. **Return** via `this.ReturnResult()` in controllers
6. **Test** both success and error paths
