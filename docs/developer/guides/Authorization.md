# Authorization Guide

Goal

- Understand the complete authorization system including guards, roles, and policies.
- Learn how to implement authorization checks in controllers using both declarative and programmatic approaches.
- Configure authorization policies via YAML configuration.

## Authorization Architecture

This application implements a multi-layered authorization system with three complementary mechanisms:

1. **Static Policies** - Declarative authorization via `[Authorize]` attributes
2. **Guard Clauses** - Programmatic authorization checks in BaseController
3. **Policy Configuration** - YAML-based policy definitions

## Static Policies with AuthPolicy Annotation

### Policy Registry

Policies are defined as constants in `App/StartUp/Registry/AuthPolicies.cs`:

```csharp
public class AuthPolicies
{
  public const string OnlyAdmin = "OnlyAdmin";
  public const string AdminOrTin = "AdminOrTin";
}

public class AuthRoles
{
  public const string Field = "roles";
  public const string Admin = "admin";
  public const string Tin = "tin";
}
```

### Usage in Controllers

Apply policies declaratively using the `[Authorize]` attribute:

```csharp
[ApiVersion(1.0)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AdminController(IAdminService service, IAuthHelper h) : AtomiControllerBase(h)
{
  // Only users with "admin" role can access this endpoint
  [Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost("inflow/{userId}")]
  public async Task<ActionResult<WalletPrincipalRes>> TransferIn(string userId, [FromBody] TransferReq req)
  {
    var x = await transferReqValidator
      .ValidateAsyncResult(req, "Invalid TransferReq")
      .ThenAwait(q => service.TransferIn(userId, q.Amount, q.Desc))
      .Then(x => x.ToRes(), Errors.MapNone);
    return this.ReturnResult(x);
  }

  // Simple [Authorize] without policy = any authenticated user
  [Authorize, HttpGet("{id}")]
  public async Task<ActionResult<UserRes>> Get(string id)
  {
    // ... implementation
  }
}
```

## Policy Configuration via YAML

Policies are configured in `App/Config/settings.yaml` under the `Auth:Policies` section:

```yaml
Auth:
  Enabled: true
  Settings:
    Policies:
      # Policy requiring ALL users to have "admin" role
      OnlyAdmin:
        Target:
          - admin
        Type: 'All' # User must have ALL specified roles
        Field: 'roles' # Check the "roles" field in JWT claims

      # Policy requiring user to have EITHER "tin" OR "admin" role
      AdminOrTin:
        Target:
          - tin
          - admin
        Type: 'Any' # User must have ANY of specified roles
        Field: 'roles'

    # JWT validation settings
    Issuer: https://api.descope.com/P2VN0BOW1x838iknfn7JGwsTbqv4
    Audience: P2VN0BOW1x838iknfn7JGwsTbqv4
    Domain: api.descope.com/P2VN0BOW1x838iknfn7JGwsTbqv4

    TokenValidation:
      ValidateAudience: false
      ValidateIssuer: true
      ClockSkew: 0
      ValidateLifetime: true
      ValidateIssuerSigningKey: true
```

### Policy Configuration Options

**Type** (required): `"All"` or `"Any"`

- `"All"` - User must have ALL roles/scopes in `Target` array
- `"Any"` - User must have ANY role/scope in `Target` array

**Field** (required): `"roles"`, `"scope"`, or custom claim field

- `"roles"` - Maps to standard Microsoft role claim: `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`
- `"scope"` - Space-separated OAuth scopes in JWT
- Custom field name - Any custom claim in JWT

**Target** (required): Array of role/scope values to check

## Guard Clauses in BaseController

Guard clauses provide programmatic authorization checks that can be chained with Result monad operations.

### Guard Methods

The `AtomiControllerBase` provides four guard methods:

#### 1. Guard(target)

Simple identity check - verifies JWT subject claim matches target:

```csharp
protected Result<Unit> Guard(string? target)
{
  if (target != null && this.Sub() == target) return new Unit();
  return new Unauthorized(
    "You are not authorized to access this resource",
    [new("sub", this.Sub() ?? "none")],
    [new("sub", target ?? "none")]
  ).ToException();
}
```

**Usage**:

