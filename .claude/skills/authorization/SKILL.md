---
name: authorization
description: Implement authorization using guards, roles, policies, and YAML configuration in Zinc ASP.NET Core 8 API
---

# Authorization Skill

Use this skill when implementing authorization checks, configuring policies, or setting up role-based access control in the Zinc ASP.NET Core 8 API project.

## Overview

Zinc implements a multi-layered authorization system with three complementary mechanisms:

1. **Static Policies** - Declarative via `[Authorize(Policy)]` attributes
2. **Guard Clauses** - Programmatic checks in BaseController (`GuardOrAll`, `GuardOrAny`)
3. **Policy Configuration** - YAML-based policy definitions

## Critical Components

**Files**:

- `App/Modules/Common/BaseController.cs` - Guard clause methods
- `App/StartUp/Registry/AuthPolicies.cs` - Policy name constants
- `App/StartUp/Services/Auth/AuthHelper.cs` - Role/scope checking
- `App/StartUp/Services/Auth/HasAllHandler.cs`, `HasAnyHandler.cs` - Policy handlers
- `App/Config/settings.yaml` - Policy configuration

**Key Concepts**:

- **Subject (sub)** - User ID from JWT token
- **Roles** - User roles from JWT claims (e.g., "admin", "user")
- **Scopes** - OAuth-style permissions from JWT claims
- **Policies** - Named authorization requirements (configured in YAML)

## Authorization Flow

```
1. Request with JWT Bearer Token
   ↓
2. UseAuthentication() Middleware
   ↓ Validates JWT, creates ClaimsPrincipal
3. [Authorize] Attribute (Declarative)
   ↓ Evaluates policy, returns 403 if denied
4. Controller Method Execution
   ↓
5. Guard Clauses (Programmatic)
   ↓ Checks identity/roles, returns Unauthorized error
6. Response (200/401/403)
```

## When to Use Each Mechanism

**Static Policies** `[Authorize(Policy = "...")]`:

- ✅ Simple role-based checks on entire endpoints
- ✅ Admin-only or role-only endpoints
- ✅ Cannot access method parameters
- ❌ Cannot check identity against route parameters

**Guard Clauses** (`GuardOrAll`, `GuardOrAny`):

- ✅ Identity checks (user accessing own data)
- ✅ Complex authorization involving route parameters
- ✅ Chains with Result monad
- ✅ Can combine identity + role checks
- ❌ More verbose than attributes

**IAuthHelper** (direct usage):

- ✅ Custom authorization logic
- ✅ Need role/scope info for business logic (not just yes/no)
- ✅ Apply different behavior based on roles

## Static Policies

### Define Policy Constants

```csharp
// App/StartUp/Registry/AuthPolicies.cs
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

### Configure in YAML

```yaml
# App/Config/settings.yaml
Auth:
  Enabled: true
  Settings:
    Policies:
      # User must have ALL specified roles
      OnlyAdmin:
        Target:
          - admin
        Type: 'All' # "All" or "Any"
        Field: 'roles' # "roles", "scope", or custom

      # User must have ANY specified role
      AdminOrTin:
        Target:
          - tin
          - admin
        Type: 'Any'
        Field: 'roles'

    Issuer: https://api.descope.com/PROJECT_ID
    Audience: PROJECT_ID
    Domain: api.descope.com/PROJECT_ID
```

### Use in Controllers

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost("admin-action")]
public async Task<ActionResult<ResultRes>> AdminAction([FromBody] AdminReq req)
{
  // Only users with "admin" role can access
  var result = await service.DoAdminThing(req);
  return this.ReturnResult(result);
}

[Authorize, HttpGet("protected")]
public async Task<ActionResult<DataRes>> Protected()
{
  // Any authenticated user can access
  var result = await service.GetData();
  return this.ReturnResult(result);
}
```

## Guard Clauses

### Guard Methods

**Guard(target)** - Identity check only:

```csharp
protected Result<Unit> Guard(string? target)
protected Task<Result<Unit>> GuardAsync(string? target)
```

Checks if JWT `sub` claim matches `target`.

**Example**:

```csharp
[Authorize, HttpPut("{id}")]
public async Task<ActionResult<UserPrincipalRes>> Update(string id, [FromBody] UpdateUserReq req)
{
  // User can ONLY update their own profile
  var user = await this.GuardAsync(id)
    .ThenAwait(_ => validator.ValidateAsyncResult(req, "Invalid UpdateUserReq"))
    .ThenAwait(x => service.Update(id, x.ToRecord()))
    .Then(x => (x?.ToRes()).ToResult());

  return this.ReturnNullableResult(user, new EntityNotFound("User Not Found", typeof(UserPrincipal), id));
}
```

**GuardOrAll(target, field, values)** - Identity OR user has ALL roles:

```csharp
protected Result<Unit> GuardOrAll(string? target, string field, params string[] value)
protected Task<Result<Unit>> GuardOrAllAsync(string? target, string field, params string[] value)
```

