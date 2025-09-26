# Guards (Authorization Helpers)

Protect endpoints with reusable checks that read the caller’s identity and claims.

Why

- Keep controller logic safe and readable by centralizing auth checks.
- Combine static policies (compile‑time) with dynamic guards (runtime context like target user ID).

Where

- Base controller: `App/Modules/Common/BaseController.cs` (Guard, GuardOrAll, GuardOrAny).
- Auth helper: `App/StartUp/Services/Auth/AuthHelper.cs`.
- Policies: `App/StartUp/Registry/AuthPolicies.cs` and configured in YAML.

How

- Static: decorate endpoints with `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`.
- Dynamic: call guard methods in actions; they return `Result<Unit>` so they integrate with pipelines.

Example

```csharp
// Allow if requester is the target user or has admin role
var res = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
  .ThenAwait(_ => service.GetById(userId))
  .Then(x => x?.ToRes(), Errors.MapAll);
return this.ReturnNullableResult(res, new EntityNotFound("User Not Found", typeof(User), userId));
```

FAQ

- When do I use policies vs guards?
  - Use policies for static, reusable rules (e.g., Admin only).
  - Use guards when the rule depends on route/body context (e.g., user can access only their own resource).

See also

- Auth policies and YAML: ../infra/AuthPolicies.md
- Problems: ./Problem.md (Unauthorized/Unauthenticated)
- Result pipelines: ./Result.md

Related

- Auth policies and YAML: ../infra/AuthPolicies.md
- Problems: ./Problem.md
- Result pipelines: ./Result.md
