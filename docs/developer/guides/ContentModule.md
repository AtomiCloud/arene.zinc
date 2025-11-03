# Content Module - Standard REST Endpoints

Goal

- Define the standard pattern for implementing REST API endpoints in content modules.
- Ensure consistency across all feature modules.
- Provide clear examples of the five standard endpoint types.

## Overview

Every content module should implement five standard REST endpoints following a consistent pattern:

1. **GET /api/v1/\<resource\>** - Search/List with filtering and pagination
2. **GET /api/v1/\<resource\>/:id** - Get single aggregate root by ID
3. **POST /api/v1/\<resource\>** - Create new resource
4. **PUT /api/v1/\<resource\>/:id** - Update existing resource
5. **DELETE /api/v1/\<resource\>/:id** - Delete resource

## Standard Endpoint Patterns

### 1. GET /api/v1/\<resource\> - Search Endpoint

**Purpose**: Search and filter resources with pagination, field-based filtering, and authorization scoping.

**Pattern**:

- Accept query parameters for filtering (nullable fields)
- Support pagination via `Limit` and `Skip`
- Validate search parameters (e.g., limit max results)
- Apply authorization to reduce scope (user can only search their own data, unless admin)
- Return list of **Principals** (summary DTOs, not full aggregates)

**Example - User-Scoped Search**:

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
{
  logger.LogInformation("Searching for passengers, query: {@Query}", query);

  var x = await this
    .GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)  // Authorization scoping
    .ThenAwait(_ => passengerSearchQueryValidator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);

  return this.ReturnResult(x);
}
```

**Request Model** (with nullable fields for optional filters):

```csharp
public record SearchPassengerQuery(
  string? UserId,           // Namespace/scope filter (required for non-admins)
  string? Name,             // Field filter (optional)
  int? Limit,              // Pagination (optional, defaults to 20)
  int? Skip                // Pagination (optional, defaults to 0)
);
```

**Validator** (validate limits):

```csharp
public class PassengerSearchQueryValidator : AbstractValidator<SearchPassengerQuery>
{
  public PassengerSearchQueryValidator()
  {
    this.RuleFor(x => x.Limit)
      .Limit();              // Custom rule: must be 1-100
    this.RuleFor(x => x.Skip)
      .Skip();               // Custom rule: must be >= 0
  }
}
```

**Response** (Principals only):

```csharp
public record PassengerPrincipalRes(
  Guid Id,
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Domain Mapping** (apply defaults):

```csharp
public static PassengerSearch ToDomain(this SearchPassengerQuery query) =>
  new()
  {
    UserId = query.UserId,
    Name = query.Name,
    Limit = query.Limit ?? 20,           // Default pagination
    Skip = query.Skip ?? 0,               // Default pagination
  };
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

**Authorization Patterns**:

- **User-scoped**: `GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)`
  - If `userId` in query matches JWT sub → allow (user searches their own)
  - OR if user has admin role → allow (admin searches any/all)
- **Admin-only**: `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`
  - Only admins can search (no user scoping)

---

### 2. GET /api/v1/\<resource\>/:id - Get Single Resource

**Purpose**: Retrieve a single **aggregate root** (full entity with related data) by ID.

**Pattern**:

- Accept ID as route parameter (strongly typed, e.g., `Guid` or `string`)
- Optionally namespace under user (e.g., `/api/v1/{userId}/passengers/{id}`)
- Apply authorization (user can only get their own, OR admins can get any)
- Return full **aggregate root** (not just principal)
- Return 404 if not found or unauthorized

**Example - UserID Namespaced**:

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
{
  var r = await this
    .GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)  // User scoping
    .ThenAwait(_ => service.Get(userId, id))                     // Domain method with userId AND id
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(r, new EntityNotFound(
    "Passenger Not Found", typeof(Passenger), id.ToString()));
}
```

**Domain Method Signature**:

