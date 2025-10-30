# Infrastructure Examples

Complete code examples for adding infrastructure services to Zinc.

## Table of Contents

- [Database](#database)
- [Cache](#cache)
- [Block Storage](#block-storage)
- [HTTP Client](#http-client)
- [SMTP](#smtp)
- [CORS](#cors)
- [Auth Policies](#auth-policies)

## Database

### Add Registry Constant

```csharp
// App/StartUp/Registry/Databases.cs
namespace App.StartUp.Registry;

public static class Databases
{
  public const string Main = "MAIN";
  public const string Analytics = "ANALYTICS";  // NEW - for read-only analytics queries
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
Database:
  MAIN:
    ConnectionString: 'Host=localhost;Database=zinc;Username=zinc;Password=${DB_PASSWORD}'
    AutoMigrate: true
    MaxRetries: 3
    CommandTimeout: 30

  ANALYTICS: # NEW
    ConnectionString: 'Host=analytics.example.com;Database=zinc_readonly;Username=reader;Password=${ANALYTICS_DB_PASSWORD}'
    AutoMigrate: false
    MaxRetries: 5
    CommandTimeout: 60
```

### Create DbContext

```csharp
// App/StartUp/Database/AnalyticsDbContext.cs
using Microsoft.EntityFrameworkCore;
using App.Modules.Widgets.Data;

namespace App.StartUp.Database;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
  : DbContext(options)
{
  // Constant for Registry
  public const string Key = Databases.Analytics;

  // DbSets (read-only for analytics)
  public DbSet<WidgetData> Widgets { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    // Configure entities
  }
}
```

### Register in DI

```csharp
// App/StartUp/Server.cs or Program.cs
services.AddDatabase<AnalyticsDbContext>(
  config.GetDatabaseOptions(Databases.Analytics)
);
```

### Use in Repository

```csharp
// App/Modules/Widgets/Data/WidgetAnalyticsRepository.cs
using App.StartUp.Database;

public class WidgetAnalyticsRepository(AnalyticsDbContext db)
{
  public async Task<List<WidgetStats>> GetStats()
  {
    return await db.Widgets
      .GroupBy(w => w.Active)
      .Select(g => new WidgetStats
      {
        IsActive = g.Key,
        Count = g.Count()
      })
      .ToListAsync();
  }
}
```

## Cache

### Add Registry Constant

```csharp
// App/StartUp/Registry/Caches.cs
namespace App.StartUp.Registry;

public static class Caches
{
  public const string Main = "MAIN";
  public const string Session = "SESSION";  // NEW - for session storage
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
Cache:
  MAIN:
    Endpoints: localhost:6379
    Password: ${REDIS_PASSWORD}
    Ssl: false
    Database: 0

  SESSION: # NEW
    Endpoints: localhost:6379
    Password: ${REDIS_PASSWORD}
    Ssl: false
    Database: 1 # Separate database for sessions
```

### Use with IConnectionMultiplexer

```csharp
// Domain/Sessions/Service.cs
using StackExchange.Redis;
using Microsoft.Extensions.Options;

public class SessionService(
  IConnectionMultiplexer redis,
  IOptionsMonitor<CacheOption> cacheOptions)
{
  public async Task<string?> GetSession(string userId)
  {
    var db = redis.GetDatabase();
    var key = $"session:{userId}";
    return await db.StringGetAsync(key);
  }

  public async Task SetSession(string userId, string data, TimeSpan expiry)
  {
    var db = redis.GetDatabase();
    var key = $"session:{userId}";
    await db.StringSetAsync(key, data, expiry);
  }

  public async Task DeleteSession(string userId)
  {
    var db = redis.GetDatabase();
    var key = $"session:{userId}";
    await db.KeyDeleteAsync(key);
  }
}
```

### Use with IDistributedCache

```csharp
// Domain/Cache/Service.cs
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class CacheService(IDistributedCache cache)
{
  public async Task<T?> Get<T>(string key)
  {
    var json = await cache.GetStringAsync(key);
    return json is null ? default : JsonSerializer.Deserialize<T>(json);
  }

  public async Task Set<T>(string key, T value, TimeSpan expiry)
  {
    var json = JsonSerializer.Serialize(value);
    await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = expiry
    });
  }

  public async Task Remove(string key)
  {
    await cache.RemoveAsync(key);
  }
}
```

## Block Storage

### Add Registry Constant

```csharp
// App/StartUp/Registry/BlockStorages.cs
namespace App.StartUp.Registry;

public static class BlockStorages
{
  public const string Main = "MAIN";
  public const string UserUploads = "USER_UPLOADS";  // NEW - for user files
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
BlockStorage:
  MAIN:
    Endpoint: minio.example.com:9000
    AccessKey: ${MINIO_ACCESS_KEY}
    SecretKey: ${MINIO_SECRET_KEY}
    Bucket: zinc-main
    Secure: true

  USER_UPLOADS: # NEW
    Endpoint: minio.example.com:9000
    AccessKey: ${MINIO_ACCESS_KEY}
    SecretKey: ${MINIO_SECRET_KEY}
    Bucket: zinc-user-uploads
    Secure: true
    MaxFileSizeMb: 10
```

### Use IFileRepository

```csharp
// Domain/Files/Service.cs
using App.Modules.Common;
using CarboxylicLithium;

public class FileUploadService(
  IFileRepository fileRepo,
  IFileValidator fileValidator,
  ILogger<FileUploadService> logger)
{
  public async Task<Result<FileUploadResult>> Upload(
    IFormFile file,
    string userId)
  {
    logger.LogInformation("Uploading file {FileName} for user {UserId}",
      file.FileName, userId);

    // Validate file
    var validation = await fileValidator.Validate(file);
    if (validation.IsFailure())
    {
      return validation.FailureOrDefault();
    }

    // Generate unique file ID
    var fileId = $"{userId}/{Guid.NewGuid()}/{file.FileName}";

    // Upload to storage
    await using var stream = file.OpenReadStream();
    await fileRepo.Upload(fileId, stream);

    logger.LogInformation("Uploaded file {FileId}", fileId);

    return Result.Ok(new FileUploadResult
    {
      FileId = fileId,
      FileName = file.FileName,
      Size = file.Length,
      ContentType = file.ContentType
    });
  }

  public async Task<Result<Stream>> Download(string fileId)
  {
    logger.LogInformation("Downloading file {FileId}", fileId);
    var stream = await fileRepo.Download(fileId);
    return Result.Ok(stream);
  }

  public async Task<Result<Unit>> Delete(string fileId)
  {
    logger.LogInformation("Deleting file {FileId}", fileId);
    await fileRepo.Delete(fileId);
    return Result.Ok(Unit.Default);
  }
}

public record FileUploadResult
{
  public required string FileId { get; init; }
  public required string FileName { get; init; }
  public required long Size { get; init; }
  public required string ContentType { get; init; }
}
```

### Use IFileValidator

```csharp
// App/Modules/Files/API/V1/FileController.cs
using App.Modules.Common;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/files")]
public class FileController(
  IFileUploadService service,
  IFileValidator validator,
  IAuthHelper authHelper) : AtomiControllerBase(authHelper)
{
  [HttpPost]
  [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
  public async Task<ActionResult<FileUploadRes>> Upload(IFormFile file)
  {
    var principal = await GetPrincipal();
    Guard(principal, "write:files");

    // Validate file
    var validationResult = await validator.Validate(file);
    if (validationResult.IsFailure())
    {
      return this.ReturnResult(Result.Fail<FileUploadRes>(validationResult.FailureOrDefault()));
    }

    // Upload
    var result = await service.Upload(file, principal.UserId);
    return this.ReturnResult(result.Then(r => r.ToRes(), Errors.MapAll));
  }
}
```

## HTTP Client

### Add Registry Constant

```csharp
// App/StartUp/Registry/HttpClients.cs
namespace App.StartUp.Registry;

public static class HttpClients
{
  public const string Main = "Main";
  public const string Logto = "Logto";
  public const string Billing = "Billing";  // NEW
  public const string Notifications = "Notifications";  // NEW
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
HttpClient:
  Logto:
    BaseAddress: https://auth.example.com
    Timeout: 60
    BearerAuth: ${LOGTO_TOKEN}

  Billing: # NEW
    BaseAddress: https://billing-api.example.com
    Timeout: 30
    BearerAuth: ${BILLING_API_KEY}

  Notifications: # NEW
    BaseAddress: https://notifications.example.com
    Timeout: 15
    Headers:
      X-Api-Key: ${NOTIFICATIONS_API_KEY}
```

### Use in Service

```csharp
// Domain/Billing/Service.cs
using System.Net.Http.Json;
using App.StartUp.Registry;

public class BillingService(
  IHttpClientFactory factory,
  ILogger<BillingService> logger)
{
  public async Task<Result<Invoice>> GetInvoice(string invoiceId)
  {
    logger.LogInformation("Fetching invoice {InvoiceId}", invoiceId);

    try
    {
      var client = factory.CreateClient(HttpClients.Billing);
      var response = await client.GetAsync($"/v1/invoices/{invoiceId}");

      if (!response.IsSuccessStatusCode)
      {
        logger.LogWarning("Failed to fetch invoice {InvoiceId}: {StatusCode}",
          invoiceId, response.StatusCode);
        return new ExternalServiceError($"Billing API returned {response.StatusCode}");
      }

      var invoice = await response.Content.ReadFromJsonAsync<Invoice>();
      return Result.Ok(invoice!);
    }
    catch (HttpRequestException ex)
    {
      logger.LogError(ex, "HTTP error fetching invoice {InvoiceId}", invoiceId);
      return new ExternalServiceError("Failed to connect to billing service");
    }
    catch (TaskCanceledException ex)
    {
      logger.LogError(ex, "Timeout fetching invoice {InvoiceId}", invoiceId);
      return new ExternalServiceError("Billing service request timed out");
    }
  }

  public async Task<Result<Unit>> CreateInvoice(InvoiceCreateRequest request)
  {
    logger.LogInformation("Creating invoice for user {UserId}", request.UserId);

    try
    {
      var client = factory.CreateClient(HttpClients.Billing);
      var response = await client.PostAsJsonAsync("/v1/invoices", request);

      response.EnsureSuccessStatusCode();

      logger.LogInformation("Created invoice for user {UserId}", request.UserId);
      return Result.Ok(Unit.Default);
    }
    catch (HttpRequestException ex)
    {
      logger.LogError(ex, "Failed to create invoice for user {UserId}", request.UserId);
      return new ExternalServiceError("Failed to create invoice");
    }
  }
}
```

### Retry Policy

```csharp
// App/StartUp/Services/HttpClientService.cs
using Polly;
using Polly.Extensions.Http;

public static IServiceCollection AddHttpClientWithRetry(
  this IServiceCollection services,
  string name,
  string baseAddress,
  int timeoutSeconds = 30)
{
  services.AddHttpClient(name, client =>
  {
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
  })
  .AddPolicyHandler(GetRetryPolicy())
  .AddPolicyHandler(GetCircuitBreakerPolicy());

  return services;
}

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
  return HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, retryAttempt =>
      TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
  return HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

## SMTP

### Add Registry Constant

```csharp
// App/StartUp/Registry/SmtpProviders.cs
namespace App.StartUp.Registry;

public static class SmtpProviders
{
  public const string Transactional = "TRANSACTIONAL";
  public const string Marketing = "MARKETING";  // NEW
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
Smtp:
  TRANSACTIONAL:
    Host: smtp.sendgrid.net
    Port: 587
    Username: apikey
    Password: ${SENDGRID_TRANSACTIONAL_KEY}
    From: noreply@example.com
    EnableSsl: true

  MARKETING: # NEW
    Host: smtp.sendgrid.net
    Port: 587
    Username: apikey
    Password: ${SENDGRID_MARKETING_KEY}
    From: marketing@example.com
    EnableSsl: true
```

### Use in Service

```csharp
// Domain/Email/Service.cs
using App.StartUp.Registry;
using App.StartUp.Smtp;
using App.StartUp.Email;

public class EmailService(
  ISmtpClientFactory smtpFactory,
  IEmailRenderer renderer,
  ILogger<EmailService> logger)
{
  public async Task<Result<Unit>> SendWelcomeEmail(string toEmail, string userName)
  {
    logger.LogInformation("Sending welcome email to {Email}", toEmail);

    try
    {
      // Render template
      var html = await renderer.Render("welcome", new { Name = userName });

      // Create message
      var message = new SmtpEmailMessage
      {
        To = [toEmail],
        Subject = "Welcome to Zinc!",
        HtmlBody = html,
        TextBody = $"Welcome {userName}!"
      };

      // Send via transactional SMTP
      var client = await smtpFactory.CreateClient(SmtpProviders.Transactional);
      await client.SendAsync(message);

      logger.LogInformation("Sent welcome email to {Email}", toEmail);
      return Result.Ok(Unit.Default);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
      return new EmailSendError("Failed to send welcome email");
    }
  }

  public async Task<Result<Unit>> SendMarketingEmail(
    List<string> recipients,
    string subject,
    string templateName,
    object templateData)
  {
    logger.LogInformation("Sending marketing email to {Count} recipients",
      recipients.Count);

    try
    {
      var html = await renderer.Render(templateName, templateData);

      var message = new SmtpEmailMessage
      {
        To = recipients,
        Subject = subject,
        HtmlBody = html
      };

      // Use marketing SMTP
      var client = await smtpFactory.CreateClient(SmtpProviders.Marketing);
      await client.SendAsync(message);

      logger.LogInformation("Sent marketing email to {Count} recipients",
        recipients.Count);
      return Result.Ok(Unit.Default);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send marketing email");
      return new EmailSendError("Failed to send marketing email");
    }
  }
}
```

## CORS

### Add Registry Constant

```csharp
// App/StartUp/Registry/CorsPolicies.cs
namespace App.StartUp.Registry;

public static class CorsPolicies
{
  public const string AllowAll = "AllowAll";
  public const string FrontendOnly = "FrontendOnly";  // NEW
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
Cors:
  - Name: AllowAll
    AllowedOrigins: ['*']
    AllowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
    AllowedHeaders: ['*']
    AllowCredentials: false

  - Name: FrontendOnly # NEW - production-ready policy
    AllowedOrigins:
      - https://app.example.com
      - https://admin.example.com
    AllowedMethods: ['GET', 'POST', 'PUT', 'DELETE', 'OPTIONS']
    AllowedHeaders:
      - Authorization
      - Content-Type
      - X-Requested-With
    AllowCredentials: true
    MaxAge: 3600
```

### Use in Application

```csharp
// App/StartUp/Server.cs
app.UseCors(CorsPolicies.FrontendOnly);
```

### Use on Controller

```csharp
// App/Modules/Widgets/API/V1/WidgetController.cs
using Microsoft.AspNetCore.Cors;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/widgets")]
[EnableCors(CorsPolicies.FrontendOnly)]  // Override default policy
public class WidgetController : AtomiControllerBase
{
  // ...
}
```

## Auth Policies

### Add Registry Constant

```csharp
// App/StartUp/Registry/AuthPolicies.cs
namespace App.StartUp.Registry;

public static class AuthPolicies
{
  public const string ReadWidgets = "ReadWidgets";
  public const string WriteWidgets = "WriteWidgets";
  public const string DeleteWidgets = "DeleteWidgets";
  public const string AdminOnly = "AdminOnly";  // NEW
  public const string SuperAdminOnly = "SuperAdminOnly";  // NEW
}
```

### Add YAML Configuration

```yaml
# App/Config/settings.yaml
Auth:
  Policies:
    ReadWidgets: ['read:widgets']
    WriteWidgets: ['write:widgets']
    DeleteWidgets: ['delete:widgets']
    AdminOnly: ['admin'] # NEW
    SuperAdminOnly: ['super:admin'] # NEW
```

### Use in Controller

```csharp
// App/Modules/Admin/API/V1/AdminController.cs
using Microsoft.AspNetCore.Authorization;
using App.StartUp.Registry;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = AuthPolicies.AdminOnly)]  // Entire controller requires admin
public class AdminController(
  IAdminService service,
  IAuthHelper authHelper) : AtomiControllerBase(authHelper)
{
  [HttpGet("users")]
  public async Task<ActionResult<List<UserRes>>> GetAllUsers()
  {
    // All admin users can access this
    var principal = await GetPrincipal();
    var result = await service.GetAllUsers();
    return this.ReturnResult(result);
  }

  [HttpDelete("users/{id}")]
  [Authorize(Policy = AuthPolicies.SuperAdminOnly)]  // Override with stricter policy
  public async Task<ActionResult> DeleteUser(string id)
  {
    // Only super admins can delete users
    var principal = await GetPrincipal();
    Guard(principal, AuthPolicies.SuperAdminOnly);

    var result = await service.DeleteUser(id);
    return result.IsSuccess() ? NoContent() : this.ReturnResult(result);
  }
}
```

### Using Guards

```csharp
// Using single policy
Guard(principal, AuthPolicies.WriteWidgets);

// All policies must pass
GuardOrAll(principal, [AuthPolicies.WriteWidgets, AuthPolicies.AdminOnly]);

// Any policy can pass
GuardOrAny(principal, [AuthPolicies.AdminOnly, AuthPolicies.SuperAdminOnly]);
```

## Complete Integration Example

Here's a complete example integrating multiple infrastructure services:

```csharp
// Domain/Widgets/Service.cs
using App.StartUp.Registry;
using CarboxylicLithium;

public class WidgetService(
  IWidgetRepository repository,
  IFileRepository fileRepo,
  IHttpClientFactory httpFactory,
  ISmtpClientFactory smtpFactory,
  IEmailRenderer emailRenderer,
  IConnectionMultiplexer redis,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<WidgetPrincipal>> CreateWithImage(
    WidgetRecord record,
    IFormFile image,
    string userId)
  {
    logger.LogInformation("Creating widget with image for user {UserId}", userId);

    // 1. Upload image to block storage
    var imageId = $"widgets/{Guid.NewGuid()}/{image.FileName}";
    await using var stream = image.OpenReadStream();
    await fileRepo.Upload(imageId, stream);

    // 2. Create widget in database
    var widgetResult = await repository.Create(record with { ImageUrl = imageId });
    if (widgetResult.IsFailure())
    {
      // Cleanup: delete uploaded image
      await fileRepo.Delete(imageId);
      return widgetResult;
    }

    var widget = widgetResult.ValueOrDefault();

    // 3. Cache widget for quick access
    var db = redis.GetDatabase();
    await db.StringSetAsync(
      $"widget:{widget.Id}",
      JsonSerializer.Serialize(widget),
      TimeSpan.FromMinutes(15)
    );

    // 4. Notify external analytics service
    try
    {
      var client = httpFactory.CreateClient(HttpClients.Analytics);
      await client.PostAsJsonAsync("/events/widget-created", new
      {
        WidgetId = widget.Id,
        UserId = userId,
        Timestamp = DateTime.UtcNow
      });
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Failed to notify analytics service");
      // Don't fail the operation
    }

    // 5. Send confirmation email
    try
    {
      var html = await emailRenderer.Render("widget-created", new
      {
        WidgetName = widget.Record.Name
      });

      var message = new SmtpEmailMessage
      {
        To = [record.OwnerEmail],
        Subject = "Widget Created Successfully",
        HtmlBody = html
      };

      var smtpClient = await smtpFactory.CreateClient(SmtpProviders.Transactional);
      await smtpClient.SendAsync(message);
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Failed to send confirmation email");
      // Don't fail the operation
    }

    logger.LogInformation("Created widget {WidgetId} with image", widget.Id);
    return Result.Ok(widget);
  }
}
```
