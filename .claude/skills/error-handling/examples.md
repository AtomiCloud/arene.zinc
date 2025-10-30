# Error Handling Examples

Complete code examples for defining and using domain problems in Zinc.

## Table of Contents

- [Domain Problems](#domain-problems)
- [Built-in Problems](#built-in-problems)
- [Using Problems in Services](#using-problems-in-services)
- [Using Problems in Repositories](#using-problems-in-repositories)
- [Controller Integration](#controller-integration)
- [Result Monad Composition](#result-monad-composition)
- [Testing Error Paths](#testing-error-paths)

## Domain Problems

### Simple Problem

```csharp
// App/Error/V1/WidgetNotActive.cs
using App.Modules.Common;

namespace App.Error.V1;

public record WidgetNotActive(Guid WidgetId) : IDomainProblem
{
  public string Id => "widget_not_active";
  public string Title => "Widget Not Active";
  public string Version => "v1";
  public string Detail => $"Widget {WidgetId} is not active";
}
```

### Problem with Multiple Properties

```csharp
// App/Error/V1/WidgetQuotaExceeded.cs
using App.Modules.Common;

namespace App.Error.V1;

public record WidgetQuotaExceeded(
  string UserId,
  int CurrentCount,
  int MaxAllowed) : IDomainProblem
{
  public string Id => "widget_quota_exceeded";
  public string Title => "Widget Quota Exceeded";
  public string Version => "v1";
  public string Detail => $"User {UserId} has {CurrentCount} widgets but maximum allowed is {MaxAllowed}";
}
```

### Problem with Complex Data

```csharp
// App/Error/V1/WidgetValidationError.cs
using App.Modules.Common;

namespace App.Error.V1;

public record WidgetValidationError : IDomainProblem
{
  public string Id => "widget_validation_error";
  public string Title => "Widget Validation Error";
  public string Version => "v1";
  public string Detail { get; init; } = "Widget validation failed";

  // These properties appear in "data" field of RFC 7807 response
  public Dictionary<string, List<string>> Errors { get; init; } = new();
  public List<string> Warnings { get; init; } = new();
}

// Usage
var error = new WidgetValidationError
{
  Detail = "Multiple validation errors occurred",
  Errors = new()
  {
    ["name"] = ["Name is required", "Name must be at least 3 characters"],
    ["description"] = ["Description is required"]
  },
  Warnings = ["Widget will be inactive by default"]
};
```

### Problem with Enum

```csharp
// App/Error/V1/InvalidWidgetStatus.cs
using App.Modules.Common;

namespace App.Error.V1;

public enum WidgetStatus
{
  Draft,
  Active,
  Archived
}

public record InvalidWidgetStatus(
  WidgetStatus CurrentStatus,
  WidgetStatus RequiredStatus) : IDomainProblem
{
  public string Id => "invalid_widget_status";
  public string Title => "Invalid Widget Status";
  public string Version => "v1";
  public string Detail => $"Widget status is {CurrentStatus} but must be {RequiredStatus}";
}
```

## Built-in Problems

### ValidationError

```csharp
// Usage in service
using App.Error.V1;

public async Task<Result<Widget>> Create(WidgetRecord record)
{
  if (string.IsNullOrEmpty(record.Name))
  {
    return new ValidationError("Name is required");
  }

  if (record.Name.Length > 255)
  {
    return new ValidationError("Name must not exceed 255 characters");
  }

  // ... continue
}
```

### EntityNotFound

```csharp
// Usage in controller
using App.Error.V1;

[HttpGet("{id}")]
public async Task<ActionResult<WidgetRes>> GetById(Guid id)
{
  var result = await service.GetById(id)
    .Then(w => w?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(
    result,
    new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
  );
}
```

### EntityConflict

```csharp
// Usage in repository
using App.Error.V1;
using App.Modules.Common;

public async Task<Result<WidgetPrincipal>> Create(WidgetRecord record)
{
  try
  {
    var widget = record.ToData();
    widget.Id = Guid.NewGuid();
    db.Widgets.Add(widget);
    await db.SaveChangesAsync();
    return Result.Ok(widget.ToPrincipal());
  }
  catch (DbUpdateException ex)
  {
    if (ex.InnerException?.Message.Contains("unique constraint") == true)
    {
      throw new DomainProblemException(
        new EntityConflict($"Widget with name '{record.Name}' already exists")
      );
    }
    throw;
  }
}
```

### Unauthorized

```csharp
// Usage in controller
using App.Error.V1;

[HttpDelete("{id}")]
public async Task<ActionResult> Delete(Guid id)
{
  var principal = await GetPrincipal();

  // Using guard (throws Unauthorized automatically)
  Guard(principal, "delete:widgets");

  // Manual check
  if (!principal.Policies.Contains("delete:widgets"))
  {
    throw new DomainProblemException(
      new Unauthorized("delete:widgets", "Insufficient permissions to delete widgets")
    );
  }

  var result = await service.Delete(id);
  return NoContent();
}
```

### Unauthenticated

```csharp
// Usage in auth helper
using App.Error.V1;
using App.Modules.Common;

public async Task<TokenPrincipal> ValidateToken(string token)
{
  if (string.IsNullOrEmpty(token))
  {
    throw new DomainProblemException(
      new Unauthenticated("No authentication token provided")
    );
  }

  var result = await authService.ValidateToken(token);
  if (result.IsFailure())
  {
    throw new DomainProblemException(
      new Unauthenticated("Invalid or expired token")
    );
  }

  return result.ValueOrDefault();
}
```

### FileTooLarge

```csharp
// Usage in file validator
using App.Error.V1;
using App.Modules.Common;

public Result<Unit> ValidateFileSize(IFormFile file, long maxSizeBytes)
{
  if (file.Length > maxSizeBytes)
  {
    throw new DomainProblemException(
      new FileTooLarge(file.FileName, file.Length, maxSizeBytes)
    );
  }

  return Result.Ok(Unit.Default);
}
```

### MultipleEntityNotFound

```csharp
// Usage in batch operations
using App.Error.V1;

public async Task<Result<List<Widget>>> GetByIds(List<Guid> ids)
{
  var widgets = await repository.GetByIds(ids);

  var foundIds = widgets.Select(w => w.Id).ToHashSet();
  var missingIds = ids.Where(id => !foundIds.Contains(id)).ToList();

  if (missingIds.Any())
  {
    throw new DomainProblemException(
      new MultipleEntityNotFound(
        typeof(Widget),
        missingIds.Select(id => id.ToString()).ToList()
      )
    );
  }

  return Result.Ok(widgets);
}
```

## Using Problems in Services

### Return Result with Problem

```csharp
// Domain/Widgets/Service.cs
using App.Error.V1;
using CarboxylicLithium;

public class WidgetService(
  IWidgetRepository repository,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<WidgetPrincipal>> Activate(Guid id)
  {
    logger.LogInformation("Activating widget {Id}", id);

    var widget = await repository.GetById(id);

    // Return problem for null
    if (widget is null)
    {
      return new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString());
    }

    // Return problem for business rule violation
    if (widget.Record.Active)
    {
      return new ValidationError("Widget is already active");
    }

    return await repository.Update(id, widget.Record with { Active = true });
  }
}
```

### Throw DomainProblemException

```csharp
// Domain/Widgets/Service.cs
using App.Error.V1;
using App.Modules.Common;
using CarboxylicLithium;

public class WidgetService(
  IWidgetRepository repository,
  IWidgetQuotaChecker quotaChecker,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<WidgetPrincipal>> Create(string userId, WidgetRecord record)
  {
    logger.LogInformation("Creating widget for user {UserId}", userId);

    // Check quota (throw if exceeded)
    var quota = await quotaChecker.GetQuota(userId);
    if (quota.CurrentCount >= quota.MaxAllowed)
    {
      throw new DomainProblemException(
        new WidgetQuotaExceeded(userId, quota.CurrentCount, quota.MaxAllowed)
      );
    }

    return await repository.Create(record);
  }
}
```

### Mixed Approach

```csharp
// Domain/Widgets/Service.cs
using App.Error.V1;
using App.Modules.Common;
using CarboxylicLithium;

public class WidgetService(
  IWidgetRepository repository,
  ITransactionManager transactionManager,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<WidgetPrincipal>> Update(Guid id, WidgetRecord record)
  {
    logger.LogInformation("Updating widget {Id}", id);

    return await transactionManager.Start(async () =>
    {
      var existing = await repository.GetById(id);

      // Return Result for expected null case
      if (existing is null)
      {
        return Result.Fail<WidgetPrincipal>(
          new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
        );
      }

      // Throw for unexpected state (should never happen)
      if (existing.Id != id)
      {
        throw new DomainProblemException(
          new ValidationError("Widget ID mismatch")
        );
      }

      return await repository.Update(id, record);
    });
  }
}
```

## Using Problems in Repositories

### Converting EF Exceptions

```csharp
// App/Modules/Widgets/Data/WidgetRepository.cs
using App.Error.V1;
using App.Modules.Common;
using CarboxylicLithium;
using Microsoft.EntityFrameworkCore;

public class WidgetRepository(
  MainDbContext db,
  ILogger<WidgetRepository> logger) : IWidgetRepository
{
  public async Task<Result<WidgetPrincipal>> Create(WidgetRecord record)
  {
    try
    {
      var widget = record.ToData();
      widget.Id = Guid.NewGuid();
      widget.CreatedAt = DateTime.UtcNow;
      widget.UpdatedAt = DateTime.UtcNow;

      db.Widgets.Add(widget);
      await db.SaveChangesAsync();

      logger.LogInformation("Created widget {Id}", widget.Id);
      return Result.Ok(widget.ToPrincipal());
    }
    catch (DbUpdateException ex) when (
      ex.InnerException?.Message.Contains("duplicate key") == true ||
      ex.InnerException?.Message.Contains("unique constraint") == true)
    {
      logger.LogWarning(ex, "Unique constraint violation for widget name {Name}", record.Name);
      throw new DomainProblemException(
        new EntityConflict($"Widget with name '{record.Name}' already exists")
      );
    }
    catch (DbUpdateConcurrencyException ex)
    {
      logger.LogWarning(ex, "Concurrency conflict for widget");
      throw new DomainProblemException(
        new LikeRaceCondition("Widget was modified by another process")
      );
    }
  }
}
```

### Handling Foreign Key Violations

```csharp
// App/Modules/Widgets/Data/WidgetRepository.cs
using App.Error.V1;
using App.Modules.Common;
using CarboxylicLithium;
using Microsoft.EntityFrameworkCore;

public async Task<Result<Unit>> Delete(Guid id)
{
  try
  {
    var widget = await db.Widgets.FindAsync(id);
    if (widget is null)
    {
      return Result.Ok(Unit.Default);
    }

    db.Widgets.Remove(widget);
    await db.SaveChangesAsync();

    logger.LogInformation("Deleted widget {Id}", id);
    return Result.Ok(Unit.Default);
  }
  catch (DbUpdateException ex) when (
    ex.InnerException?.Message.Contains("foreign key") == true)
  {
    logger.LogWarning(ex, "Cannot delete widget {Id} due to references", id);
    throw new DomainProblemException(
      new EntityConflict($"Cannot delete widget because it is referenced by other entities")
    );
  }
}
```

## Controller Integration

### Using ReturnResult

```csharp
// App/Modules/Widgets/API/V1/WidgetController.cs
using Asp.Versioning;
using CarboxylicLithium;
using Domain.Widgets;
using Microsoft.AspNetCore.Mvc;
using App.Modules.Common;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/widgets")]
public class WidgetController(
  IWidgetService service,
  IAuthHelper authHelper) : AtomiControllerBase(authHelper)
{
  [HttpPost]
  public async Task<ActionResult<WidgetRes>> Create([FromBody] WidgetCreateReq req)
  {
    var principal = await GetPrincipal();
    Guard(principal, "write:widgets");

    // Service returns Result<WidgetPrincipal>
    // Errors are automatically mapped to HTTP status codes
    var result = await service.Create(req.ToRecord())
      .ThenAwait(async w =>
      {
        var aggregate = await service.GetById(w.Id);
        return aggregate.Then(a => a?.ToRes(), Errors.MapAll);
      });

    return this.ReturnResult(result);
  }
}
```

### Using ReturnNullableResult

```csharp
// App/Modules/Widgets/API/V1/WidgetController.cs
[HttpGet("{id}")]
public async Task<ActionResult<WidgetRes>> GetById(Guid id)
{
  var principal = await GetPrincipal();
  Guard(principal, "read:widgets");

  // Service returns Result<WidgetAggregate?>
  // If null, use the provided fallback error
  var result = await service.GetById(id)
    .Then(w => w?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(
    result,
    new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
  );
}
```

### Manual Error Handling

```csharp
// App/Modules/Widgets/API/V1/WidgetController.cs
[HttpPut("{id}/activate")]
public async Task<ActionResult<WidgetRes>> Activate(Guid id)
{
  var principal = await GetPrincipal();
  Guard(principal, "write:widgets");

  var result = await service.Activate(id);

  if (result.IsFailure())
  {
    var error = result.FailureOrDefault();

    // Custom handling based on error type
    return error switch
    {
      EntityNotFound => NotFound(error),
      ValidationError => BadRequest(error),
      _ => this.ReturnResult(Result.Fail<WidgetRes>(error))
    };
  }

  // Get full aggregate and return
  var aggregate = await service.GetById(id);
  return this.ReturnResult(aggregate.Then(a => a?.ToRes(), Errors.MapAll));
}
```

## Result Monad Composition

### Chaining with Error Propagation

```csharp
// Service method using Result chaining
public async Task<Result<WidgetRes>> ProcessWidget(Guid id)
{
  return await repository.GetById(id)                    // Result<WidgetAggregate?>
    .NullToError(id.ToString())                          // Result<WidgetAggregate>
    .Then(w => w.Validate(), Errors.MapAll)              // Result<WidgetAggregate>
    .ThenAwait(w => w.Enrich(), Errors.MapAll)           // Result<WidgetAggregate>
    .ThenAwait(w => repository.Update(w.Id, w.Record), Errors.MapAll)  // Result<WidgetPrincipal>
    .Then(p => p.ToRes(), Errors.MapAll);                // Result<WidgetRes>

  // If any step returns an error, the chain stops and the error propagates
}
```

### Conditional Error Mapping

```csharp
// Service method with selective error mapping
public async Task<Result<Widget>> GetActiveWidget(Guid id)
{
  return await repository.GetById(id)
    .Then(
      widget => widget?.Record.Active == true
        ? Result.Ok(widget)
        : Result.Fail<WidgetAggregate>(new WidgetNotActive(id)),
      error => error switch
      {
        NotFoundException => new EntityNotFound("Widget not found", typeof(Widget), id.ToString()),
        _ => error
      }
    );
}
```

### DoAwait for Side Effects

```csharp
// Service method with side effects and error handling
public async Task<Result<WidgetPrincipal>> CreateAndNotify(WidgetRecord record)
{
  return await repository.Create(record)
    .DoAwait(async widget =>
    {
      // Side effect: send notification
      await notificationService.NotifyWidgetCreated(widget);
    }, Errors.MapAll)
    .DoAwait(async widget =>
    {
      // Side effect: log audit
      await auditService.LogCreated(widget.Id);
    }, Errors.MapAll);

  // Returns the widget principal, but side effects run first
  // If any side effect fails, the error propagates
}
```

## Testing Error Paths

### Testing Problem Creation

```csharp
// UnitTest/Domain/Widgets/ErrorTests.cs
using App.Error.V1;
using FluentAssertions;

public class WidgetErrorTests
{
  [Fact]
  public void WidgetNotActive_ShouldSetProperties()
  {
    // Arrange
    var widgetId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

    // Act
    var error = new WidgetNotActive(widgetId);

    // Assert
    error.Id.Should().Be("widget_not_active");
    error.Title.Should().Be("Widget Not Active");
    error.Version.Should().Be("v1");
    error.Detail.Should().Contain(widgetId.ToString());
  }
}
```

### Testing Service Error Returns

```csharp
// UnitTest/Domain/Widgets/ServiceTests.cs
using App.Error.V1;
using CarboxylicLithium;
using Domain.Widgets;
using FluentAssertions;
using Moq;

public class WidgetServiceTests
{
  [Fact]
  public async Task Create_WithEmptyName_ShouldReturnValidationError()
  {
    // Arrange
    var mockRepo = new Mock<IWidgetRepository>();
    var service = new WidgetService(mockRepo.Object, Mock.Of<ILogger<WidgetService>>());
    var record = new WidgetRecord { Name = "", Description = "Test", Active = true };

    // Act
    var result = await service.Create(record);

    // Assert
    result.IsFailure().Should().BeTrue();
    result.FailureOrDefault().Should().BeOfType<ValidationError>();
    var error = result.FailureOrDefault() as ValidationError;
    error!.Detail.Should().Contain("Name is required");
  }

  [Fact]
  public async Task GetById_WithNonExistingId_ShouldReturnEntityNotFound()
  {
    // Arrange
    var mockRepo = new Mock<IWidgetRepository>();
    mockRepo.Setup(r => r.GetById(It.IsAny<Guid>()))
      .ReturnsAsync(Result.Ok<WidgetAggregate?>(null));

    var service = new WidgetService(mockRepo.Object, Mock.Of<ILogger<WidgetService>>());
    var id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");

    // Act
    var result = await service.GetById(id);

    // Assert
    result.IsSuccess().Should().BeTrue();
    result.ValueOrDefault().Should().BeNull();
  }
}
```

### Testing Repository Exception Handling

```csharp
// UnitTest/Data/Widgets/RepositoryTests.cs
using App.Error.V1;
using App.Modules.Common;
using App.Modules.Widgets.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class WidgetRepositoryTests
{
  [Fact]
  public async Task Create_WithDuplicateName_ShouldThrowEntityConflict()
  {
    // Arrange
    var options = new DbContextOptionsBuilder<MainDbContext>()
      .UseInMemoryDatabase("TestDb")
      .Options;

    await using var db = new MainDbContext(options);
    var repository = new WidgetRepository(db, Mock.Of<ILogger<WidgetRepository>>());

    var record = new WidgetRecord { Name = "Test", Description = "Test", Active = true };
    await repository.Create(record);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<DomainProblemException>(
      async () => await repository.Create(record)
    );

    exception.Problem.Should().BeOfType<EntityConflict>();
  }
}
```

### Testing Controller Error Responses

```csharp
// IntTest/Widgets/WidgetControllerTests.cs
using System.Net;
using FluentAssertions;

public class WidgetControllerTests
{
  [Fact]
  public async Task GetById_WithNonExistingId_ShouldReturn404()
  {
    // Arrange
    var client = _factory.CreateClient();
    var id = Guid.NewGuid();

    // Act
    var response = await client.GetAsync($"/api/v1/widgets/{id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);

    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails!.Type.Should().Contain("entity_not_found");
    problemDetails.Title.Should().Be("Entity Not Found");
    problemDetails.Status.Should().Be(404);
  }

  [Fact]
  public async Task Create_WithInvalidData_ShouldReturn400()
  {
    // Arrange
    var client = _factory.CreateClient();
    var request = new WidgetCreateReq { Name = "", Description = "Test", Active = true };

    // Act
    var response = await client.PostAsJsonAsync("/api/v1/widgets", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails!.Type.Should().Contain("validation_error");
    problemDetails.Title.Should().Be("Validation Error");
    problemDetails.Status.Should().Be(400);
  }
}
```