```csharp
// Service fetches by BOTH userId and id (AND clause)
// Even if id exists but doesn't belong to userId → NotFoundxException
Task<Result<Passenger?>> Get(string userId, Guid id);
```

**Response - Aggregate Root**:

```csharp
public record PassengerRes(
  PassengerPrincipalRes Principal,    // Core passenger data
  UserPrincipalRes User              // Related user entity
);

public record PassengerPrincipalRes(
  Guid Id,
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);

public record UserPrincipalRes(
  string Id,
  string Username
);
```

**Admin Variant - Optional UserId**:

```csharp
[Authorize, HttpGet("{id:guid}")]
public async Task<ActionResult<WalletRes>> Get(Guid id, [FromQuery] string? userId)
{
  // userId can be null ONLY if user has admin role
  var wallet = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(id, userId))  // Repository handles optional userId
    .Then(x => x?.ToRes(), Errors.MapNone);

  return this.ReturnNullableResult(wallet, new EntityNotFound(
    "Wallet Not Found", typeof(Wallet), id.ToString()));
}
```

**Domain Method with Optional UserID**:

```csharp
// If userId is null → fetch by id only (admin access)
// If userId is provided → fetch by id AND userId (user access)
Task<Result<Wallet?>> Get(Guid id, string? userId);
```

