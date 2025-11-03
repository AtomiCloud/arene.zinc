---
name: integration-testing
description: Write end-to-end integration tests for Zinc ASP.NET Core 8 API using xUnit and WebApplicationFactory to test complete user flows and business scenarios
---

# Integration Testing Skill

Use this skill when writing integration tests for the Zinc ASP.NET Core 8 API project.

## Related Documentation

- **[examples.md](examples.md)** - Code examples, templates, and real-world test scenarios
- **[reference.md](reference.md)** - Links to official documentation for testing frameworks

## Framework & Tools

- **xUnit** with `[Theory]` for parameterized tests
- **FluentAssertions** for readable assertions
- **WebApplicationFactory** for in-memory test server
- **In-Memory EF Core Database** (no PostgreSQL required)

## Integration Testing Philosophy

Integration tests focus on **end-to-end user flows and business scenarios**:

- **Full Stack**: Test complete request/response cycle through real API endpoints
- **Business Scenarios**: Each test represents a real user journey or business use case
- **Multi-Step Flows**: One scenario can use multiple endpoints together (Create → Get → Update → Delete)
- **Edge Cases**: Test boundary conditions, error cases, and business rule violations
- **No Mocking**: Use real services, real database (in-memory), real controllers
- **User Perspective**: Test what users actually do, not individual functions

## Test Environment: `tauros` Landscape

Integration tests run in the `tauros` landscape:

- **Landscape**: Automatically set by `pls int` task
- **Database**: In-memory EF Core database (bypasses PostgreSQL)
- **Configuration**: `App/Config/settings.tauros.yaml`
- **Services Disabled**: OTEL, Auth, and external services are disabled
- **Test Factory**: `IntTest/Infrastructure/TestWebApplicationFactory.cs` creates custom WebApplication

## Core Testing Principles

### 1. Scenario-Based Organization

Test files represent business scenarios, not just API endpoints:

- `IntTest/Scenarios/ProjectManagement/CreateAndUpdateProjectTests.cs`
- `IntTest/Scenarios/UserOnboarding/RegisterAndVerifyEmailTests.cs`
- `IntTest/Scenarios/Subscriptions/SubscribeAndUnsubscribeTests.cs`
- **Naming Convention**: `{Scenario}Tests.cs` (describe the user flow)

### 2. User Flow Testing

Each test should represent a complete user journey:

**Examples**:

- User registers → verifies email → logs in → updates profile
- Admin creates project → adds users → assigns permissions → archives project
- Customer subscribes to newsletter → receives confirmation → updates preferences → unsubscribes
- User uploads document → shares with team → edits metadata → downloads

### 3. Test Naming Convention (Scenario-Based)

**Format**: `{ScenarioName}_{Context}_{ShouldExpectedOutcome}`

**Examples**:

- `CreateProject_WithValidData_ShouldReturnCreatedProjectAndBeRetrievable`
- `UpdateProject_AfterCreation_ShouldPersistChanges`
- `DeleteProject_WithExistingProject_ShouldReturn204AndNotBeRetrievable`
- `Subscribe_WithDuplicateEmail_ShouldReturnConflictError`
- `UserRegistration_WithInvalidEmail_ShouldReturnValidationError`
- `CompleteCheckout_WithExpiredCart_ShouldReturnBadRequest`

### 4. Multi-Endpoint Flows

Integration tests should test multiple endpoints together to verify complete scenarios:

**Example Flow**:

