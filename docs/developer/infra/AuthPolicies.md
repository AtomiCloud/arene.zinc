# Auth Policies & Guards

Why

- Centralize authorization rules and reuse them consistently across controllers. Guards enable contextual checks beyond static policies.

Where it lives

- Services: `App/StartUp/Services/Auth/*`, `AuthService.cs`, `AuthHelper.cs`.
- Options: `App/StartUp/Options/Auth/*`.
- Registry: `App/StartUp/Registry/AuthPolicies.cs` (policy names) and `AuthRoles` (claim fields).

Registry keys

```csharp
// App/StartUp/Registry/AuthPolicies.cs
public class AuthPolicies {
  public const string OnlyAdmin = "OnlyAdmin";
  public const string Registered = "Registered";
}
```

Configure (YAML)

```yaml
Auth:
  Enabled: true
  Settings:
    Issuer: https://auth.example.com
    Audience: https://api.example.com
    Policies:
      OnlyAdmin:
        Type: All
        Field: roles
        Target: [admin]
```

Validation

```csharp
services.RegisterOption<AuthOption>(AuthOption.Key)
  .Validate(c => c.Settings?.Policies?.All(x => AuthPolicies.Any(d => d == x.Key)) ?? true,
    "Auth.Settings.Policies.Key must be declared in AuthPolicies (Registry)");
```

Use

```csharp
// Allow if requester is target OR has admin role
await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
  .ThenAwait(_ => service.GetById(userId));
```

ITokenDataExtractor

```csharp
var token = await extractor.ExtractFromToken(idToken, accessToken);
```
