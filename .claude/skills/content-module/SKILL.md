---
name: content-module
description: Implement standard REST API endpoints (GET search, GET by ID, POST create, PUT update, DELETE) with consistent patterns
---

# Content Module Skill

Use this skill when implementing REST API controllers for content modules. Follow the standard 5-endpoint pattern for consistency.

## Overview

Every content module implements five standard REST endpoints:

1. **GET /api/v1/<resource>** - Search/list with filtering and pagination
2. **GET /api/v1/<resource>/:id** - Get single aggregate root by ID
3. **POST /api/v1/<resource>** - Create new resource
4. **PUT /api/v1/<resource>/:id** - Update existing resource
5. **DELETE /api/v1/<resource>/:id** - Delete resource

## Key Concepts

**Value Types**:

- **Principal** - Summary DTO (for lists/search results)
- **Aggregate Root** - Full entity with related data (for Get endpoint)
- **Record** - Mutable fields (can be updated via PUT)
- **Property** - Initial-only fields (set at creation, cannot be updated)

**Authorization Patterns**:

- **User-scoped** - User can only access their own data (OR admin can access any)
- **Admin-only** - Only admins can access
- **Self-only** - User can only access their own (no admin override)

**BaseController Helpers**:

- `ReturnResult(result)` - For operations returning data
- `ReturnNullableResult(result, notFoundError)` - For nullable data (returns 404 if null)
- `ReturnUnitResult(result)` - For non-nullable Unit
- `ReturnUnitNullableResult(result, notFoundError)` - For nullable Unit (delete)

## 1. GET /api/v1/<resource> - Search Endpoint

**Purpose**: Search/filter with pagination, field filtering, authorization scoping.

**Pattern**:

- Query parameters with nullable fields
- Pagination via `Limit` (default 20) and `Skip` (default 0)
- Validate limits (e.g., max 100)
- Apply authorization to reduce scope
- Return list of **Principals** (not full aggregates)

**Example - User-Scoped**:

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
{
  var x = await this
    .GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)  // Authorization scoping
    .ThenAwait(_ => searchValidator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);

  return this.ReturnResult(x);
}
```

**Request Model**:

```csharp
public record SearchPassengerQuery(
  string? UserId,      // Scope filter (required for non-admins)
  string? Name,        // Optional field filter
  int? Limit,         // Pagination (default 20)
  int? Skip           // Pagination (default 0)
);
```

**Validator**:

```csharp
public class PassengerSearchQueryValidator : AbstractValidator<SearchPassengerQuery>
{
  public PassengerSearchQueryValidator()
  {
    this.RuleFor(x => x.Limit).Limit();      // Custom rule: 1-100
    this.RuleFor(x => x.Skip).Skip();        // Custom rule: >= 0
  }
}
```

**Mapping** (apply defaults):

```csharp
public static PassengerSearch ToDomain(this SearchPassengerQuery query) =>
  new()
  {
    UserId = query.UserId,
    Name = query.Name,
    Limit = query.Limit ?? 20,
    Skip = query.Skip ?? 0,
  };
```

**Response** (Principals only):

```csharp
public record PassengerPrincipalRes(Guid Id, string FullName, string Gender, string PassportExpiry, string PassportNumber);
```

**Admin-Only Variant**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpGet]
public async Task<ActionResult<IEnumerable<UserPrincipalRes>>> Search([FromQuery] SearchUserQuery query)
{
  var x = await userSearchQueryValidator
    .ValidateAsyncResult(query, "Invalid SearchUserQuery")
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()).ToResult());
  return this.ReturnResult(x);
}
```

## 2. GET /api/v1/<resource>/:id - Get Single Resource

**Purpose**: Retrieve full **aggregate root** (with related entities) by ID.

**Pattern**:

- ID as route parameter (strongly typed)
- Optional userId namespacing in route
- Apply authorization (identity check OR admin)
- Return **aggregate root** (not just principal)
- Return 404 if not found

