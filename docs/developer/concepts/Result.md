# Result (CSharp_Result)

Make success/failure explicit, and compose work like a pipeline instead of throwing for control flow.

Why

- Improves readability: each step either transforms the success value or stops at the first failure.
- Safer error handling: failures carry context (often a Problem) and map cleanly to HTTP.

Where

- Library: `CSharp_Result` (extension methods like `.Then`, `.ThenAwait`, `.DoAwait`).
- Helpers: `App/Utility/ValidationUtility.cs`, `App/Utility/Utils.cs`.

Mental model

- Think of `Result<T>` as “checked outcome”. Success flows to the next step; failure short‑circuits the chain.
- Exceptions are still used, but wrapped as failures (e.g., `new DomainProblemException(problem)`).

Common pattern

1. Validate input → `Result<Validated>`
2. Authorize (guards) → `Result<Unit>`
3. Execute domain/service → `Result<Domain>`
4. Map to DTO → `Result<Dto>`
5. Return via base controller → HTTP 2xx or Problem response

Example

```csharp
var res = await validator
  .ValidateAsyncResult(req, "Invalid CreateUserReq")
  .ThenAwait(v => tokenExtractor.ExtractFromToken(v.IdToken, v.AccessToken))
  .Then<UserToken, UserToken>(t => t.Sub == id ? t : new DomainProblemException(new InvalidUserToken()))
  .ThenAwait(t => service.Create(id, t.ToRecord(), null))
  .Then(x => x.ToRes(), Errors.MapAll);
return this.ReturnResult(res);
```

Do / Don’t

- Do return `new Unit()` for side‑effect‑only success paths.
- Do wrap domain problems as `DomainProblemException` and let the pipeline short‑circuit.
- Don’t throw raw exceptions for predictable domain cases; model them as Problems.

See also

- Problems: ./Problem.md — how failures become RFC7807 responses
- Define Errors: ../guides/DefineErrors.md — step‑by‑step error creation

Related

- Problems: ./Problem.md (how failures become RFC7807 responses)
- How to define errors: ../guides/DefineErrors.md
