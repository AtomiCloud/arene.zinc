# Infrastructure Reference

Links to official documentation, guides, and related resources for infrastructure services in Zinc.

## Internal Documentation

### Infrastructure Guides

- **[Registry & Keys](../../../docs/developer/infra/RegistryAndKeys.md)** - Registry constants pattern
- **[Database](../../../docs/developer/infra/Database.md)** - PostgreSQL and EF Core setup
- **[Cache](../../../docs/developer/infra/Cache.md)** - Redis/Dragonfly configuration
- **[Block Storage](../../../docs/developer/infra/BlockStorage.md)** - MinIO/S3 file storage
- **[HttpClient](../../../docs/developer/infra/HttpClient.md)** - External API clients
- **[SMTP](../../../docs/developer/infra/Smtp.md)** - Email sending
- **[CORS](../../../docs/developer/infra/Cors.md)** - Cross-origin configuration
- **[Auth Policies](../../../docs/developer/infra/AuthPolicies.md)** - Authorization setup
- **[Telemetry](../../../docs/developer/infra/Telemetry.md)** - OpenTelemetry observability

### General Guides

- **[Architecture & Startup](../../../docs/developer/ArchitectureAndStartup.md)** - System architecture
- **[New Feature Walkthrough](../../../docs/developer/guides/NewFeatureWalkthrough.md)** - Adding features

### Other

- **[Dev & Tasks](../../../docs/developer/DevAndTasks.md)** - Development commands
- **[File Uploads](../../../docs/developer/files/Uploads.md)** - File handling patterns
- **[Security/Encryption](../../../docs/developer/security/Encryption.md)** - Data encryption

## Code Locations

### Registry Files

All registry files are in `App/StartUp/Registry/`:

- **`Databases.cs`** - Database connection constants
- **`Caches.cs`** - Cache endpoint constants
- **`BlockStorages.cs`** - Block storage bucket constants
- **`HttpClients.cs`** - HTTP client name constants
- **`SmtpProviders.cs`** - SMTP provider constants
- **`CorsPolicies.cs`** - CORS policy name constants
- **`AuthPolicies.cs`** - Authorization policy constants

### Options Files

All option classes are in `App/StartUp/Options/`:

- **`DatabaseOption.cs`** - Database configuration
- **`CacheOption.cs`** - Cache configuration
- **`BlockStorageOption.cs`** - Block storage configuration
- **`HttpClientOption.cs`** - HTTP client configuration
- **`SmtpOption.cs`** - SMTP configuration
- **`CorsOption.cs`** - CORS configuration
- **`AuthOption.cs`** - Authentication and authorization configuration

### Service Files

Infrastructure service implementations are in `App/StartUp/Services/`:

- **`HttpClientService.cs`** - HTTP client factory setup
- **`ProblemDetailsService.cs`** - Error response formatting
- **`Auth/`** - Authentication services

### Configuration Files

- **Base**: `App/Config/settings.yaml`
- **Environment overrides**: `App/Config/settings.<landscape>.yaml`
- **Examples**: `App/Config/settings.corsola.yaml`, `App/Config/settings.lapras.yaml`

## External Documentation

### Database (Entity Framework Core)

