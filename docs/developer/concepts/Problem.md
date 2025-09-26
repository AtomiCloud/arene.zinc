# Problems (Domain Errors as RFC7807)

Structured, versioned errors that become HTTP problem+json.

Why

- Consistent, debuggable errors: each has an `Id`, `Title`, and `Version` plus structured payload.
- Easy mapping to HTTP: the controller and ProblemDetailsService do the heavy lifting.

Where

- Contract: `App/Error/DomainProblem.cs` (interface), `DomainProblemException`.
- Implementations: `App/Error/V1/*` (e.g., `Unauthorized`, `EntityNotFound`, `ValidationError`).
- Mapping: `App/Modules/Common/BaseController.cs` (Exception → HTTP), `ProblemDetailsService` (RFC7807 payload).

How

1. Implement `IDomainProblem` (stable `Id`/`Title`/`Version`; keep message in `Detail`).
2. Wrap and propagate as `new DomainProblemException(problem)` from domain/data/service.
3. Use base controller helpers to map Result to HTTP response.

Minimal example

```csharp
public class UploadTooSmall : IDomainProblem {
  public string Id => "upload_too_small";
  public string Title => "Upload Too Small";
  public string Version => "v1";
  public string Detail { get; } = "File size below minimum";
  public long MinimumBytes { get; } = 1024;
}

// Use it
return new DomainProblemException(new UploadTooSmall());
```

Example HTTP response (problem+json)

```json
{
  "type": "http://localhost:3000/docs/lapras/aldehyde/manganese/api/v1/upload_too_small",
  "title": "Upload Too Small",
  "status": 400,
  "detail": "File size below minimum",
  "data": {
    "minimumBytes": 1024
  }
}
```

Tips

- Keep `Id` stable across versions; evolve schemas by adding fields or bumping `Version` folder (e.g., V1 → V2).
- Put transport concerns (Problems) in `App/Error/*`, not the pure domain.

See also

- Result: ./Result.md — how problems flow through pipelines
- Define Errors: ../guides/DefineErrors.md — practical guide to create/mapping errors

Mapping

- Base controller maps exceptions to status codes (e.g., `EntityNotFound` → 404, `Unauthorized` → 403/401).
- `ProblemDetailsService` copies the Problem fields into the RFC7807 response.

Related

- Result: ./Result.md (how problems flow through pipelines)
- How to define errors: ../guides/DefineErrors.md
