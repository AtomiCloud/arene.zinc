# Feature Module Examples

This document provides complete code examples for each layer of a feature module.

## Table of Contents

- [Domain Layer](#domain-layer)
- [Data Layer](#data-layer)
- [API Layer](#api-layer)
- [Dependency Injection](#dependency-injection)
- [Configuration](#configuration)
- [Error Handling](#error-handling)
- [Complete Example](#complete-example)

## Domain Layer

### Domain Models

```csharp
// Domain/Widgets/Widget.cs
using CarboxylicLithium;

namespace Domain.Widgets;

/// <summary>
/// Aggregate - Complete entity with all properties
/// </summary>
public record WidgetAggregate
{
  public required Guid Id { get; init; }
  public required WidgetRecord Record { get; init; }
  public required DateTime CreatedAt { get; init; }
  public required DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Principal - Summary representation for lists
/// </summary>
public record WidgetPrincipal
{
  public required Guid Id { get; init; }
  public required WidgetRecord Record { get; init; }
}

/// <summary>
/// Record - Data Transfer Object
/// </summary>
public record WidgetRecord
{
  public required string Name { get; init; }
  public required string Description { get; init; }
  public required bool Active { get; init; }
}

/// <summary>
/// Search - Query parameters
/// </summary>
public record WidgetSearch
{
  public string? Name { get; init; }
  public bool? Active { get; init; }
}
```

### Service Interface

```csharp
// Domain/Widgets/IService.cs
using CarboxylicLithium;

namespace Domain.Widgets;

public interface IWidgetService
{
  Task<Result<List<WidgetPrincipal>>> Search(WidgetSearch search);
  Task<Result<WidgetAggregate?>> GetById(Guid id);
  Task<Result<WidgetPrincipal>> Create(WidgetRecord record);
  Task<Result<WidgetPrincipal>> Update(Guid id, WidgetRecord record);
  Task<Result<Unit>> Delete(Guid id);
}
```

### Repository Interface

```csharp
// Domain/Widgets/Repository.cs
using CarboxylicLithium;

namespace Domain.Widgets;

public interface IWidgetRepository
{
  Task<Result<List<WidgetPrincipal>>> Search(WidgetSearch search);
  Task<Result<WidgetAggregate?>> GetById(Guid id);
  Task<Result<WidgetPrincipal>> Create(WidgetRecord record);
  Task<Result<WidgetPrincipal>> Update(Guid id, WidgetRecord record);
  Task<Result<WidgetAggregate?>> Delete(Guid id);
}
```

### Service Implementation

```csharp
// Domain/Widgets/Service.cs
using CarboxylicLithium;
using Microsoft.Extensions.Logging;

namespace Domain.Widgets;

public class WidgetService(
  IWidgetRepository repository,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<List<WidgetPrincipal>>> Search(WidgetSearch search)
  {
    logger.LogInformation("Searching widgets with filter: {@Search}", search);
    return await repository.Search(search);
  }

  public async Task<Result<WidgetAggregate?>> GetById(Guid id)
  {
    logger.LogInformation("Getting widget by id: {Id}", id);
    return await repository.GetById(id);
  }

  public async Task<Result<WidgetPrincipal>> Create(WidgetRecord record)
  {
    logger.LogInformation("Creating widget: {@Record}", record);
    return await repository.Create(record);
  }

  public async Task<Result<WidgetPrincipal>> Update(Guid id, WidgetRecord record)
  {
    logger.LogInformation("Updating widget {Id}: {@Record}", id, record);
    return await repository.Update(id, record);
  }

  public async Task<Result<Unit>> Delete(Guid id)
  {
    logger.LogInformation("Deleting widget: {Id}", id);
    return await repository.Delete(id)
      .Then(_ => Result.Ok(Unit.Default));
  }
}
```

## Data Layer

### EF Core Entity

```csharp
// App/Modules/Widgets/Data/WidgetData.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Modules.Widgets.Data;

[Table("widgets")]
public class WidgetData
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; }

  [Required]
  [Column("name")]
  [MaxLength(255)]
  public string Name { get; set; } = string.Empty;

  [Required]
  [Column("description")]
  public string Description { get; set; } = string.Empty;

  [Required]
  [Column("active")]
  public bool Active { get; set; }

  [Required]
  [Column("created_at")]
  public DateTime CreatedAt { get; set; }

  [Required]
  [Column("updated_at")]
  public DateTime UpdatedAt { get; set; }
}
```

### Data Mapper

```csharp
// App/Modules/Widgets/Data/WidgetMapper.cs
using Domain.Widgets;

namespace App.Modules.Widgets.Data;

public static class WidgetMapper
{
  public static WidgetAggregate ToAggregate(this WidgetData data) =>
    new()
    {
      Id = data.Id,
      Record = data.ToRecord(),
      CreatedAt = data.CreatedAt,
      UpdatedAt = data.UpdatedAt
    };

  public static WidgetPrincipal ToPrincipal(this WidgetData data) =>
    new()
    {
      Id = data.Id,
      Record = data.ToRecord()
    };

  public static WidgetRecord ToRecord(this WidgetData data) =>
    new()
    {
      Name = data.Name,
      Description = data.Description,
      Active = data.Active
    };

  public static WidgetData ToData(this WidgetRecord record) =>
    new()
    {
      Name = record.Name,
      Description = record.Description,
      Active = record.Active
    };
}
```

### Repository Implementation

```csharp
// App/Modules/Widgets/Data/WidgetRepository.cs
using CarboxylicLithium;
using Domain.Widgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using App.StartUp.Database;

namespace App.Modules.Widgets.Data;

public class WidgetRepository(
  MainDbContext db,
  ILogger<WidgetRepository> logger) : IWidgetRepository
{
  public async Task<Result<List<WidgetPrincipal>>> Search(WidgetSearch search)
  {
    var query = db.Widgets.AsQueryable();

    if (!string.IsNullOrEmpty(search.Name))
    {
      query = query.Where(w => w.Name.Contains(search.Name));
    }

    if (search.Active.HasValue)
    {
      query = query.Where(w => w.Active == search.Active.Value);
    }

    var results = await query
      .OrderBy(w => w.Name)
      .ToListAsync();

    return Result.Ok(results.Select(w => w.ToPrincipal()).ToList());
  }

  public async Task<Result<WidgetAggregate?>> GetById(Guid id)
  {
    var widget = await db.Widgets.FindAsync(id);
    return Result.Ok(widget?.ToAggregate());
  }

  public async Task<Result<WidgetPrincipal>> Create(WidgetRecord record)
  {
    var now = DateTime.UtcNow;
    var widget = record.ToData();
    widget.Id = Guid.NewGuid();
    widget.CreatedAt = now;
    widget.UpdatedAt = now;

    db.Widgets.Add(widget);
    await db.SaveChangesAsync();

    logger.LogInformation("Created widget {Id}", widget.Id);
    return Result.Ok(widget.ToPrincipal());
  }

  public async Task<Result<WidgetPrincipal>> Update(Guid id, WidgetRecord record)
  {
    var widget = await db.Widgets.FindAsync(id);
    if (widget is null)
    {
      return Result.Ok<WidgetPrincipal>(null!);
    }

    widget.Name = record.Name;
    widget.Description = record.Description;
    widget.Active = record.Active;
    widget.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    logger.LogInformation("Updated widget {Id}", id);
    return Result.Ok(widget.ToPrincipal());
  }

  public async Task<Result<WidgetAggregate?>> Delete(Guid id)
  {
    var widget = await db.Widgets.FindAsync(id);
    if (widget is null)
    {
      return Result.Ok<WidgetAggregate?>(null);
    }

    db.Widgets.Remove(widget);
    await db.SaveChangesAsync();

    logger.LogInformation("Deleted widget {Id}", id);
    return Result.Ok(widget.ToAggregate());
  }
}
```

### DbContext Configuration

```csharp
// App/StartUp/Database/MainDbContext.cs
using App.Modules.Widgets.Data;
using Microsoft.EntityFrameworkCore;

namespace App.StartUp.Database;

public class MainDbContext : DbContext
{
  // Add DbSet
  public DbSet<WidgetData> Widgets { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure entity
    modelBuilder.Entity<WidgetData>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.HasIndex(e => e.Name);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
      entity.Property(e => e.Description).IsRequired();
    });
  }
}
```

## API Layer

### Request/Response DTOs

```csharp
// App/Modules/Widgets/API/V1/WidgetModel.cs
namespace App.Modules.Widgets.API.V1;

public record WidgetSearchReq
{
  public string? Name { get; init; }
  public bool? Active { get; init; }
}

public record WidgetCreateReq
{
  public required string Name { get; init; }
  public required string Description { get; init; }
  public required bool Active { get; init; }
}

public record WidgetUpdateReq
{
  public required string Name { get; init; }
  public required string Description { get; init; }
  public required bool Active { get; init; }
}

public record WidgetRes
{
  public required Guid Id { get; init; }
  public required string Name { get; init; }
  public required string Description { get; init; }
  public required bool Active { get; init; }
  public required DateTime CreatedAt { get; init; }
  public required DateTime UpdatedAt { get; init; }
}

public record WidgetListRes
{
  public required Guid Id { get; init; }
  public required string Name { get; init; }
  public required bool Active { get; init; }
}
```

### FluentValidation Validators

```csharp
// App/Modules/Widgets/API/V1/WidgetValidator.cs
using FluentValidation;

namespace App.Modules.Widgets.API.V1;

public class WidgetCreateReqValidator : AbstractValidator<WidgetCreateReq>
{
  public WidgetCreateReqValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Name is required")
      .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

    RuleFor(x => x.Description)
      .NotEmpty().WithMessage("Description is required")
      .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
  }
}

public class WidgetUpdateReqValidator : AbstractValidator<WidgetUpdateReq>
{
  public WidgetUpdateReqValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Name is required")
      .MaximumLength(255).WithMessage("Name must not exceed 255 characters");

    RuleFor(x => x.Description)
      .NotEmpty().WithMessage("Description is required")
      .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
  }
}
```

### API Mapper

```csharp
// App/Modules/Widgets/API/V1/WidgetMapper.cs
using Domain.Widgets;

namespace App.Modules.Widgets.API.V1;

public static class WidgetMapper
{
  public static WidgetSearch ToSearch(this WidgetSearchReq req) =>
    new()
    {
      Name = req.Name,
      Active = req.Active
    };

  public static WidgetRecord ToRecord(this WidgetCreateReq req) =>
    new()
    {
      Name = req.Name,
      Description = req.Description,
      Active = req.Active
    };

  public static WidgetRecord ToRecord(this WidgetUpdateReq req) =>
    new()
    {
      Name = req.Name,
      Description = req.Description,
      Active = req.Active
    };

  public static WidgetRes ToRes(this WidgetAggregate agg) =>
    new()
    {
      Id = agg.Id,
      Name = agg.Record.Name,
      Description = agg.Record.Description,
      Active = agg.Record.Active,
      CreatedAt = agg.CreatedAt,
      UpdatedAt = agg.UpdatedAt
    };

  public static WidgetListRes ToListRes(this WidgetPrincipal principal) =>
    new()
    {
      Id = principal.Id,
      Name = principal.Record.Name,
      Active = principal.Record.Active
    };
}
```

### Controller

```csharp
// App/Modules/Widgets/API/V1/WidgetController.cs
using Asp.Versioning;
using CarboxylicLithium;
using Domain.Widgets;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Error.V1;
using App.Modules.Common;
using App.StartUp.Services.Auth;

namespace App.Modules.Widgets.API.V1;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/widgets")]
[Authorize]
public class WidgetController(
  IWidgetService service,
  IAuthHelper authHelper,
  IValidator<WidgetCreateReq> createValidator,
  IValidator<WidgetUpdateReq> updateValidator) : AtomiControllerBase(authHelper)
{
  [HttpGet]
  public async Task<ActionResult<List<WidgetListRes>>> Search([FromQuery] WidgetSearchReq req)
  {
    var principal = await GetPrincipal();
    Guard(principal, "read:widgets");

    var result = await service.Search(req.ToSearch())
      .Then(widgets => widgets.Select(w => w.ToListRes()).ToList(), Errors.MapAll);

    return this.ReturnResult(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<WidgetRes>> GetById(Guid id)
  {
    var principal = await GetPrincipal();
    Guard(principal, "read:widgets");

    var result = await service.GetById(id)
      .Then(w => w?.ToRes(), Errors.MapAll);

    return this.ReturnNullableResult(
      result,
      new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
    );
  }

  [HttpPost]
  public async Task<ActionResult<WidgetRes>> Create([FromBody] WidgetCreateReq req)
  {
    var principal = await GetPrincipal();
    Guard(principal, "write:widgets");

    var validationResult = await createValidator.ValidateAsyncResult(req, "Invalid widget data");
    if (validationResult.IsFailure())
    {
      return this.ReturnResult(Result.Fail<WidgetRes>(validationResult.FailureOrDefault()));
    }

    var result = await service.Create(req.ToRecord())
      .ThenAwait(async w =>
      {
        var aggregate = await service.GetById(w.Id);
        return aggregate.Then(a => a?.ToRes(), Errors.MapAll);
      });

    return this.ReturnResult(result);
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<WidgetRes>> Update(Guid id, [FromBody] WidgetUpdateReq req)
  {
    var principal = await GetPrincipal();
    Guard(principal, "write:widgets");

    var validationResult = await updateValidator.ValidateAsyncResult(req, "Invalid widget data");
    if (validationResult.IsFailure())
    {
      return this.ReturnResult(Result.Fail<WidgetRes>(validationResult.FailureOrDefault()));
    }

    var result = await service.Update(id, req.ToRecord())
      .ThenAwait(async w =>
      {
        var aggregate = await service.GetById(w.Id);
        return aggregate.Then(a => a?.ToRes(), Errors.MapAll);
      });

    return this.ReturnNullableResult(
      result,
      new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
    );
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var principal = await GetPrincipal();
    Guard(principal, "delete:widgets");

    var result = await service.Delete(id);

    return result.IsSuccess()
      ? NoContent()
      : this.ReturnResult(Result.Fail<Unit>(result.FailureOrDefault()));
  }
}
```

## Dependency Injection

```csharp
// App/Modules/DomainServices.cs
using Domain.Widgets;
using App.Modules.Widgets.Data;
using App.StartUp.Telemetry;

namespace App.Modules;

public static class DomainServices
{
  public static IServiceCollection AddDomainServices(this IServiceCollection s)
  {
    // ... other services ...

    // Widget services
    s.AddScoped<IWidgetService, WidgetService>()
      .AutoTrace<IWidgetService>();
    s.AddScoped<IWidgetRepository, WidgetRepository>()
      .AutoTrace<IWidgetRepository>();

    return s;
  }
}
```

## Configuration

### Registry Constants

```csharp
// App/StartUp/Registry/HttpClients.cs (example)
namespace App.StartUp.Registry;

public static class HttpClients
{
  public const string WidgetApi = "WidgetApi";
}
```

### YAML Configuration

```yaml
# App/Config/settings.yaml
HttpClient:
  WidgetApi:
    BaseUrl: 'https://widget-api.example.com'
    Timeout: 30
```

### Using Infrastructure in Service

```csharp
// Domain/Widgets/Service.cs
using App.StartUp.Registry;

public class WidgetService(
  IWidgetRepository repository,
  IHttpClientFactory httpClientFactory,
  ILogger<WidgetService> logger) : IWidgetService
{
  public async Task<Result<WidgetData>> FetchFromExternalApi(string id)
  {
    // Use Registry constant, never raw string
    var client = httpClientFactory.CreateClient(HttpClients.WidgetApi);
    var response = await client.GetAsync($"/widgets/{id}");
    // ... process response
  }
}
```

## Error Handling

### Define Domain Problem

```csharp
// App/Error/V1/WidgetNotActive.cs
using App.Modules.Common;

namespace App.Error.V1;

public record WidgetNotActive(Guid WidgetId) : IDomainProblem
{
  public string Id => "widget_not_active";
  public string Title => "Widget Not Active";
  public string Version => "v1";
  public string Detail => $"Widget {WidgetId} is not active and cannot be used";
}
```

### Use in Service

```csharp
// Domain/Widgets/Service.cs
using App.Error.V1;
using App.Modules.Common;

public async Task<Result<WidgetPrincipal>> Activate(Guid id)
{
  var widget = await repository.GetById(id);

  if (widget is null)
  {
    throw new DomainProblemException(
      new EntityNotFound("Widget not found", typeof(WidgetAggregate), id.ToString())
    );
  }

  if (!widget.Record.Active)
  {
    throw new DomainProblemException(new WidgetNotActive(id));
  }

  return await repository.Update(id, widget.Record with { Active = true });
}
```

## Complete Example

For a complete working example, see these reference implementations:

- **Simple CRUD**: `App/Modules/Projects/` - Basic CRUD with GUIDs
- **String IDs**: `App/Modules/Users/` - Using string identifiers
- **Complex Nested**: `App/Modules/Subscribers/` - Complex nested objects
- **Composite Keys**: `App/Modules/SubscriptionTypes/` - Composite key handling
- **With Transactions**: Domain service examples using `ITransactionManager`