```csharp
[Authorize, HttpPut("{id}")]
public async Task<ActionResult<UserPrincipalRes>> Update(string id, [FromBody] UpdateUserReq req)
{
  var user = await this.GuardAsync(id)  // Only allow user to update their own profile
    .ThenAwait(_ => updateUserReqValidator.ValidateAsyncResult(req, "Invalid UpdateUserReq"))
    .ThenAwait(x => service.Update(id, x.ToRecord()))
    .Then(x => (x?.ToRes()).ToResult());

  return this.ReturnNullableResult(user, new EntityNotFound("User Not Found", typeof(UserPrincipal), id));
}
```

#### 2. GuardOrAll(target, field, values)

Identity check OR user must have ALL specified roles/scopes:

```csharp
protected Result<Unit> GuardOrAll(string? target, string field, params string[] value)
{
  if (
    (target != null && this.Sub() == target)
    ||
    h.HasAll(this.HttpContext.User, field, value)
  ) return new Unit().ToResult();

  h.Logger.LogInformation(
    "Auth Failed (All): Target: {Target}, Sub: {Sub}, Field: {Field}, Value: {@Value}",
    target, this.Sub(), field, value);

  return new Unauthorized(...).ToException();
}
```

**Usage**:

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
{
  // Allow if requester IS the user OR has admin role
  var r = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(userId, id))
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(r, new EntityNotFound(
    "Passenger Not Found", typeof(Passenger), id.ToString()));
}
```

#### 3. GuardOrAny(target, field, values)

Identity check OR user must have ANY of the specified roles/scopes:

```csharp
protected Result<Unit> GuardOrAny(string? target, string field, params string[] value)
{
  if (
    (target != null && this.Sub() == target)
    ||
    h.HasAny(this.HttpContext.User, field, value)
  ) return new Unit().ToResult();

  // ... logging and error handling
  return new Unauthorized(...).ToException();
}
```

**Usage**:

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
{
  // Allow if requester IS the user OR has ANY of: admin, support, tin
  var x = await this
    .GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin, AuthRoles.Support, AuthRoles.Tin)
    .ThenAwait(_ => passengerSearchQueryValidator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);

  return this.ReturnResult(x);
}
```

#### 4. Async Variants

All guards have async variants for chaining with async operations:

- `GuardAsync(target)`
- `GuardOrAllAsync(target, field, values)`
- `GuardOrAnyAsync(target, field, values)`

### Helper: Sub()

Extract JWT subject claim (user ID):

```csharp
protected string? Sub() => this.HttpContext.User.Identity?.Name;
```

## IAuthHelper - Role and Scope Checking

The `IAuthHelper` service provides low-level claim checking:

```csharp
public interface IAuthHelper
{
  bool HasAll(ClaimsPrincipal? user, string field, params string[] scopes);
  bool HasAny(ClaimsPrincipal? user, string field, params string[] scopes);
  IEnumerable<string> FieldToScope(ClaimsPrincipal? user, string field);
  ILogger<IAuthHelper> Logger { get; }
}
```

### Methods

**HasAll(user, field, scopes)** - Returns true if user has ALL specified scopes:

```csharp
public bool HasAll(ClaimsPrincipal? user, string field, params string[] scopes)
{
  var s = this.FieldToScope(user, field);
  var r = scopes.All(scope => s.Contains(scope));
  if (!r) logger.LogInformation("No matching scopes. Field: {Field} Needed: {@Require}, Token: {@Token}",
    field, scopes, s);
  return r;
}
```

**HasAny(user, field, scopes)** - Returns true if user has ANY of the specified scopes:

```csharp
public bool HasAny(ClaimsPrincipal? user, string field, params string[] scopes)
{
  var s = this.FieldToScope(user, field);
  var r = scopes.Any(scope => s.Contains(scope));
  if (!r) logger.LogInformation("No matching scopes. Field: {Field} Needed: {@Require}, Token: {@Token}",
    field, scopes, s);
  return r;
}
```

**FieldToScope(user, field)** - Extract claim values from JWT:

```csharp
public IEnumerable<string> FieldToScope(ClaimsPrincipal? user, string field)
{
  // Map "roles" to standard Microsoft role claim type
  var f = field == "roles"
    ? "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    : field;

  var s = user?
    .FindAll(c => c.Type == f && c.Issuer == this.Issuer)?
    .Select(x => x.Value);

  // Handle space-separated scope claims (e.g., "read write delete")
  if (field == "scope")
    s = s?.SelectMany(x => x.Split(' '));

  return s ?? [];
}
```

