---
name: infrastructure
description: Add infrastructure services (Database, Cache, BlockStorage, HttpClient, SMTP) to Zinc ASP.NET Core 8 API using Registry constants and YAML configuration
---

# Infrastructure Skill

Use this skill when adding infrastructure services to the Zinc ASP.NET Core 8 API project.

## Related Documentation

- **[examples.md](examples.md)** - Complete code examples for each infrastructure type
- **[reference.md](reference.md)** - Links to official documentation

## Overview

Zinc infrastructure services (Database, Cache, BlockStorage, HttpClient, SMTP, CORS, Auth) are configured using a Registry-and-YAML pattern. This ensures type-safe configuration keys and prevents typos.

## Core Principle: Registry Constants

**NEVER use raw strings in code.** Always:

1. Add constant to appropriate Registry class
2. Add matching YAML configuration
3. Reference via Registry constant in code

This pattern is grep-able, refactor-safe, and fail-fast.

## Infrastructure Types

### Available Infrastructure

| Type          | Registry                 | YAML Section     | Use Case               |
| ------------- | ------------------------ | ---------------- | ---------------------- |
| Database      | `Registry.Databases`     | `Database:`      | PostgreSQL via EF Core |
| Cache         | `Registry.Caches`        | `Cache:`         | Redis/Dragonfly        |
| Block Storage | `Registry.BlockStorages` | `BlockStorage:`  | MinIO/S3 file storage  |
| HTTP Client   | `Registry.HttpClients`   | `HttpClient:`    | External API calls     |
| SMTP          | `Registry.SmtpProviders` | `Smtp:`          | Email sending          |
| CORS          | `Registry.CorsPolicies`  | `Cors:`          | Cross-origin policies  |
| Auth          | `Registry.AuthPolicies`  | `Auth:Policies:` | Authorization policies |

## Adding Infrastructure Services

### Step 1: Add Registry Constant

Add a constant to the appropriate Registry file:

```csharp
// App/StartUp/Registry/HttpClients.cs
public static class HttpClients
{
  public const string Main = "Main";
  public const string Logto = "Logto";
  public const string Billing = "Billing";  // NEW
}
```

**Registry File Locations**:

- `App/StartUp/Registry/Databases.cs`
- `App/StartUp/Registry/Caches.cs`
- `App/StartUp/Registry/BlockStorages.cs`
- `App/StartUp/Registry/HttpClients.cs`
- `App/StartUp/Registry/SmtpProviders.cs`
- `App/StartUp/Registry/CorsPolicies.cs`
- `App/StartUp/Registry/AuthPolicies.cs`

### Step 2: Add YAML Configuration

Add matching configuration in `App/Config/settings.yaml`:

```yaml
HttpClient:
  Billing: # Matches HttpClients.Billing constant
    BaseAddress: https://billing.example.com
    Timeout: 30
    BearerAuth: ${BILLING_TOKEN}
```

**Environment-specific overrides**: `App/Config/settings.<landscape>.yaml`

### Step 3: Use in Code

Always reference via Registry constant:

```csharp
public class BillingService(IHttpClientFactory factory)
{
  public async Task<BillingData> FetchInvoice(string id)
  {
    // ✅ Use Registry constant
    var client = factory.CreateClient(HttpClients.Billing);

    // ❌ NEVER use raw string
    // var client = factory.CreateClient("Billing");

    var response = await client.GetAsync($"/invoices/{id}");
    return await response.Content.ReadFromJsonAsync<BillingData>();
  }
}
```

## Infrastructure Service Details

### Database (PostgreSQL via EF Core)

**Registry**: `Registry.Databases`
**YAML**: `Database:`
**Options**: `App/StartUp/Options/DatabaseOption.cs`

**When to use**:

- Primary data storage
- Relational data with transactions
- EF Core entity management

**Configuration**:

```yaml
Database:
  MAIN: # Matches MainDbContext constant
    ConnectionString: 'Host=localhost;Database=zinc;Username=user;Password=pass'
    AutoMigrate: true
    MaxRetries: 3
```

**Usage**:

```csharp
// Inject DbContext
public class WidgetRepository(MainDbContext db)
{
  public async Task<Widget?> GetById(Guid id) =>
    await db.Widgets.FindAsync(id);
}
```