**Simple Variant - No User Scoping**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpGet("{id:guid}")]
public async Task<ActionResult<DiscountPrincipalRes>> Get(Guid id)
{
  var discount = await service.Get(id)
    .Then(x => x?.ToRes(), Errors.MapNone);

  return this.ReturnNullableResult(discount, new EntityNotFound(
    "Discount Not Found", typeof(DiscountPrincipal), id.ToString()));
}
```

**Authorization Patterns**:

- **User-namespaced**: Route includes `{userId}`, guard ensures `userId` matches JWT sub OR user is admin
- **Optional userId**: Query parameter `userId`, null allowed only for admins
- **Admin-only**: No user scoping, `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`

---

### 3. POST /api/v1/\<resource\> - Create Resource

**Purpose**: Create a new resource using a **Record** (required fields) and optional **Property** (initial-only fields).

**Pattern**:

- Accept `[FromBody]` request DTO containing record data and optional properties
- **Record**: Value type containing mutable fields (can be changed after creation)
- **Property**: Value type containing immutable fields (set only at creation, cannot be updated)
- Validate request using FluentValidation
- Return created **aggregate root** or **principal**
- Optionally namespace under user (e.g., `/api/v1/{userId}/passengers`)

**Example - User-Namespaced Create**:

```csharp
[Authorize, HttpPost("{userId}")]
public async Task<ActionResult<PassengerPrincipalRes>> Create(string userId, [FromBody] CreatePassengerReq req)
{
  var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => createPassengerReqValidator.ValidateAsyncResult(req, "Invalid CreatePassengerReq"))
    .ThenAwait(x => service.Create(userId, x.ToRecord()))  // Convert to Record
    .Then(x => x.ToRes(), Errors.MapAll);                  // Return Principal
  return this.ReturnResult(user);
}
```

**Request Model** (DTO):

```csharp
public record CreatePassengerReq(
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Domain Model - Record** (mutable fields):

```csharp
public record PassengerRecord
{
  public required string FullName { get; init; }
  public required PassengerGender Gender { get; init; }
  public required DateOnly PassportExpiry { get; init; }
  public required string PassportNumber { get; init; }
}
```

**Mapper** (DTO → Domain Record):

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

**Response** (Principal - newly created):

```csharp
public record PassengerPrincipalRes(
  Guid Id,
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Validator**:

```csharp
public class CreatePassengerReqValidator : AbstractValidator<CreatePassengerReq>
{
  public CreatePassengerReqValidator()
  {
    this.RuleFor(x => x.FullName)
      .NotEmpty()
      .MaximumLength(200);
    this.RuleFor(x => x.Gender)
      .NotEmpty()
      .GenderValid();
    this.RuleFor(x => x.PassportExpiry)
      .NotEmpty()
      .DateValid();
    this.RuleFor(x => x.PassportNumber)
      .NotEmpty()
      .MaximumLength(50);
  }
}
```

**Example - Record + Property Pattern**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost]
public async Task<ActionResult<DiscountPrincipalRes>> Create([FromBody] CreateDiscountReq req)
{
  var discount = await createDiscountReqValidator
    .ValidateAsyncResult(req, "Invalid CreateDiscountReq")
    .ThenAwait(x => service.Create(
      x.Record.ToDomain(),   // Mutable record
      x.Target.ToDomain()    // Initial property (cannot be updated via PUT)
    ))
    .Then(x => x.ToRes(), Errors.MapNone);
  return this.ReturnResult(discount);
}
```

**Request with Record + Property**:

```csharp
public record CreateDiscountReq(
  DiscountRecordReq Record,     // Mutable fields
  DiscountTargetReq Target      // Initial-only property
);

public record DiscountRecordReq(
  string Name,
  string Description,
  decimal Amount,
  string Type
);

public record DiscountTargetReq(
  string MatchMode,
  DiscountMatchReq[] Matches
);
```

**Authorization Patterns**:

- **User-namespaced**: Route includes `{userId}`, guard ensures user can create for themselves OR is admin
- **Current user context**: Extract `userId` from JWT (`this.Sub()`) instead of route
- **Admin-only**: `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`

---

### 4. PUT /api/v1/\<resource\>/:id - Update Resource

**Purpose**: Update an existing resource by **replacing** its **Record** (mutable fields).

**Pattern**:

- Accept ID as route parameter
- Accept `[FromBody]` request DTO containing updated record
- **Important**: PUT replaces the entire record (not partial update)
- Properties (initial-only fields) cannot be updated
- Validate request using FluentValidation
- Return updated **aggregate root** or **principal**
- Return 404 if not found or unauthorized

**Example - User-Scoped Update**:

```csharp
[Authorize, HttpPut("{id:guid}")]
public async Task<ActionResult<PassengerPrincipalRes>> Update(
  [FromQuery] string? userId,
  Guid id,
  [FromBody] UpdatePassengerReq req)
{
  var user = await this.GuardOrAnyAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => updatePassengerReqValidator.ValidateAsyncResult(req, "Invalid UpdatePassengerReq"))
    .ThenAwait(x => service.Update(userId, id, x.ToRecord()))  // Replace record
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(user, new EntityNotFound(
    "Passenger Not Found", typeof(PassengerPrincipal), id.ToString()));
}
```

**Request Model** (same structure as Create, for full replacement):

```csharp
public record UpdatePassengerReq(
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Mapper** (DTO → Domain Record):

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

**Example - Update with Multiple Components**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPut("{id:guid}")]
public async Task<ActionResult<DiscountPrincipalRes>> Update(Guid id, [FromBody] UpdateDiscountReq req)
{
  var discount = await updateDiscountReqValidator
    .ValidateAsyncResult(req, "Invalid UpdateDiscountReq")
    .ThenAwait(x => service.Update(
      id,
      x.Status.ToDomain(),   // Status record
      x.Record.ToDomain(),   // Main record
      x.Target.ToDomain()    // Target record
    ))
    .Then(x => x?.ToRes(), Errors.MapNone);

  return this.ReturnNullableResult(discount, new EntityNotFound(
    "Discount Not Found", typeof(DiscountPrincipal), id.ToString()));
}
```

**Request with Multiple Records**:

```csharp
public record UpdateDiscountReq(
  DiscountRecordReq Record,
  DiscountTargetReq Target,
  DiscountStatusReq Status
);

public record DiscountStatusReq(bool Disabled);
```

**Authorization Patterns**:

- **User-scoped**: Optional `userId` query param, guard ensures match OR admin
- **Self-only**: Guard without admin override (e.g., user can only update own profile)
- **Admin-only**: `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`

---

### 5. DELETE /api/v1/\<resource\>/:id - Delete Resource

**Purpose**: Permanently delete a resource.

**Pattern**:

- Accept ID as route parameter
- Optionally accept userId for authorization
- Return `Unit` (NoContent 204) on success
- Return 404 if not found or unauthorized
- Use `ReturnUnitNullableResult()` helper

**Example - User-Scoped Delete**:

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

**Domain Method**:

```csharp
// Returns Unit? (null if not found)
Task<Result<Unit?>> Delete(string? userId, Guid id);
```

**Example - Admin-Only Delete**:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpDelete("{id}")]
public async Task<ActionResult> Delete(string id)
{
  var user = await service.Delete(id);
  return this.ReturnUnitNullableResult(user, new EntityNotFound(
    "User Not Found", typeof(UserPrincipal), id));
}
```

**Response**:

- **204 No Content** - Successfully deleted
- **404 Not Found** - Resource doesn't exist or user unauthorized

**Authorization Patterns**:

- **User-scoped**: Optional `userId`, guard ensures match OR admin
- **Admin-only**: `[Authorize(Policy = AuthPolicies.OnlyAdmin)]`

---

## BaseController Helper Methods

All standard endpoints use helper methods from `AtomiControllerBase` for consistent error handling:

### ReturnResult\<T\>(Result\<T\> entity)

For operations returning data (Create, Search):

```csharp
protected ActionResult<T> ReturnResult<T>(Result<T> entity)
{
  return entity.IsSuccess()
    ? this.Ok(entity.Get())
    : this.MapException<T>(entity.FailureOrDefault());
}
```

**Returns**:

- **200 OK** with entity on success
- **Error status code** (400, 403, 404, etc.) on failure

### ReturnNullableResult\<T\>(Result\<T?\> entity, EntityNotFound notFound)

For operations returning nullable data (Get, Update):

```csharp
protected ActionResult<T> ReturnNullableResult<T>(Result<T?> entity, EntityNotFound notFound)
{
  if (entity.IsSuccess())
  {
    var suc = entity.Get();
    return suc == null ? this.Error<T>(HttpStatusCode.NotFound, notFound) : this.Ok(suc);
  }
  var e = entity.FailureOrDefault();
  return this.MapException<T>(e);
}
```

**Returns**:

- **200 OK** with entity if found
- **404 Not Found** with EntityNotFound problem if null
- **Error status code** on failure

### ReturnUnitNullableResult(Result\<Unit?\> ent, EntityNotFound notFound)

For operations returning Unit (Delete):

```csharp
protected ActionResult ReturnUnitNullableResult(Result<Unit?> ent, EntityNotFound notFound)
{
  if (ent.IsSuccess())
  {
    var suc = ent.Get();
    return suc == null ? this.Error(HttpStatusCode.NotFound, notFound) : this.NoContent();
  }
  var e = ent.FailureOrDefault();
  return this.MapException(e);
}
```

**Returns**:

- **204 No Content** if deleted
- **404 Not Found** with EntityNotFound problem if not found
- **Error status code** on failure

### ReturnUnitResult(Result\<Unit\> ent)

For operations returning non-nullable Unit:

```csharp
protected ActionResult ReturnUnitResult(Result<Unit> ent)
{
  if (ent.IsSuccess()) return this.NoContent();
  var e = ent.FailureOrDefault();
  return this.MapException(e);
}
```

**Returns**:

- **204 No Content** on success
- **Error status code** on failure

---

## Data Models Pattern

### Value Types

**Principal** - Summary DTO (for lists/search results):

```csharp
public record PassengerPrincipalRes(
  Guid Id,
  string FullName,
  string Gender,
  string PassportExpiry,
  string PassportNumber
);
```

**Aggregate Root** - Full entity with related data (for Get endpoint):

```csharp
public record PassengerRes(
  PassengerPrincipalRes Principal,    // Core data
  UserPrincipalRes User              // Related entity
);
```

**Record** - Mutable fields (can be updated):

```csharp
public record PassengerRecord
{
  public required string FullName { get; init; }
  public required PassengerGender Gender { get; init; }
  public required DateOnly PassportExpiry { get; init; }
  public required string PassportNumber { get; init; }
}
```

**Property** - Initial-only fields (cannot be updated):

```csharp
public record DiscountTarget
{
  public required MatchMode MatchMode { get; init; }
  public required DiscountMatch[] Matches { get; init; }
}
```

---

## Validation Pattern

Use FluentValidation for all request DTOs:

```csharp
public class CreatePassengerReqValidator : AbstractValidator<CreatePassengerReq>
{
  public CreatePassengerReqValidator()
  {
    this.RuleFor(x => x.FullName)
      .NotEmpty()
      .MaximumLength(200);

    this.RuleFor(x => x.Gender)
      .NotEmpty()
      .GenderValid();  // Custom validator

    this.RuleFor(x => x.PassportExpiry)
      .NotEmpty()
      .DateValid();    // Custom validator

    this.RuleFor(x => x.PassportNumber)
      .NotEmpty()
      .MaximumLength(50);
  }
}
```

**Nested Validators** (for complex requests):

```csharp
public class CreateDiscountReqValidator : AbstractValidator<CreateDiscountReq>
{
  public CreateDiscountReqValidator()
  {
    this.RuleFor(x => x.Target)
      .NotNull()
      .SetValidator(new DiscountTargetReqValidator());

    this.RuleFor(x => x.Record)
      .NotNull()
      .SetValidator(new DiscountRecordReqValidator());
  }
}
```

**Usage in Controllers**:

```csharp
await createPassengerReqValidator
  .ValidateAsyncResult(req, "Invalid CreatePassengerReq")
  .ThenAwait(x => service.Create(userId, x.ToRecord()))