### Direct Usage

You can inject `IAuthHelper` and use it directly for custom authorization logic:

```csharp
public class BookingController(
  IBookingService service,
  IAuthHelper h
) : AtomiControllerBase(h)
{
  [Authorize, HttpPost("{userId}/purchase")]
  public async Task<ActionResult<BookingPrincipalRes>> Purchase(string userId, [FromBody] CreateBookingReq req)
  {
    var p = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
      .ThenAwait(_ => createBookingReqValidator.ValidateAsyncResult(req, "Invalid CreateBookingReq"))
      .Then(r => r.ToRecord(), Errors.MapNone)
      .ThenAwait(rec => costCalculator
        // Extract user's roles directly for cost calculation
        .BookingCost(userId, h.FieldToScope(this.HttpContext.User, AuthRoles.Field).ToArray(), rec)
        .Then(cost => (c: cost, r: rec), Errors.MapNone)
      )
      .ThenAwait(cr => service.Create(userId, cr.c, cr.r))
      .Then(b => b.ToRes(), Errors.MapNone);

    return this.ReturnResult(p);
  }
}
```

## Authorization Requirements and Handlers

The system uses ASP.NET Core's `IAuthorizationRequirement` and `IAuthorizationHandler` for policy evaluation.

### Requirements

**HasAllRequirement** - User must have ALL specified scopes:

```csharp
public class HasAllRequirement : IAuthorizationRequirement
{
  public string Issuer { get; }
  public string Field { get; }
  public IEnumerable<string> Scope { get; }

  public HasAllRequirement(string issuer, string field, params string[] scope) { ... }
}
```

**HasAnyRequirement** - User must have ANY specified scope:

```csharp
public class HasAnyRequirement : IAuthorizationRequirement
{
  public string Issuer { get; }
  public string Field { get; }
  public IEnumerable<string> Scope { get; }

  public HasAnyRequirement(string issuer, string field, params string[] scope) { ... }
}
```

### Handlers

**HasAllHandler** - Evaluates HasAllRequirement:

```csharp
public class HasAllHandler : AuthorizationHandler<HasAllRequirement>
{
  protected override Task HandleRequirementAsync(
    AuthorizationHandlerContext context,
    HasAllRequirement requirement)
  {
    // Map "roles" to standard Microsoft role claim
    var field = requirement.Field == "roles"
      ? "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
      : requirement.Field;

    var scopes = context.User
      .FindAll(c => c.Type == field && c.Issuer == requirement.Issuer)?
      .Select(x => x.Value);

    // Handle space-separated scope claims
    if (requirement.Field == "scope")
      scopes = scopes?.SelectMany(x => x.Split(' '));

    if (scopes == null) return Task.CompletedTask;

    // Succeed if user has ALL required scopes
    if (requirement.Scope.All(s => scopes.Contains(s)))
      context.Succeed(requirement);

    return Task.CompletedTask;
  }
}
```

**HasAnyHandler** - Evaluates HasAnyRequirement (similar pattern, uses `.Any()` instead of `.All()`).

## Complete Authorization Flow

```
1. CLIENT REQUEST with JWT Bearer Token
   └── Token contains claims: { "sub": "user123", "roles": "admin", "scope": "read write" }

2. MIDDLEWARE: UseAuthentication()
   ├── Validates JWT signature using issuer's public key
   ├── Validates audience, issuer, expiration
   ├── Creates ClaimsPrincipal with extracted claims
   └── Sets HttpContext.User

3. DECLARATIVE AUTHORIZATION: [Authorize] Attribute
   ├── [Authorize(Policy = "OnlyAdmin")] triggers
   ├── ASP.NET Core finds registered policy from YAML config
   ├── Calls HasAllHandler or HasAnyHandler
   └── Returns 403 Forbidden if denied, or continues if authorized

4. CONTROLLER METHOD EXECUTION
   └── Method body executes if declarative checks passed

5. PROGRAMMATIC AUTHORIZATION: Guard Clauses
   ├── GuardOrAll() or GuardOrAny() called in method
   ├── Checks identity (JWT sub claim) OR role requirements
   ├── Returns Result<Unit> success or Unauthorized error
   └── Chains with Result monad operations

6. RESPONSE
   ├── Success: Return 200/201/204 with response body
   └── Failure: Return 401 Unauthorized or 403 Forbidden with Problem Details
```