- **[EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)** - Official Microsoft docs
- **[DbContext](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)** - DbContext configuration
- **[Connection Strings](https://www.npgsql.org/doc/connection-string-parameters.html)** - Npgsql (PostgreSQL) connection strings
- **[Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)** - EF Core migrations
- **[Connection Resilience](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency)** - Retry logic

### Cache (Redis/StackExchange.Redis)

- **[StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/)** - Redis client library
- **[IConnectionMultiplexer](https://stackexchange.github.io/StackExchange.Redis/Basics.html)** - Connection management
- **[IDistributedCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)** - Distributed caching
- **[Redis Commands](https://redis.io/commands/)** - Redis command reference
- **[Dragonfly](https://www.dragonflydb.io/)** - Redis-compatible in-memory datastore

### Block Storage (MinIO/S3)

- **[MinIO Client SDK](https://min.io/docs/minio/linux/developers/dotnet/minio-dotnet.html)** - .NET SDK
- **[Amazon S3](https://docs.aws.amazon.com/s3/)** - S3 documentation
- **[Minio.AspNetCore](https://github.com/appany/Minio.AspNetCore)** - ASP.NET Core integration

### HTTP Client

- **[IHttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)** - HTTP client factory
- **[Named Clients](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#named-clients)** - Named HTTP clients
- **[Polly](https://github.com/App-vNext/Polly)** - Resilience and transient-fault-handling
- **[Retry Policies](https://github.com/App-vNext/Polly#retry)** - Polly retry patterns
- **[Circuit Breaker](https://github.com/App-vNext/Polly#circuit-breaker)** - Polly circuit breaker

### SMTP (Email)

- **[MailKit](https://github.com/jstedfast/MailKit)** - SMTP client library
- **[MimeKit](https://github.com/jstedfast/MimeKit)** - MIME message creation
- **[SendGrid](https://docs.sendgrid.com/)** - SendGrid email service
- **[Handlebars.Net](https://github.com/Handlebars-Net/Handlebars.Net)** - Email templating

### CORS

- **[Enable CORS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)** - CORS setup
- **[CORS Specification](https://www.w3.org/TR/cors/)** - W3C CORS standard
- **[CORS MDN](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)** - CORS overview

### Authorization

- **[Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)** - Authorization overview
- **[Policy-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)** - Policy setup
- **[Claims-based Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)** - Claims handling
- **[JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)** - JWT setup

## Configuration Patterns

### YAML Structure

```yaml
# Top-level keys match infrastructure types
Database:
  KEY_NAME: # Matches Registry constant
    ConnectionString: '...'
    # ... other options

Cache:
  KEY_NAME:
    Endpoints: '...'
    # ... other options

BlockStorage:
  KEY_NAME:
    Endpoint: '...'
    # ... other options

HttpClient:
  KEY_NAME:
    BaseAddress: '...'
    # ... other options

Smtp:
  KEY_NAME:
    Host: '...'
    # ... other options

Cors:
  - Name: KEY_NAME # Array of policies
    AllowedOrigins: ['...']
    # ... other options

Auth:
  Policies:
    KEY_NAME: ['scope1', 'scope2'] # Dictionary of policies
```

### Environment Variables

Use `${VARIABLE}` syntax in YAML:

```yaml
Database:
  MAIN:
    ConnectionString: 'Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}'

Cache:
  MAIN:
    Password: ${REDIS_PASSWORD}

HttpClient:
  Billing:
    BearerAuth: ${BILLING_API_KEY}
```

### Environment-Specific Overrides

Base configuration in `settings.yaml`, overrides in `settings.<landscape>.yaml`:

```yaml
# settings.yaml
HttpClient:
  Billing:
    BaseAddress: https://billing-dev.example.com
    Timeout: 30
```

```yaml
# settings.production.yaml
HttpClient:
  Billing:
    BaseAddress: https://billing.example.com
    Timeout: 60
```

## Registry Pattern Quick Reference

### Step 1: Add Constant

```csharp
// App/StartUp/Registry/HttpClients.cs
public static class HttpClients
{
  public const string NewService = "NewService";
}
```

### Step 2: Add YAML

```yaml
# App/Config/settings.yaml
HttpClient:
  NewService:
    BaseAddress: https://newservice.example.com
    Timeout: 30
```

### Step 3: Use Constant

```csharp
// Domain/Service.cs
public class MyService(IHttpClientFactory factory)
{
  public async Task DoSomething()
  {
    var client = factory.CreateClient(HttpClients.NewService);
    // ... use client
  }
}
```

## Common HTTP Client Patterns

### Basic GET Request

```csharp
var client = factory.CreateClient(HttpClients.Service);
var response = await client.GetAsync("/endpoint");
response.EnsureSuccessStatusCode();
var data = await response.Content.ReadFromJsonAsync<DataType>();
```

### POST with JSON Body

```csharp
var client = factory.CreateClient(HttpClients.Service);
var response = await client.PostAsJsonAsync("/endpoint", requestData);
response.EnsureSuccessStatusCode();
```

### Error Handling

```csharp
try
{
  var client = factory.CreateClient(HttpClients.Service);
  var response = await client.GetAsync("/endpoint");

  if (!response.IsSuccessStatusCode)
  {
    logger.LogWarning("Request failed: {StatusCode}", response.StatusCode);
    return new ExternalServiceError($"Service returned {response.StatusCode}");
  }

  var data = await response.Content.ReadFromJsonAsync<DataType>();
  return Result.Ok(data);
}
catch (HttpRequestException ex)
{
  logger.LogError(ex, "HTTP error");
  return new ExternalServiceError("Failed to connect");
}
catch (TaskCanceledException ex)
{
  logger.LogError(ex, "Timeout");
  return new ExternalServiceError("Request timed out");
}
```

## Common Cache Patterns

### String Operations

```csharp
var db = redis.GetDatabase();

// Get
var value = await db.StringGetAsync("key");

// Set with expiry
await db.StringSetAsync("key", "value", TimeSpan.FromMinutes(15));

// Delete
await db.KeyDeleteAsync("key");
```

### JSON Caching

```csharp
var db = redis.GetDatabase();

// Get
var json = await db.StringGetAsync("key");
var data = json.HasValue ? JsonSerializer.Deserialize<DataType>(json) : null;

// Set
var json = JsonSerializer.Serialize(data);
await db.StringSetAsync("key", json, TimeSpan.FromMinutes(15));
```

## Tools & Commands

### Generate Encryption Key

```bash
pls gen:encryption-key
```

### Test Infrastructure Connections

```bash
# Test database connection
pls exec -- dotnet ef database drop
pls exec -- dotnet ef database update

# Test cache connection
redis-cli -h localhost -p 6379 ping

# Test MinIO connection
mc alias set local http://localhost:9000 accesskey secretkey
mc ls local
```

## Related Skills

- **[feature-module](../feature-module/SKILL.md)** - Adding features with infrastructure
- **[error-handling](../error-handling/SKILL.md)** - Handling infrastructure errors

## Quick Links

- [Project README](../../../README.md)
- [CLAUDE.md](../../../CLAUDE.md) - Claude Code instructions
- [AGENTS.md](../../../AGENTS.md) - Repository guidelines
- [Taskfile.yaml](../../../Taskfile.yaml) - Available tasks

## Common Mistakes

### ❌ Using Raw Strings

```csharp
// DON'T
var client = factory.CreateClient("Billing");

// DO
var client = factory.CreateClient(HttpClients.Billing);
```

### ❌ Mismatched Keys

```csharp
// Registry
public const string Billing = "BILLING";

// YAML (WRONG - case mismatch)
HttpClient:
  Billing:  # Should be BILLING
    BaseAddress: ...
```

### ❌ Missing Registry Constant

```yaml
# YAML
HttpClient:
  NewService: # No constant defined
    BaseAddress: ...
```

```csharp
// Code (WRONG - hardcoded)
var client = factory.CreateClient("NewService");
```

### ❌ Hardcoded Secrets

```yaml
# DON'T
Database:
  MAIN:
    ConnectionString: "Host=localhost;Password=supersecret123"

# DO
Database:
  MAIN:
    ConnectionString: "Host=localhost;Password=${DB_PASSWORD}"
```

## Troubleshooting Guide

### Configuration not loading

1. Check YAML syntax (indentation, colons)
2. Verify key matches Registry constant exactly
3. Check `LANDSCAPE` environment variable
4. Look for typos in environment variables

### Connection failures

1. Verify service is running and accessible
2. Check credentials are correct
3. Test network connectivity
4. Review timeout settings
5. Check firewall rules

### DI registration errors

1. Ensure Options class is registered
2. Verify service is added to DI container
3. Check for circular dependencies
4. Review constructor parameters

### HTTP client errors

1. Check base address is correct
2. Verify timeout is reasonable
3. Review retry and circuit breaker policies
4. Check for network issues
5. Validate authentication headers
