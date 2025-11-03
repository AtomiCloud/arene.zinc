# How to Define Errors

Goal

- Create clear, actionable errors that flow from domain/data to HTTP responses.
- Understand the distinction between Errors (domain problems) and Exceptions (infrastructure failures).
- Properly use the Result monad for error handling and composition.

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

This project uses **CSharp_Result** (CarboxylicLithium) for the Result monad pattern:

```csharp
// Result<T> is a struct that wraps either success (TSucc) or failure (Exception)
public readonly struct Result<TSucc>
{
  private readonly TSucc? _value;
  private readonly Exception? _exception;
  private readonly bool _isSuccess;
}
```

**Key Operations**:

- `Result.Ok(value)` or `new Result<T>(value)` - Create success
- `new Result<T>(exception)` - Create failure
- `.Then()` - Chain synchronous operations
- `.ThenAwait()` - Chain async operations
- `.IsSuccess()` - Check if successful
- `.Get()` - Extract success value (throws if failure)
- `.FailureOrDefault()` - Extract exception

**Error Mapping Predicates** (from CarboxylicLithium):

```csharp
// Errors.MapNone - Let exceptions propagate through Result chain
.Then(x => x.ToRes(), Errors.MapNone)

// Errors.MapAll - Convert all exceptions to Result failures
.Then(x => x.ToRes(), Errors.MapAll)

// Errors.MapIfExceptionIs<T>() - Convert only specific exception types
.Then(x => x.ToRes(), Errors.MapIfExceptionIs<NotFoundException>())
```

## Concepts

- **Problem type**: implement `IDomainProblem` (from CarboxylicBoron) with stable identity (Id/Title/Version) and structured fields.
- **Transport**: wrap problems in `DomainProblemException` so they propagate through Result pipelines.
- **Mapping**: `AtomiControllerBase.MapException()` converts exceptions to status codes; `ProblemDetailsService` renders RFC7807.

## Steps to Define an Error

1. Define a Problem (implements IDomainProblem)

```csharp
// App/Error/V1/UploadTooSmall.cs
[Description("This error represents an upload that is too small")]
public class UploadTooSmall : IDomainProblem
{
  public UploadTooSmall() { }

  public UploadTooSmall(string detail, long minimumBytes)
  {
    this.Detail = detail;
    this.MinimumBytes = minimumBytes;
  }

  [JsonIgnore, JsonSchemaIgnore]
  public string Id => "upload_too_small";

  [JsonIgnore, JsonSchemaIgnore]
  public string Title => "Upload Too Small";

  [JsonIgnore, JsonSchemaIgnore]
  public string Version => "v1";

  [JsonIgnore, JsonSchemaIgnore]
  public string Namespace => "atomi.zinc";

  public string Detail { get; } = "File size below minimum";

  [Description("Minimum file size in bytes")]
  public long MinimumBytes { get; } = 1024;
}
```

2. Throw/return as a failure

```csharp
// In domain/data/service code - return as Result failure
return new DomainProblemException(new UploadTooSmall(
  "File must be at least 1KB",
  1024
)).ToResult<T>();

// or extension:
return myProblem.ToException();

// or throw if not in Result chain:
throw new DomainProblemException(new UploadTooSmall(...));
```

3. Map to HTTP in controllers

```csharp
// Use base helpers to unwrap Result and map to HTTP
return this.ReturnResult(result);
// or this.ReturnNullableResult(result, new EntityNotFound(...));
// or this.ReturnUnitResult(result);
// or this.ReturnUnitNullableResult(result, new EntityNotFound(...));
```

## Error Mapper Flow in BaseController

The `MapException()` method in `AtomiControllerBase` uses a switch pattern to map exceptions to HTTP status codes:

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

## Custom Exceptions

**Domain Layer Exceptions**:

- `NotFoundException` - Entity not found by identifier (mapped to `EntityNotFound` → 404)

**API Layer Exceptions**:

- `DomainProblemException` - Wraps any `IDomainProblem` for throwing through Result chains

**Infrastructure Exceptions**:

- `DbUpdateException`, `SqlException`, etc. - Should be caught at repository boundaries and converted to domain problems

## Catch vs Throw

- Use `Result<T>` to compose operations; when a business rule fails, return `DomainProblemException` to carry context.
- Catch infrastructure exceptions at boundaries (e.g., EF unique constraint in repositories) and convert to Problems:

```csharp
// Example from repository
try
{
  await _dbContext.SaveChangesAsync();
  return entity;
}
catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
{
  return new DomainProblemException(new EntityConflict(
    "User with this username already exists",
    typeof(User),
    username
  ));
}
```

- Prefer returning Problems over generic exceptions so they are mapped meaningfully.
- Use error mapping predicates to control exception handling:
  - `Errors.MapNone` - Let domain problems propagate, map infrastructure exceptions
  - `Errors.MapAll` - Convert all exceptions to Result failures
  - `Errors.MapIfExceptionIs<T>()` - Convert only specific types

## Where Mapping Happens

- `App/Modules/Common/BaseController.cs:MapException()` - Switch on exception/problem → HTTP status code
- `App/StartUp/Services/ProblemDetailsService.cs` - Fill RFC7807 fields from Problem and attach `data` property

Related

- Problems concept: ../concepts/Problem.md
- Result pipelines: ../concepts/Result.md
- Guards and authorization: ../concepts/Guards.md