```

On validation failure, returns **400 Bad Request** with `ValidationError` problem details.

---

## Complete Example - Passenger Module

**Controller** (`App/Modules/Passengers/API/V1/PassengerController.cs`):

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

---

## Summary - Standard Endpoint Checklist

When implementing a new content module, ensure:

**1. Search Endpoint (GET /api/v1/\<resource\>)**

- [ ] Query parameters with nullable fields
- [ ] Pagination support (Limit, Skip with defaults)
- [ ] FluentValidation validator
- [ ] Authorization scoping (GuardOrAny/GuardOrAll or [Authorize])
- [ ] Returns list of Principals
- [ ] Uses `ReturnResult()`

**2. Get Endpoint (GET /api/v1/\<resource\>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] Optional userId namespacing
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns aggregate root (not just principal)
- [ ] Uses `ReturnNullableResult()` with EntityNotFound

**3. Create Endpoint (POST /api/v1/\<resource\>)**

- [ ] [FromBody] request DTO
- [ ] Record (mutable) and optional Property (initial-only)
- [ ] FluentValidation validator
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns created principal or aggregate
- [ ] Uses `ReturnResult()`

**4. Update Endpoint (PUT /api/v1/\<resource\>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] [FromBody] request DTO (full record replacement)
- [ ] FluentValidation validator
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns updated principal or aggregate
- [ ] Uses `ReturnNullableResult()` with EntityNotFound

**5. Delete Endpoint (DELETE /api/v1/\<resource\>/:id)**

- [ ] Strongly-typed ID parameter
- [ ] Authorization (Guard or [Authorize])
- [ ] Returns Unit (NoContent 204)
- [ ] Uses `ReturnUnitNullableResult()` with EntityNotFound

## Related

- Authorization patterns: Authorization.md
- Error handling: DefineErrors.md
- Module walkthrough: NewFeatureWalkthrough.md
- Base controller: `App/Modules/Common/BaseController.cs`