**Example - userId Namespaced**:

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
{
  var r = await this
    .GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)  // User scoping
    .ThenAwait(_ => service.Get(userId, id))                     // Fetch by userId AND id
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(r, new EntityNotFound(
    "Passenger Not Found", typeof(Passenger), id.ToString()));
}
```

**Service Method**:

```csharp
// Fetches by userId AND id (even if id exists, wrong userId returns null)
Task<Result<Passenger?>> Get(string userId, Guid id);
```

**Response - Aggregate Root**:

```csharp
public record PassengerRes(
  PassengerPrincipalRes Principal,    // Core data
  UserPrincipalRes User              // Related entity
);
```

**Optional userId Variant**:

```csharp
[Authorize, HttpGet("{id:guid}")]
public async Task<ActionResult<WalletRes>> Get(Guid id, [FromQuery] string? userId)
{
  // userId can be null ONLY if user has admin role
  var wallet = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(id, userId))  // Service handles optional userId
    .Then(x => x?.ToRes(), Errors.MapNone);

  return this.ReturnNullableResult(wallet, new EntityNotFound("Wallet Not Found", typeof(Wallet), id.ToString()));
}
```

**Admin-Only Variant**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpGet("{id:guid}")]
public async Task<ActionResult<DiscountPrincipalRes>> Get(Guid id)
{
  var discount = await service.Get(id).Then(x => x?.ToRes(), Errors.MapNone);
  return this.ReturnNullableResult(discount, new EntityNotFound("Discount Not Found", typeof(DiscountPrincipal), id.ToString()));
}
```

## 3. POST /api/v1/<resource> - Create Resource

**Purpose**: Create resource with **Record** (mutable) + optional **Property** (initial-only).

**Pattern**:

- `[FromBody]` request DTO
- Validate with FluentValidation
- Map to domain Record (and optional Property)
- Return created **principal** or **aggregate root**
- Optional userId namespacing

**Example - User-Namespaced**:

```csharp
[Authorize, HttpPost("{userId}")]
public async Task<ActionResult<PassengerPrincipalRes>> Create(string userId, [FromBody] CreatePassengerReq req)
{
  var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => createValidator.ValidateAsyncResult(req, "Invalid CreatePassengerReq"))
    .ThenAwait(x => service.Create(userId, x.ToRecord()))  // Convert to Record
    .Then(x => x.ToRes(), Errors.MapAll);                  // Return Principal
  return this.ReturnResult(user);
}
```

**Request DTO**:

```csharp
public record CreatePassengerReq(
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Domain Record** (mutable):

```csharp
public record PassengerRecord
{
  public required string FullName { get; init; }
  public required PassengerGender Gender { get; init; }
  public required DateOnly PassportExpiry { get; init; }
  public required string PassportNumber { get; init; }
}
```

**Mapper**:

```csharp
public static PassengerRecord ToRecord(this CreatePassengerReq req) =>
  new()
  {
    Gender = req.Gender.GenderToDomain(),
    FullName = req.FullName,
    PassportExpiry = req.PassportExpiry.ToDate(),
    PassportNumber = req.PassportNumber,
  };
```

**Validator**:

```csharp
public class CreatePassengerReqValidator : AbstractValidator<CreatePassengerReq>
{
  public CreatePassengerReqValidator()
  {
    this.RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    this.RuleFor(x => x.Gender).NotEmpty().GenderValid();
    this.RuleFor(x => x.PassportExpiry).NotEmpty().DateValid();
    this.RuleFor(x => x.PassportNumber).NotEmpty().MaximumLength(50);
  }
}
```

**Record + Property Pattern**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost]
public async Task<ActionResult<DiscountPrincipalRes>> Create([FromBody] CreateDiscountReq req)
{
  var discount = await createValidator
    .ValidateAsyncResult(req, "Invalid CreateDiscountReq")
    .ThenAwait(x => service.Create(
      x.Record.ToDomain(),   // Mutable record
      x.Target.ToDomain()    // Initial property (cannot update via PUT)
    ))
    .Then(x => x.ToRes(), Errors.MapNone);
  return this.ReturnResult(discount);
}
```

**Request with Property**:

```csharp
public record CreateDiscountReq(
  DiscountRecordReq Record,     // Mutable
  DiscountTargetReq Target      // Initial-only
);
```

## 4. PUT /api/v1/<resource>/:id - Update Resource

**Purpose**: Update resource by **replacing** its **Record** (full replacement, not partial).

**Pattern**:

- ID as route parameter
- `[FromBody]` request DTO (full record)
- Validate with FluentValidation
- Replace record entirely
- Return updated **principal** or **aggregate root**
- Return 404 if not found

**Example - User-Scoped**:

```csharp
[Authorize, HttpPut("{id:guid}")]
public async Task<ActionResult<PassengerPrincipalRes>> Update(
  [FromQuery] string? userId,
  Guid id,
  [FromBody] UpdatePassengerReq req)
{
  var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => updateValidator.ValidateAsyncResult(req, "Invalid UpdatePassengerReq"))
    .ThenAwait(x => service.Update(userId, id, x.ToRecord()))  // Replace record
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(user, new EntityNotFound(
    "Passenger Not Found", typeof(PassengerPrincipal), id.ToString()));
}
```

**Request Model** (same structure as Create):

```csharp
public record UpdatePassengerReq(
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Mapper** (identical to Create):

```csharp
public static PassengerRecord ToRecord(this UpdatePassengerReq req) =>
  new()
  {
    Gender = req.Gender.GenderToDomain(),
    FullName = req.FullName,
    PassportExpiry = req.PassportExpiry.ToDate(),
    PassportNumber = req.PassportNumber,
  };
```

**Multiple Records Pattern**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPut("{id:guid}")]
public async Task<ActionResult<DiscountPrincipalRes>> Update(Guid id, [FromBody] UpdateDiscountReq req)
{
  var discount = await updateValidator
    .ValidateAsyncResult(req, "Invalid UpdateDiscountReq")
    .ThenAwait(x => service.Update(
      id,
      x.Status.ToDomain(),   // Status record
      x.Record.ToDomain(),   // Main record
      x.Target.ToDomain()    // Target record
    ))
    .Then(x => x?.ToRes(), Errors.MapNone);

  return this.ReturnNullableResult(discount, new EntityNotFound("Discount Not Found", typeof(DiscountPrincipal), id.ToString()));
}
```

## 5. DELETE /api/v1/<resource>/:id - Delete Resource

**Purpose**: Permanently delete resource.

**Pattern**:

- ID as route parameter
- Optional userId for authorization
- Return `Unit` (NoContent 204) on success
- Return 404 if not found

**Example - User-Scoped**:

```csharp
[Authorize, HttpDelete("{id:guid}")]
public async Task<ActionResult> Delete([FromQuery] string? userId, Guid id)
{
  var user = await this
    .GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Delete(userId, id));

  return this.ReturnUnitNullableResult(user, new EntityNotFound(
    "Passenger Not Found", typeof(PassengerPrincipal), id.ToString()));
}
```

**Service Method**:

```csharp
Task<Result<Unit?>> Delete(string? userId, Guid id);
```

