# Error Handling Reference

Links to official documentation, guides, and related resources for error handling in Zinc.

## Internal Documentation

### Guides

- **[Define Errors](../../../docs/developer/guides/DefineErrors.md)** - How to create domain problems
- **[New Feature Walkthrough](../../../docs/developer/guides/NewFeatureWalkthrough.md)** - Including error handling in features

### Concepts

- **[Problem Details](../../../docs/developer/concepts/Problem.md)** - Domain problem pattern explained
- **[Result Monad](../../../docs/developer/concepts/Result.md)** - Result pattern and composition
- **[Guards](../../../docs/developer/concepts/Guards.md)** - Authorization guard patterns

### Architecture

- **[Architecture & Startup](../../../docs/developer/ArchitectureAndStartup.md)** - System architecture overview
- **[Project Structure](../../../docs/developer/ProjectStructure.md)** - Directory organization

## External Documentation

### RFC 7807 Problem Details

- **[RFC 7807 Specification](https://datatracker.ietf.org/doc/html/rfc7807)** - HTTP API Problem Details standard
- **[Problem Details in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)** - Microsoft's implementation guide
- **[ProblemDetails Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails)** - .NET API documentation

### Result Monad Pattern

- **[CSharp-Result (CarboxylicLithium)](https://github.com/AtomiCloud/CSharp-Result)** - Result monad library used in Zinc
- **[Railway-Oriented Programming](https://fsharpforfunandprofit.com/rop/)** - F# pattern explanation (conceptual)
- **[Functional Error Handling](https://fsharpforfunandprofit.com/posts/recipe-part2/)** - Error handling with composition

### ASP.NET Core Error Handling

- **[Error Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)** - Official guide
- **[Exception Handling Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/exception-handling)** - Middleware patterns
- **[Custom Error Responses](https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors)** - Web API error handling

### HTTP Status Codes

- **[HTTP Status Codes (MDN)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)** - Complete reference
- **[RFC 7231 - HTTP/1.1 Semantics](https://datatracker.ietf.org/doc/html/rfc7231#section-6)** - Status code definitions
- **[REST API Status Codes](https://restfulapi.net/http-status-codes/)** - REST-specific guidance

### Exception Handling

- **[Exception Handling in C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/exceptions/)** - C# exceptions guide
- **[Best Practices for Exceptions](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)** - Exception guidelines
- **[DbUpdateException](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbupdateexception)** - EF Core exception handling

### Validation

- **[FluentValidation](https://docs.fluentvalidation.net/)** - Validation library
- **[Model Validation in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)** - Built-in validation

## Built-in Problems Reference

Zinc includes these standard problems in `App/Error/V1/`:

### Resource Errors

| Problem                  | HTTP Status | File                        | When to Use                                   |
| ------------------------ | ----------- | --------------------------- | --------------------------------------------- |
| `EntityNotFound`         | 404         | `EntityNotFound.cs`         | Resource doesn't exist                        |
| `MultipleEntityNotFound` | 404         | `MultipleEntityNotFound.cs` | Multiple resources missing in batch operation |

### Validation Errors

| Problem           | HTTP Status | File                 | When to Use               |
| ----------------- | ----------- | -------------------- | ------------------------- |
| `ValidationError` | 400         | `ValidationError.cs` | Input validation failures |
| `InvalidJson`     | 400         | `InvalidJson.cs`     | Malformed JSON payloads   |

### Authorization Errors

| Problem           | HTTP Status | File                 | When to Use                    |
| ----------------- | ----------- | -------------------- | ------------------------------ |
| `Unauthenticated` | 401         | `Unauthenticated.cs` | Missing/invalid authentication |
| `Unauthorized`    | 403         | `Unauthorized.cs`    | Insufficient permissions       |

### Conflict Errors

| Problem             | HTTP Status | File                   | When to Use                     |
| ------------------- | ----------- | ---------------------- | ------------------------------- |
| `EntityConflict`    | 409         | `EntityConflict.cs`    | Unique constraint violations    |
| `LikeRaceCondition` | 409         | `LikeRaceCondition.cs` | Optimistic concurrency failures |

### File Upload Errors

| Problem             | HTTP Status | File                   | When to Use                |
| ------------------- | ----------- | ---------------------- | -------------------------- |
| `FileTooLarge`      | 413         | `FileTooLarge.cs`      | File size exceeds limit    |
| `InvalidFileType`   | 400         | `InvalidFileType.cs`   | Unsupported MIME type      |
| `InvalidFileExt`    | 400         | `InvalidFileExt.cs`    | Invalid file extension     |
| `InvalidFileUpload` | 400         | `InvalidFileUpload.cs` | General upload failures    |
| `UnknownFileType`   | 400         | `UnknownFileType.cs`   | Cannot determine file type |

### Usage Errors

| Problem            | HTTP Status | File                  | When to Use               |
| ------------------ | ----------- | --------------------- | ------------------------- |
| `InvalidUserToken` | 401         | `InvalidUserToken.cs` | Token validation failures |

## Code Locations

### Problem Definitions

- **Location**: `App/Error/V1/`
- **Pattern**: `{ProblemName}.cs`
- **Interface**: Implement `IDomainProblem`

### Exception Types

- **`DomainProblemException`** - `App/Modules/Common/DomainProblemException.cs`
- **`NotFoundException`** - `Domain/Exceptions/NotFoundException.cs`

### Controllers

- **Base Controller**: `App/Modules/Common/BaseController.cs`
  - `ReturnResult<T>()` - Return Result<T>
  - `ReturnNullableResult<T>()` - Return Result<T?> with fallback
  - `ProblemToStatusCode()` - Map problems to HTTP status codes

### Services

- **ProblemDetailsService**: `App/StartUp/Services/ProblemDetailsService.cs`
  - Converts problems to RFC 7807 JSON
  - Enriches with request context

### Errors Utility

- **Location**: `App/Modules/Common/Errors.cs` (if exists)
- **Helpers**: `MapAll`, `MapNone` for Result error mapping

## Reference Implementations

### Study These Files

For complete working examples:

- **`App/Modules/Projects/API/V1/ProjectController.cs`** - Controller error handling
- **`App/Modules/Users/Data/UserRepository.cs`** - Repository exception handling
- **`Domain/User/Service.cs`** - Service-level error handling
- **`App/Error/V1/EntityNotFound.cs`** - Simple problem definition
- **`App/Error/V1/ValidationError.cs`** - Problem with complex data

### Test Examples

- **`UnitTest/Domain/Exceptions/NotFoundExceptionTests.cs`** - Testing exceptions
- **`UnitTest/Domain/Projects/ServiceTests.cs`** - Testing service errors
- **`IntTest/Projects/ProjectControllerTests.cs`** - Testing HTTP error responses

## Result Monad Methods

### Core Methods

| Method             | Signature                                                          | Purpose                             |
| ------------------ | ------------------------------------------------------------------ | ----------------------------------- |
| `Then`             | `Then<TOut>(Func<T, TOut>, Func<Exception, Exception>)`            | Transform success value, map errors |
| `ThenAwait`        | `ThenAwait<TOut>(Func<T, Task<TOut>>, Func<Exception, Exception>)` | Async transform                     |
| `DoAwait`          | `DoAwait(Func<T, Task>, Func<Exception, Exception>)`               | Side effects                        |
| `IsSuccess`        | `IsSuccess()`                                                      | Check if result succeeded           |
| `IsFailure`        | `IsFailure()`                                                      | Check if result failed              |
| `ValueOrDefault`   | `ValueOrDefault()`                                                 | Get value or default                |
| `FailureOrDefault` | `FailureOrDefault()`                                               | Get exception or null               |

### Extension Methods

| Method        | Signature                           | Purpose                           |
| ------------- | ----------------------------------- | --------------------------------- |
| `NullToError` | `NullToError<T>(string identifier)` | Convert null to NotFoundException |
| `ToResult`    | `ToResult<T>()`                     | Wrap value in Result              |

## Testing Utilities

### FluentAssertions

```csharp
result.IsSuccess().Should().BeTrue();
result.IsFailure().Should().BeFalse();
result.ValueOrDefault().Should().NotBeNull();
result.FailureOrDefault().Should().BeOfType<ValidationError>();
```

### Moq Setups

```csharp
mockRepo.Setup(r => r.GetById(It.IsAny<Guid>()))
  .ReturnsAsync(Result.Ok<Widget?>(null));

mockRepo.Setup(r => r.Create(It.IsAny<WidgetRecord>()))
  .ThrowsAsync(new DomainProblemException(new EntityConflict("Duplicate")));
```

## Common HTTP Status Codes

### Success Codes

- **200 OK** - GET, PUT successful
- **201 Created** - POST successful
- **204 No Content** - DELETE successful

### Client Error Codes

- **400 Bad Request** - ValidationError, InvalidJson
- **401 Unauthorized** - Unauthenticated (authentication failed)
- **403 Forbidden** - Unauthorized (insufficient permissions)
- **404 Not Found** - EntityNotFound
- **409 Conflict** - EntityConflict, LikeRaceCondition
- **413 Payload Too Large** - FileTooLarge
- **422 Unprocessable Entity** - Semantic validation errors

### Server Error Codes

- **500 Internal Server Error** - Unhandled exceptions
- **503 Service Unavailable** - Service temporarily down

## Configuration

### Problem Details Options

Configured in `App/StartUp/Server.cs`:

```csharp
builder.Services.AddProblemDetails(options =>
{
  options.CustomizeProblemDetails = context =>
  {
    // Add custom fields to all problem details
    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
  };
});
```

## Related Skills

- **[feature-module](../feature-module/SKILL.md)** - Adding new feature modules with error handling
- **[testing](../testing/SKILL.md)** - Testing error paths comprehensively

## Quick Reference

### Define Problem

```csharp
public record MyProblem(string Detail) : IDomainProblem
{
  public string Id => "my_problem";
  public string Title => "My Problem";
  public string Version => "v1";
}
```

### Return Error

```csharp
return new MyProblem("Error details");
```

### Throw Error

```csharp
throw new DomainProblemException(new MyProblem("Error details"));
```

### Handle in Controller

```csharp
return this.ReturnResult(result);
```

### Test Error

```csharp
result.IsFailure().Should().BeTrue();
result.FailureOrDefault().Should().BeOfType<MyProblem>();
```