## When to Use Each Mechanism

**Static Policies** (`[Authorize(Policy)]`):

- Use for simple role-based checks on entire endpoints
- Best for admin-only or role-only endpoints
- Evaluated before controller method executes
- Cannot access method parameters

**Guard Clauses** (`GuardOrAll`, `GuardOrAny`):

- Use for identity checks (user can only access their own data)
- Use for complex authorization involving method parameters (e.g., userId from route)
- Chains with Result monad for clean error handling
- Can combine identity check with role check

**IAuthHelper** (direct usage):

- Use for custom authorization logic
- Use when you need role/scope information for business logic (not just yes/no)
- Example: Apply different discounts based on user roles

## Common Patterns

### Pattern 1: User-Scoped Resource (UserID in Route)

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
{
  // User can access their own passengers, OR admins can access any
  var r = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(userId, id))
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(r, new EntityNotFound(...));
}
```

**Authorization Logic**:

- If `userId` in route matches JWT sub claim → allow
- OR if user has "admin" role → allow
- Otherwise → 403 Forbidden

### Pattern 2: Admin-Only Endpoint

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost("inflow/{userId}")]
public async Task<ActionResult<WalletPrincipalRes>> TransferIn(string userId, [FromBody] TransferReq req)
{
  var x = await transferReqValidator
    .ValidateAsyncResult(req, "Invalid TransferReq")
    .ThenAwait(q => service.TransferIn(userId, q.Amount, q.Desc))
    .Then(x => x.ToRes(), Errors.MapNone);
  return this.ReturnResult(x);
}
```

**Authorization Logic**:

- User must have "admin" role (from YAML config)
- Checked before method executes
- No additional guards needed

### Pattern 3: Search with Optional UserID Filter

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
{
  // If query.UserId is provided, user must match OR have admin role
  // If query.UserId is null, admin role required
  var x = await this.GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => searchQueryValidator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);

  return this.ReturnResult(x);
}
```

**Authorization Logic**:

- If `query.UserId` provided and matches JWT sub → allow (user searches their own)
- OR if user has admin role → allow (admin searches any/all)
- Otherwise → 403 Forbidden

### Pattern 4: Self-Update Only

```csharp
[Authorize, HttpPut("{id}")]
public async Task<ActionResult<UserPrincipalRes>> Update(string id, [FromBody] UpdateUserReq req)
{
  // User can ONLY update their own profile (no admin override)
  var user = await this.GuardAsync(id)
    .ThenAwait(_ => updateUserReqValidator.ValidateAsyncResult(req, "Invalid UpdateUserReq"))
    .ThenAwait(x => service.Update(id, x.ToRecord()))
    .Then(x => (x?.ToRes()).ToResult());

  return this.ReturnNullableResult(user, new EntityNotFound(...));
}
```

**Authorization Logic**:

- `id` in route must match JWT sub claim
- Even admins cannot update other users' profiles via this endpoint
- Admins would use a separate admin endpoint if needed

## Error Responses

When authorization fails, the system returns RFC 7807 Problem Details:

**401 Unauthorized** - Not authenticated (no valid JWT):

```json
{
  "type": "unauthenticated",
  "title": "Unauthenticated",
  "status": 401,
  "detail": "You are not authenticated"
}
```

**403 Forbidden** - Authenticated but not authorized (Unauthorized problem):

```json
{
  "type": "unauthorized",
  "title": "Unauthorized",
  "status": 403,
  "detail": "You are not authorized to access this resource",
  "granted": [{ "field": "roles", "value": "user" }],
  "required": [{ "field": "roles", "value": "admin" }]
}
```

Where `Scope` is defined as:

```csharp
public record Scope(string Field, string Value);
```

## Related

- Error handling: DefineErrors.md
- REST endpoint patterns: ContentModule.md
- Base controller: `App/Modules/Common/BaseController.cs`
- Auth configuration: `App/Config/settings.yaml`