```csharp
[Fact]
public async Task CompleteProjectLifecycle_ShouldSucceed()
{
  // Step 1: Create project
  var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", createRequest);
  createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
  var project = await createResponse.Content.ReadFromJsonAsync<ProjectRes>();

  // Step 2: Verify project was created
  var getResponse = await _client.GetAsync($"/api/v1/projects/{project!.Id}");
  getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

  // Step 3: Update project
  var updateResponse = await _client.PutAsJsonAsync($"/api/v1/projects/{project.Id}", updateRequest);
  updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

  // Step 4: Verify update persisted
  var verifyResponse = await _client.GetAsync($"/api/v1/projects/{project.Id}");
  var updatedProject = await verifyResponse.Content.ReadFromJsonAsync<ProjectRes>();
  updatedProject!.Name.Should().Be("Updated Name");

  // Step 5: Delete project
  var deleteResponse = await _client.DeleteAsync($"/api/v1/projects/{project.Id}");
  deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

  // Step 6: Verify project is gone
  var checkResponse = await _client.GetAsync($"/api/v1/projects/{project.Id}");
  checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

### 5. Business Edge Cases

Test real-world edge cases from business requirements:

- **Validation**: Invalid email format, missing required fields, out-of-range values
- **Conflicts**: Duplicate names, concurrent updates, resource conflicts
- **Permissions**: Unauthorized access, insufficient privileges
- **State Transitions**: Invalid state changes, expired resources
- **Data Consistency**: Related entities, cascading deletes, referential integrity

### 6. Theory-Based Testing for Variations

Use `[Theory]` to test variations of the same scenario:

```csharp
[Theory]
[InlineData("", "Name is required")]
[InlineData("a", "Name must be at least 3 characters")]
[InlineData("this-name-is-way-too-long-and-exceeds-the-maximum-allowed-length-for-project-names", "Name must not exceed 50 characters")]
public async Task CreateProject_WithInvalidName_ShouldReturnValidationError(
  string invalidName,
  string expectedErrorMessage)
{
  var request = new CreateProjectReq { Name = invalidName };
  var response = await _client.PostAsJsonAsync("/api/v1/projects", request);

  response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
  problem!.Errors.Should().ContainKey("Name");
}
```

## Test Structure with WebApplicationFactory

### Base Test Class

```csharp
public class ProjectManagementTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
  private readonly TestWebApplicationFactory _factory;
  private readonly HttpClient _client;
  private readonly MainDbContext _dbContext;

  public ProjectManagementTests(TestWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
    _dbContext = factory.Services.GetRequiredService<MainDbContext>();
  }

  public async Task InitializeAsync()
  {
    // Database is clean at start of each test
    await Task.CompletedTask;
  }

  public async Task DisposeAsync()
  {
    // Clean up database after each test
    await _dbContext.Database.EnsureDeletedAsync();
    await _dbContext.Database.EnsureCreatedAsync();
  }

  [Fact]
  public async Task CreateProject_WithValidData_ShouldSucceed()
  {
    // Test implementation
  }
}
```

### TestWebApplicationFactory

Located at `IntTest/Infrastructure/TestWebApplicationFactory.cs`:

- Creates in-memory test server
- Configures in-memory database
- Disables authentication/authorization
- Provides HttpClient for API calls

## Database Management

### In-Memory Database

Integration tests use EF Core's in-memory database provider:

- **No PostgreSQL**: Tests run without external database
- **Fast**: In-memory operations are very fast
- **Isolated**: Each test gets a clean database
- **Provider-Aware**: Repository methods detect in-memory vs PostgreSQL

### Database Cleanup

Use `IAsyncLifetime` to manage database state:

```csharp
public async Task InitializeAsync()
{
  // Optional: Seed data needed for tests
  await Task.CompletedTask;
}

public async Task DisposeAsync()
{
  // Clean database after each test
  await _dbContext.Database.EnsureDeletedAsync();
  await _dbContext.Database.EnsureCreatedAsync();
}
```

### JSONB Handling

PostgreSQL JSONB columns are automatically ignored for in-memory database via reflection in `MainDbContext`.

## HTTP Client Usage

### Making Requests

```csharp
// GET
var response = await _client.GetAsync("/api/v1/projects");

// POST
var response = await _client.PostAsJsonAsync("/api/v1/projects", request);

// PUT
var response = await _client.PutAsJsonAsync($"/api/v1/projects/{id}", request);

