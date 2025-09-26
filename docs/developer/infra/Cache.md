# Cache (Redis/Dragonfly)

Why

- Low‑latency storage for sessions, tokens, and transient data.
- You can configure multiple caches; each YAML key must match a constant in `Caches`.

Where it lives

- Service: `App/StartUp/Services/CacheService.cs`.
- Options: `App/StartUp/Options/CacheOption.cs`.
- Registry: `App/StartUp/Registry/Caches.cs`.

Define keys

```csharp
// App/StartUp/Registry/Caches.cs
public static class Caches {
  public const string Main = "MAIN";
}
```

Configure (YAML)

```yaml
Cache:
  MAIN:
    Endpoints: [localhost:6379]
    Password: supersecret
    SSL: false
    ConnectTimeout: 5000
    SyncTimeout: 5000
```

Key matching

- Ensure the YAML key (e.g., `MAIN`) matches `Caches.Main`.
- Add more caches by adding constants and YAML entries.

Use

```csharp
// Registered by .AddCache(options)
// Resolve via StackExchange.Redis.Extensions factory for default connection
```