Allows if `sub` matches `target` OR user has ALL specified roles/scopes.

**Example**:

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<PassengerRes>> Get(string userId, Guid id)
{
  // Allow if user IS userId OR has admin role
  var r = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(userId, id))
    .Then(x => x?.ToRes(), Errors.MapAll);

  return this.ReturnNullableResult(r, new EntityNotFound("Passenger Not Found", typeof(Passenger), id.ToString()));
}
```

**GuardOrAny(target, field, values)** - Identity OR user has ANY role:

```csharp
protected Result<Unit> GuardOrAny(string? target, string field, params string[] value)
protected Task<Result<Unit>> GuardOrAnyAsync(string? target, string field, params string[] value)
```

Allows if `sub` matches `target` OR user has ANY of specified roles/scopes.

**Example**:

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<PassengerPrincipalRes>>> Search([FromQuery] SearchPassengerQuery query)
{
  // Allow if user IS query.UserId OR has ANY of: admin, support, tin
  var x = await this
    .GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin, AuthRoles.Support, AuthRoles.Tin)
    .ThenAwait(_ => validator.ValidateAsyncResult(query, "Invalid SearchPassengerQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);

  return this.ReturnResult(x);
}
```

### Helper: Sub()

Extract JWT subject claim:

```csharp
protected string? Sub() => this.HttpContext.User.Identity?.Name;
```

## IAuthHelper - Role/Scope Checking

Inject `IAuthHelper` for low-level claim checks:

```csharp
public class MyController(
  IMyService service,
  IAuthHelper h
) : AtomiControllerBase(h)
{
  // ...
}
```

**Methods**:

```csharp
// Check if user has ALL specified scopes
bool HasAll(ClaimsPrincipal? user, string field, params string[] scopes)

// Check if user has ANY specified scope
bool HasAny(ClaimsPrincipal? user, string field, params string[] scopes)

// Extract claim values from JWT
IEnumerable<string> FieldToScope(ClaimsPrincipal? user, string field)
```

**Field Parameter**:

- `"roles"` → Maps to `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`
- `"scope"` → Space-separated OAuth scopes
- Custom string → Custom claim type

**Example - Using roles for business logic**:

```csharp
[Authorize, HttpPost("{userId}/purchase")]
public async Task<ActionResult<BookingPrincipalRes>> Purchase(string userId, [FromBody] CreateBookingReq req)
{
  var p = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => validator.ValidateAsyncResult(req, "Invalid CreateBookingReq"))
    .Then(r => r.ToRecord(), Errors.MapNone)
    .ThenAwait(rec =>
      {
        // Extract user's roles for pricing logic
        var roles = h.FieldToScope(this.HttpContext.User, AuthRoles.Field).ToArray();
        return costCalculator.BookingCost(userId, roles, rec)
          .Then(cost => (c: cost, r: rec), Errors.MapNone);
      }
    )
    .ThenAwait(cr => service.Create(userId, cr.c, cr.r))
    .Then(b => b.ToRes(), Errors.MapNone);

  return this.ReturnResult(p);
}
```

## Common Patterns

### Pattern 1: User-Scoped Resource (userId in Route)

User can access their own data, OR admins can access any:

```csharp
[Authorize, HttpGet("{userId}/{id:guid}")]
public async Task<ActionResult<ResourceRes>> Get(string userId, Guid id)
{
  var r = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(userId, id))
    .Then(x => x?.ToRes(), Errors.MapAll);
  return this.ReturnNullableResult(r, new EntityNotFound(...));
}
```

**Authorization**: `userId` matches `sub` OR user has "admin" role.

### Pattern 2: Admin-Only Endpoint

Only admins can access:

```csharp
[Authorize(Policy = AuthPolicies.OnlyAdmin), HttpPost("admin-action")]
public async Task<ActionResult<ResultRes>> AdminAction([FromBody] AdminReq req)
{
  var x = await service.DoAdminThing(req);
  return this.ReturnResult(x);
}
```

**Authorization**: User must have "admin" role (from YAML config).

### Pattern 3: Search with Optional userId Filter

If `userId` provided, user must match OR be admin. If null, admin required:

```csharp
[Authorize, HttpGet]
public async Task<ActionResult<IEnumerable<ResourcePrincipalRes>>> Search([FromQuery] SearchQuery query)
{
  var x = await this.GuardOrAnyAsync(query.UserId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => validator.ValidateAsyncResult(query, "Invalid SearchQuery"))
    .ThenAwait(q => service.Search(q.ToDomain()))
    .Then(x => x.Select(u => u.ToRes()), Errors.MapAll);
  return this.ReturnResult(x);
}
```

**Authorization**: If `query.UserId` matches `sub` → allow. OR if user has admin → allow.

### Pattern 4: Self-Update Only

User can ONLY update their own data (no admin override):