**Admin-Only Variant**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{id}")]
public async Task<ActionResult> Delete(string id)
{
  var user = await service.Delete(id);
  return this.ReturnUnitNullableResult(user, new EntityNotFound("User Not Found", typeof(UserPrincipal), id));
}
```

**Response**:

- **204 No Content** - Successfully deleted
- **404 Not Found** - Resource doesn't exist or unauthorized

## Complete Controller Template

```csharp
[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/[controller]")]
public class PassengerController(
  IPassengerService service,
  CreatePassengerReqValidator createValidator,
  UpdatePassengerReqValidator updateValidator,
  PassengerSearchQueryValidator searchValidator,
  ILogger<PassengerController> logger,
  IAuthHelper h
) : AtomiControllerBase(h)
{
  // 1. Search
  [Authorize, HttpGet]
  public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
  {
    var x = await this
      .GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => searchValidator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
      .ThenAwait(q => service.Search(q.ToDomain()))
      .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);
    return this.ReturnResult(x);
  }

  // 2. Get
  [Authorize, HttpGet("{userId}/{id:guid}")]
  public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
  {
    var r = await this
      .GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => service.Get(userId, id))
      .Then(x => x?.ToRes(), Errors.MapAll);
    return this.ReturnNullableResult(r, new EntityNotFound("Passenger Not Found", typeof(Passenger), id.ToString()));
  }

  // 3. Create
  [Authorize, HttpPost("{userId}")]
  public async Task<ActionResult<PassengerPrincipalRes>> Create(string userId, [FromBody] CreatePassengerReq req)
  {
    var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => createValidator.ValidateAsyncResult(req, "Invalid CreatePassengerReq"))
      .ThenAwait(x => service.Create(userId, x.ToRecord()))
      .Then(x => x.ToRes(), Errors.MapAll);
    return this.ReturnResult(user);
  }

  // 4. Update
  [Authorize, HttpPut("{id:guid}")]
  public async Task<ActionResult<PassengerPrincipalRes>> Update(string? userId, Guid id, [FromBody] UpdatePassengerReq req)
  {
    var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => updateValidator.ValidateAsyncResult(req, "Invalid UpdatePassengerReq"))
      .ThenAwait(x => service.Update(userId, id, x.ToRecord()))
      .Then(x => x?.ToRes(), Errors.MapAll);
    return this.ReturnNullableResult(user, new EntityNotFound("Passenger Not Found", typeof(PassengerPrincipal), id.ToString()));
  }

  // 5. Delete
  [Authorize, HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(string? userId, Guid id)
  {
    var user = await this
      .GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => service.Delete(userId, id));
    return this.ReturnUnitNullableResult(user, new EntityNotFound("Passenger Not Found", typeof(PassengerPrincipal), id.ToString()));
  }
}
```

## Quick Checklist

When implementing a new content module, ensure:

**1. Search (GET /api/v1/<resource>)**

- [ ] Query parameters with nullable fields
- [ ] Pagination (Limit, Skip with defaults)
- [ ] FluentValidation validator
- [ ] Authorization scoping (Guard or [Authorize])
- [ ] Returns list of Principals
- [ ] Uses `ReturnResult()`

**2. Get (GET /api/v1/<resource>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] Optional userId namespacing
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns aggregate root
- [ ] Uses `ReturnNullableResult()` with EntityNotFound

**3. Create (POST /api/v1/<resource>)**

- [ ] [FromBody] request DTO
- [ ] Record (mutable) and optional Property (initial-only)
- [ ] FluentValidation validator
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns created principal/aggregate
- [ ] Uses `ReturnResult()`

**4. Update (PUT /api/v1/<resource>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] [FromBody] request DTO (full replacement)
- [ ] FluentValidation validator
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns updated principal/aggregate
- [ ] Uses `ReturnNullableResult()` with EntityNotFound

**5. Delete (DELETE /api/v1/<resource>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns Unit (NoContent 204)
- [ ] Uses `ReturnUnitNullableResult()` with EntityNotFound

## Best Practices

### DO

- ✅ Follow the 5-endpoint pattern consistently
- ✅ Use Principals for search results
- ✅ Use Aggregate Roots for Get endpoint
- ✅ Apply pagination defaults (Limit: 20, Skip: 0)
- ✅ Validate all inputs with FluentValidation
- ✅ Use authorization guards for user-scoped resources
- ✅ Use `[Authorize(Policy)]` for admin-only endpoints
- ✅ Return 404 for not found or unauthorized
- ✅ Use strongly-typed IDs (Guid, string)
- ✅ Chain operations with Result monad

### DON'T

- ❌ Return full aggregates in search results
- ❌ Use partial updates (PUT must replace entire record)
- ❌ Skip validation
- ❌ Hardcode pagination limits
- ❌ Allow unauthenticated access
- ❌ Return 200 with empty data instead of 404
- ❌ Mix authorization patterns unnecessarily
- ❌ Update Properties (initial-only fields) via PUT

## Related Skills

- `error-handling` - Error handling and Result monad
- `authorization` - Authorization patterns and guards
- `feature-module` - Complete module structure
- `unit-testing` - Testing domain logic
- `integration-testing` - Testing REST endpoints

## Quick Start

1. **Create** controller inheriting from `AtomiControllerBase`
2. **Inject** service, validators, logger, IAuthHelper
3. **Implement** 5 standard endpoints
4. **Define** Request/Response models (Req, Res, Principal, Aggregate)
5. **Create** FluentValidation validators
6. **Add** mappers (ToRecord, ToDomain, ToRes)
7. **Apply** authorization (Guards or [Authorize])
8. **Test** all endpoints (success + error cases)
