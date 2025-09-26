# Validation (FluentValidation)

Reject bad input early and consistently, with clear messages for clients.

Why

- Keep controllers lean and provide consistent, descriptive error payloads for invalid requests.

Where

- Validators: near APIs (e.g., `App/Modules/Users/API/V1/*Validator.cs`).
- Helpers: `App/Utility/ValidationUtility.cs` (maps to `ValidationError`).

Create a validator

```csharp
public class CreateUserReqValidator : AbstractValidator<CreateUserReq> {
  public CreateUserReqValidator() {
    RuleFor(x => x.IdToken).NotEmpty();
    RuleFor(x => x.AccessToken).NotEmpty();
  }
}
```

Use in controllers

```csharp
var res = await createUserReqValidator
  .ValidateAsyncResult(req, "Invalid CreateUserReq");
```

Problem mapping

- `ValidateAsyncResult` returns `ValidationError` (Problem) on failure, which `BaseController` maps to 400 and `ProblemDetailsService` renders.

Tips

- Keep messages actionable (what field, what rule, expected format).
- Validate both structure (types, lengths) and semantics (allowed values).
