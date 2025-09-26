# CORS Policies

Why

- Control which origins, methods, and headers can access your APIs.
- You can define multiple policies; each policy Name in YAML must match a constant in `CorsPolicies`.

Where it lives

- Service: `App/StartUp/Services/CorsService.cs`.
- Options: `App/StartUp/Options/CorsOption.cs`.
- Registry: `App/StartUp/Registry/CorsPolicies.cs`.

Define keys

```csharp
// App/StartUp/Registry/CorsPolicies.cs
public static class CorsPolicies {
  public const string AllowAll = "AllowAll";
}
```

Configure (YAML)

```yaml
Cors:
  - Name: AllowAll
    PreflightMaxAge: 600
    Origins: ['*']
    Headers: ['*']
    Methods: ['*']
App:
  DefaultCors: AllowAll
```

Key matching

- Ensure each YAML policy Name (e.g., `AllowAll`) matches a constant in `CorsPolicies`.
- Set `App.DefaultCors` to one of the declared constants.