See [examples.md#database](examples.md#database) for complete example.

### Cache (Redis/Dragonfly)

**Registry**: `Registry.Caches`
**YAML**: `Cache:`
**Options**: `App/StartUp/Options/CacheOption.cs`

**When to use**:

- Session storage
- Temporary data caching
- Rate limiting
- Distributed locks

**Configuration**:

```yaml
Cache:
  MAIN:
    Endpoints: localhost:6379
    Password: ${REDIS_PASSWORD}
    Ssl: false
```

**Usage**:

```csharp
// Inject IConnectionMultiplexer or IDistributedCache
public class SessionService(IConnectionMultiplexer redis)
{
  public async Task<string?> GetSession(string userId)
  {
    var db = redis.GetDatabase();
    return await db.StringGetAsync($"session:{userId}");
  }
}
```

See [examples.md#cache](examples.md#cache) for complete example.

### Block Storage (MinIO/S3)

**Registry**: `Registry.BlockStorages`
**YAML**: `BlockStorage:`
**Options**: `App/StartUp/Options/BlockStorageOption.cs`

**When to use**:

- File uploads (images, documents, videos)
- Large binary data
- Static assets

**Configuration**:

```yaml
BlockStorage:
  MAIN:
    Endpoint: minio.example.com:9000
    AccessKey: ${MINIO_ACCESS_KEY}
    SecretKey: ${MINIO_SECRET_KEY}
    Bucket: zinc-uploads
    Secure: true
```

**Usage**:

```csharp
// Inject IFileRepository and IFileValidator
public class FileUploadService(
  IFileRepository fileRepo,
  IFileValidator fileValidator)
{
  public async Task<Result<string>> Upload(IFormFile file)
  {
    // Validate file
    var validation = await fileValidator.Validate(file);
    if (validation.IsFailure())
      return validation.FailureOrDefault();

    // Upload to storage
    var fileId = Guid.NewGuid().ToString();
    await fileRepo.Upload(fileId, file.OpenReadStream());
    return Result.Ok(fileId);
  }
}
```

See [examples.md#block-storage](examples.md#block-storage) for complete example.

### HTTP Client (External APIs)

**Registry**: `Registry.HttpClients`
**YAML**: `HttpClient:`
**Options**: `App/StartUp/Options/HttpClientOption.cs`

**When to use**:

- External API calls
- Third-party integrations
- Microservice communication

**Configuration**:

```yaml
HttpClient:
  Logto:
    BaseAddress: https://auth.example.com
    Timeout: 60
    BearerAuth: ${AUTH_TOKEN}
```

**Usage**:

```csharp
// Inject IHttpClientFactory
public class AuthService(IHttpClientFactory factory)
{
  public async Task<UserInfo> GetUserInfo(string userId)
  {
    var client = factory.CreateClient(HttpClients.Logto);
    var response = await client.GetAsync($"/users/{userId}");
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<UserInfo>();
  }
}
```

See [examples.md#http-client](examples.md#http-client) for complete example.

### SMTP (Email)

**Registry**: `Registry.SmtpProviders`
**YAML**: `Smtp:`
**Options**: `App/StartUp/Options/SmtpOption.cs`

**When to use**:

- Transactional emails
- Notifications
- Password resets
- Invitations

**Configuration**:

```yaml
Smtp:
  TRANSACTIONAL:
    Host: smtp.sendgrid.net
    Port: 587
    Username: apikey
    Password: ${SENDGRID_API_KEY}
    From: noreply@example.com
    EnableSsl: true
```

**Usage**:

```csharp
// Inject ISmtpClientFactory and IEmailRenderer
public class EmailService(
  ISmtpClientFactory smtpFactory,
  IEmailRenderer renderer)
{
  public async Task SendWelcomeEmail(string toEmail, string userName)
  {
    // Render template
    var html = await renderer.Render("welcome", new { Name = userName });

    // Send email
    var message = new SmtpEmailMessage
    {
      To = [toEmail],
      Subject = "Welcome!",
      HtmlBody = html
    };

    var client = await smtpFactory.CreateClient(SmtpProviders.Transactional);
    await client.SendAsync(message);
  }
}
```

See [examples.md#smtp](examples.md#smtp) for complete example.

### CORS (Cross-Origin Resource Sharing)

**Registry**: `Registry.CorsPolicies`
**YAML**: `Cors:`
**Options**: `App/StartUp/Options/CorsOption.cs`

**When to use**:

- Frontend applications on different domains
- Browser-based API access
- Development environments

**Configuration**:

```yaml
Cors:
  - Name: AllowAll
    AllowedOrigins: ['*']
    AllowedMethods: ['GET', 'POST', 'PUT', 'DELETE']
    AllowedHeaders: ['*']
    AllowCredentials: false
```

**Usage**:

```csharp
// In Program.cs / Server.cs
app.UseCors(CorsPolicies.AllowAll);

// Or on controller
[EnableCors(CorsPolicies.AllowAll)]
public class WidgetController : ControllerBase
{
  // ...
}
```

See [examples.md#cors](examples.md#cors) for complete example.

### Auth Policies (Authorization)

**Registry**: `Registry.AuthPolicies`
**YAML**: `Auth:Policies:`
**Options**: `App/StartUp/Options/AuthOption.cs`

**When to use**:

- Role-based access control
- Permission checks
- Resource authorization

**Configuration**:

```yaml
Auth:
  Policies:
    ReadWidgets: ['read:widgets']
    WriteWidgets: ['write:widgets']
    AdminOnly: ['admin', 'super:admin']
```

**Usage**:

```csharp
// In controller
[Authorize(Policy = AuthPolicies.WriteWidgets)]
public class WidgetController : AtomiControllerBase
{
  [HttpPost]
  public async Task<ActionResult> Create([FromBody] WidgetCreateReq req)
  {
    var principal = await GetPrincipal();
    Guard(principal, AuthPolicies.WriteWidgets);
    // ... create widget
  }
}
```

See [examples.md#auth-policies](examples.md#auth-policies) for complete example.

## Common Patterns

### Multiple Instances

You can configure multiple instances of the same infrastructure type:

```yaml
HttpClient:
  Billing:
    BaseAddress: https://billing.example.com
    Timeout: 30
  Analytics:
    BaseAddress: https://analytics.example.com
    Timeout: 60
  Notifications:
    BaseAddress: https://notifications.example.com
    Timeout: 15
```

```csharp
public static class HttpClients
{
  public const string Billing = "Billing";
  public const string Analytics = "Analytics";
  public const string Notifications = "Notifications";
}
```

### Environment-Specific Overrides

Base configuration in `settings.yaml`, overrides in `settings.<landscape>.yaml`:

```yaml
# settings.yaml (base)
HttpClient:
  Billing:
    BaseAddress: https://billing-dev.example.com
    Timeout: 30
```

```yaml
# settings.production.yaml (override)
HttpClient:
  Billing:
    BaseAddress: https://billing.example.com
    Timeout: 60
```

### Environment Variables

Use `${VARIABLE}` syntax for secrets:

```yaml
Database:
  MAIN:
    ConnectionString: 'Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}'
```

## Best Practices

### DO

- ✅ Always use Registry constants, never raw strings
- ✅ Keep secrets in environment variables
- ✅ Use environment-specific overrides for production
- ✅ Add timeouts to all HTTP clients
- ✅ Use meaningful constant names (e.g., `Billing`, not `Client1`)
- ✅ Document what each infrastructure instance is for
- ✅ Test infrastructure configuration in integration tests

### DON'T

- ❌ Use raw strings in code
- ❌ Hardcode credentials in YAML
- ❌ Reuse the same HTTP client for different services
- ❌ Forget to add Registry constants for new YAML keys
- ❌ Mix development and production credentials
- ❌ Ignore connection failures (add retries and circuit breakers)

## Checklist

When adding new infrastructure:

- [ ] Add constant to appropriate Registry file
- [ ] Add matching YAML configuration in `settings.yaml`
- [ ] Add environment-specific overrides if needed
- [ ] Use environment variables for secrets
- [ ] Reference via Registry constant in code
- [ ] Add DI registration if needed
- [ ] Test configuration loads correctly
- [ ] Test infrastructure service works
- [ ] Document what the service is for
- [ ] Update README or documentation

## Troubleshooting

### Configuration not loading

- Check YAML key matches Registry constant exactly (case-sensitive)
- Verify `LANDSCAPE` environment variable is set
- Look for YAML syntax errors (indentation, colons)
- Check environment variable substitution

### DI registration errors

- Ensure service is registered in `Server.cs` or `Program.cs`
- Check Options class is properly configured
- Verify interface and implementation are both registered

### Connection failures

- Check network connectivity
- Verify credentials are correct
- Look for firewall or security group issues
- Check timeout values are reasonable

### Environment variable not found

- Verify variable is set in shell/environment
- Check `.env` file if using one
- Use `echo $VARIABLE` to verify
- Check if variable needs `Atomi_` prefix

## Quick Start

1. **Read** [examples.md](examples.md) for complete code examples
2. **Reference** [reference.md](reference.md) for official documentation
3. **Add** Registry constant
4. **Configure** YAML with matching key
5. **Use** via Registry constant in code
6. **Test** infrastructure service
