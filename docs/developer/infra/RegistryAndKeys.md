# Registry & Keys

Think of the Registry classes as the “source of truth” for names you use in YAML. You can add as many entries as you like in configuration — just give each one a matching constant in code and always reference the constant in your code.

Files

- App/StartUp/Registry/Databases.cs
- App/StartUp/Registry/Caches.cs
- App/StartUp/Registry/BlockStorages.cs
- App/StartUp/Registry/HttpClients.cs
- App/StartUp/Registry/SmtpProviders.cs
- App/StartUp/Registry/CorsPolicies.cs
- App/StartUp/Registry/AuthPolicies.cs

Why this matters

- Avoids typos: if you misspell a key in YAML or code, things won’t wire up.
- Makes usage explicit: constants are grep‑able and safe to refactor.

Pattern to add a new key

1. Add the key to the appropriate Registry.
2. Add the corresponding YAML section using that key.
3. Use the constant in code (e.g., `HttpClients.Logto`, `BlockStorages.Main`).
4. (Optional) Options validations fail fast when keys don’t match.

Example (HttpClient)

```csharp
// 1) Registry
public static class HttpClients {
  public const string Billing = "Billing"; // App/StartUp/Registry/HttpClients.cs
}

// 2) YAML
// App/Config/settings.yaml
HttpClient:
  Billing:
    BaseAddress: https://billing.example.com
    Timeout: 30

// 3) Use in code (never raw strings)
public class BillingService(IHttpClientFactory f) {
  public Task<HttpClient> Client() => Task.FromResult(f.CreateClient(HttpClients.Billing));
}
```

See component docs for per‑service details.

Quick reference

| Registry (code)                      | YAML section             | Example constant → YAML key                     |
| ------------------------------------ | ------------------------ | ----------------------------------------------- |
| `App/StartUp/Registry/Databases`     | `Database:`              | `MainDbContext.Key` → `MAIN`                    |
| `App/StartUp/Registry/BlockStorages` | `BlockStorage:`          | `BlockStorages.Main` → `MAIN`                   |
| `App/StartUp/Registry/HttpClients`   | `HttpClient:`            | `HttpClients.Logto` → `Logto`                   |
| `App/StartUp/Registry/Caches`        | `Cache:`                 | `Caches.Main` → `MAIN`                          |
| `App/StartUp/Registry/SmtpProviders` | `Smtp:`                  | `SmtpProviders.Transactional` → `TRANSACTIONAL` |
| `App/StartUp/Registry/CorsPolicies`  | `Cors[].Name`            | `CorsPolicies.AllowAll` → `AllowAll`            |
| `App/StartUp/Registry/AuthPolicies`  | `Auth.Settings.Policies` | `AuthPolicies.OnlyAdmin` → `OnlyAdmin`          |

Common mistakes

- Using raw strings in code instead of constants → brittle and hard to search.
- Adding YAML without adding a Registry constant → code can’t reliably reference it.