// DELETE
var response = await _client.DeleteAsync($"/api/v1/projects/{id}");
```

### Reading Responses

```csharp
// Deserialize JSON response
var result = await response.Content.ReadFromJsonAsync<ProjectRes>();

// Read problem details
var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

// Read raw content
var content = await response.Content.ReadAsStringAsync();
```

### Asserting Responses

```csharp
// Status codes
response.StatusCode.Should().Be(HttpStatusCode.OK);
response.StatusCode.Should().Be(HttpStatusCode.Created);
response.StatusCode.Should().Be(HttpStatusCode.NoContent);
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
response.StatusCode.Should().Be(HttpStatusCode.NotFound);
response.StatusCode.Should().Be(HttpStatusCode.Conflict);

// Response content
var result = await response.Content.ReadFromJsonAsync<ProjectRes>();
result!.Name.Should().Be("Expected Name");
result!.Id.Should().NotBeEmpty();

// Problem details
var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
problem!.Status.Should().Be(400);
problem!.Title.Should().Be("Validation Error");
```

## Scenario Coverage Checklist

For each feature, ensure you test these scenarios:

### Happy Path

- [ ] Create resource with valid data
- [ ] Retrieve resource by ID
- [ ] List/search resources with filters
- [ ] Update resource with valid changes
- [ ] Delete resource successfully

### Validation Errors

- [ ] Missing required fields
- [ ] Invalid formats (email, phone, etc.)
- [ ] Out of range values (too long, too short, negative, etc.)
- [ ] Invalid data types

### Business Rule Violations

- [ ] Duplicate unique fields (email, username, name)
- [ ] Invalid state transitions
- [ ] Resource conflicts
- [ ] Cascading constraints

### Edge Cases

- [ ] Empty search results
- [ ] Large datasets (pagination)
- [ ] Concurrent operations
- [ ] Related entities (foreign keys)
- [ ] Soft deletes vs hard deletes

### Error Cases

- [ ] Resource not found (404)
- [ ] Unauthorized access (401)
- [ ] Forbidden operations (403)
- [ ] Conflict errors (409)
- [ ] Server errors (500)

## Running Tests

```bash
# Run all integration tests
pls int

# Run integration tests with coverage
pls int:cover

# Run specific test class
pls exec -- dotnet test --filter "FullyQualifiedName~ProjectManagementTests"

# Run specific test method
pls exec -- dotnet test --filter "FullyQualifiedName~CreateProject_WithValidData"
```

## Pre-Submission Checklist

Before submitting tests, verify:

- [ ] Test represents a complete business scenario or user flow
- [ ] Test name describes the scenario and expected outcome
- [ ] Multiple endpoints are tested together where appropriate
- [ ] All happy path scenarios covered
- [ ] Validation errors tested comprehensively
- [ ] Business rule violations tested
- [ ] Edge cases covered
- [ ] HTTP status codes asserted correctly
- [ ] Response content validated with FluentAssertions
- [ ] Database cleanup implemented with `IAsyncLifetime`
- [ ] Tests are independent and can run in any order
- [ ] Tests pass: `pls int`

## Coverage Targets

| Component  | Coverage Target | Priority |
| ---------- | --------------- | -------- |
| Domain     | 80%+            | High     |
| App/API    | 90%+            | Critical |
| Full Stack | Complete flows  | Critical |

## Reference Test Files

See these files for complete examples:

- `IntTest/Scenarios/Projects/ProjectManagementTests.cs`
- `IntTest/Scenarios/Users/UserRegistrationTests.cs`
- `IntTest/Scenarios/Subscriptions/SubscriptionFlowTests.cs`

## Quick Start

1. **Read** [examples.md](examples.md) for code templates and scenarios
2. **Reference** [reference.md](reference.md) for official documentation
3. **Think** in user flows and business scenarios, not isolated functions
4. **Test** complete journeys from start to finish
5. **Cover** all edge cases and error conditions
6. **Run** with `pls int` before submitting
