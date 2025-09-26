# How to Define Errors

Goal

- Create clear, actionable errors that flow from domain/data to HTTP responses.

Concepts

- Problem type: implement `IDomainProblem` with stable identity (Id/Title/Version) and structured fields.
- Transport: wrap problems in `DomainProblemException` so they propagate through Result pipelines.
- Mapping: `AtomiControllerBase` converts exceptions to status codes; `ProblemDetailsService` renders RFC7807.

Steps

1. Define a Problem

```csharp
// App/Error/V1/UploadTooSmall.cs
public class UploadTooSmall : IDomainProblem {
  public string Id => "upload_too_small";
  public string Title => "Upload Too Small";
  public string Version => "v1";
  public string Detail { get; } = "File size below minimum";
  public long MinimumBytes { get; } = 1024;
}
```

2. Throw/return as a failure

```csharp
// In domain/data/service code
return new DomainProblemException(new UploadTooSmall());
// or extension: myProblem.ToException()
```

3. Map to HTTP in controllers

```csharp
// Use base helpers to unwrap Result and map to HTTP
return this.ReturnResult(result);
// or this.ReturnNullableResult(result, new EntityNotFound(...));
```

Catch vs Throw

- Use Result to compose; when a business rule fails, return/throw `DomainProblemException` to carry context.
- Catch infrastructure exceptions at boundaries (e.g., EF unique constraint) and convert to Problems (see `UserRepository.Create`).
- Prefer returning Problems over generic exceptions so they are mapped meaningfully.

Custom exceptions

- `DomainProblemException`: carry a Problem.
- `NotFoundException` (Domain): mapped to `EntityNotFound` in base controller.

Where mapping happens

- `App/Modules/Common/BaseController.cs` (switch on exception/problem → status code).
- `App/StartUp/Services/ProblemDetailsService.cs` (fill RFC7807 fields from Problem and attach `data`).

Related

- Problems concept: ../concepts/Problem.md
- Result pipelines: ../concepts/Result.md
- Guards and authorization: ../concepts/Guards.md
