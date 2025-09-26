# HttpClient (Named Clients)

Why

- External service access with consistent base addresses, timeouts, and auth.
- You can configure multiple clients; each YAML key must match a constant in `HttpClients`.

Where it lives

- Service: `App/StartUp/Services/HttpClientService.cs`.
- Options: `App/StartUp/Options/HttpClientOption.cs`.
- Registry: `App/StartUp/Registry/HttpClients.cs`.

Define keys

```csharp
// App/StartUp/Registry/HttpClients.cs
public static class HttpClients {
  public const string Main = "Main";
  public const string Logto = "Logto";
}
```

Configure (YAML)

```yaml
HttpClient:
  Logto:
    BaseAddress: https://auth.example.com
    Timeout: 60
    BearerAuth: ${TOKEN}
```

Key matching

- Ensure the YAML key (e.g., `Logto`) matches `HttpClients.Logto`.
- Add more clients by adding constants and YAML entries (e.g., `Billing`).

Use

```csharp
public class MyService(IHttpClientFactory f) {
  public async Task<string> Ping() {
    var client = f.CreateClient(HttpClients.Logto);
    var res = await client.GetAsync("/.well-known/openid-configuration");
    res.EnsureSuccessStatusCode();
    return await res.Content.ReadAsStringAsync();
  }
}
```