```csharp
[Authorize, HttpPut("{id}")]
public async Task<ActionResult<UserPrincipalRes>> Update(string id, [FromBody] UpdateUserReq req)
{
  var user = await this.GuardAsync(id)
    .ThenAwait(_ => validator.ValidateAsyncResult(req, "Invalid UpdateUserReq"))
    .ThenAwait(x => service.Update(id, x.ToRecord()))
    .Then(x => (x?.ToRes()).ToResult());
  return this.ReturnNullableResult(user, new EntityNotFound(...));
}
```

**Authorization**: `id` must match `sub`. Even admins cannot use this endpoint for other users.

### Pattern 5: Optional userId Query Parameter

```csharp
[Authorize, HttpGet("{id:guid}")]
public async Task<ActionResult<WalletRes>> Get(Guid id, [FromQuery] string? userId)
{
  // userId can be null ONLY if user has admin role
  var wallet = await this.GuardOrAllAsync(userId, AuthRoles.Field, AuthRoles.Admin)
    .ThenAwait(_ => service.Get(id, userId))
    .Then(x => x?.ToRes(), Errors.MapNone);
  return this.ReturnNullableResult(wallet, new EntityNotFound(...));
}
```

**Authorization**: If `userId` provided and matches `sub` → allow. OR if admin → allow (userId can be null).

## Error Responses

**401 Unauthorized** - Not authenticated (no/invalid JWT):

```json
{
  "type": "unauthenticated",
  "title": "Unauthenticated",
  "status": 401,
  "detail": "You are not authenticated"
}
```

**403 Forbidden** - Authenticated but not authorized:

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

## Adding New Policies

### 1. Define Constant

```csharp
// App/StartUp/Registry/AuthPolicies.cs
public class AuthPolicies
{
  // ... existing
  public const string SuperAdmin = "SuperAdmin";
}
```

### 2. Configure in YAML

```yaml
# App/Config/settings.yaml
Auth:
  Settings:
    Policies:
      SuperAdmin:
        Target:
          - superadmin
        Type: 'All'
        Field: 'roles'
```

### 3. Use in Controllers

```csharp
[Authorize(Policy = AuthPolicies.SuperAdmin), HttpDelete("dangerous")]
public async Task<ActionResult> DangerousAction()
{
  // ...
}
```

## Best Practices

### DO

- ✅ Use `[Authorize(Policy)]` for simple role checks
- ✅ Use Guards for identity + route parameter checks
- ✅ Chain guards with Result monad operations
- ✅ Extract roles via IAuthHelper for business logic
- ✅ Define policy constants in `AuthPolicies`
- ✅ Configure policies in YAML (never hardcode)
- ✅ Use `GuardOrAll` for strict checks (AND)
- ✅ Use `GuardOrAny` for flexible checks (OR)
- ✅ Always use `[Authorize]` at minimum (require authentication)

### DON'T

- ❌ Hardcode role names in controllers
- ❌ Skip `[Authorize]` attribute (endpoints are public by default)
- ❌ Mix declarative and programmatic checks unnecessarily
- ❌ Forget to configure policies in YAML
- ❌ Use guards when simple `[Authorize(Policy)]` suffices
- ❌ Allow null `userId` without admin check
- ❌ Return 200 with empty data instead of 403 Forbidden

## Testing Authorization

### Test Policy Checks

```csharp
[Fact]
public async Task AdminAction_WithoutAdminRole_ShouldReturn403()
{
  // Arrange
  var client = factory.CreateClient();
  var token = CreateTokenWithRoles("user"); // Not admin

  client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

  // Act
  var response = await client.PostAsync("/api/v1/admin/action", content);

  // Assert
  response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

### Test Guard Clauses

```csharp
[Fact]
public async Task Get_OtherUserWithoutAdmin_ShouldReturn403()
{
  // Arrange
  var client = factory.CreateClient();
  var token = CreateTokenWithSub("user123"); // Not user456

  client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);

  // Act
  var response = await client.GetAsync("/api/v1/passengers/user456/guid");

  // Assert
  response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```

## Troubleshooting

### 401 Unauthorized (should be 403)

- Check if `[Authorize]` attribute is present
- Verify JWT token is being sent in Authorization header
- Check token expiration

### 403 Forbidden (should allow)

- Verify role claims in JWT token (inspect token at jwt.io)
- Check policy configuration in YAML
- Verify `Field` setting ("roles" vs "scope")
- Check if issuer matches between JWT and config
- Add logging in guards to see what was checked

### Guard not triggering

- Ensure `GuardAsync` is awaited in chain
- Check that Result chain uses `ThenAwait` (not `Then`)
- Verify `target` parameter is not null when checking identity

### Policy not found

- Check policy name matches constant in `AuthPolicies`
- Verify policy is defined in YAML
- Check LANDSCAPE environment variable is set

## Quick Start

1. **Define** policy constants in `App/StartUp/Registry/AuthPolicies.cs`
2. **Configure** policies in `App/Config/settings.yaml`
3. **Apply** `[Authorize(Policy)]` for role checks
4. **Use** guards for identity + role checks
5. **Inject** `IAuthHelper` for custom logic
6. **Test** with different roles and identities
