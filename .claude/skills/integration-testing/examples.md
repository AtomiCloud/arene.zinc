# Integration Testing Examples

Code examples, templates, and patterns for writing integration tests in the Zinc project.

## Table of Contents

- [Complete User Flow Example](#complete-user-flow-example)
- [Multi-Step Scenario Example](#multi-step-scenario-example)
- [Validation Testing](#validation-testing)
- [Business Rule Violations](#business-rule-violations)
- [Edge Cases](#edge-cases)
- [Search and Pagination](#search-and-pagination)
- [Error Handling](#error-handling)
- [Test Class Structure](#test-class-structure)
- [Quick Templates](#quick-templates)

## Complete User Flow Example

Testing a complete project lifecycle from creation to deletion:

```csharp
using System.Net;
using System.Net.Http.Json;
using App.Modules.Projects.API.V1;
using FluentAssertions;
using IntTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntTest.Scenarios.Projects;

public class ProjectLifecycleTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
  private readonly TestWebApplicationFactory _factory;
  private readonly HttpClient _client;
  private readonly MainDbContext _dbContext;

  public ProjectLifecycleTests(TestWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
    _dbContext = factory.Services.GetRequiredService<MainDbContext>();
  }

  public async Task InitializeAsync() => await Task.CompletedTask;

  public async Task DisposeAsync()
  {
    await _dbContext.Database.EnsureDeletedAsync();
    await _dbContext.Database.EnsureCreatedAsync();
  }

  [Fact]
  public async Task CompleteProjectLifecycle_CreateUpdateDeleteFlow_ShouldSucceed()
  {
    // Step 1: Create project
    var createRequest = new CreateProjectReq
    {
      Name = "Alpha Project",
      Open = true
    };
    var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", createRequest);
    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectRes>();
    createdProject!.Name.Should().Be("Alpha Project");
    createdProject.Open.Should().BeTrue();
    createdProject.Id.Should().NotBeEmpty();

    // Step 2: Retrieve the created project
    var getResponse = await _client.GetAsync($"/api/v1/projects/{createdProject.Id}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var retrievedProject = await getResponse.Content.ReadFromJsonAsync<ProjectRes>();
    retrievedProject.Should().BeEquivalentTo(createdProject);

    // Step 3: Update the project
    var updateRequest = new UpdateProjectReq
    {
      Name = "Alpha Project Updated",
      Open = false
    };
    var updateResponse = await _client.PutAsJsonAsync(
      $"/api/v1/projects/{createdProject.Id}",
      updateRequest);
    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var updatedProject = await updateResponse.Content.ReadFromJsonAsync<ProjectRes>();
    updatedProject!.Name.Should().Be("Alpha Project Updated");
    updatedProject.Open.Should().BeFalse();

    // Step 4: Verify the update persisted
    var verifyResponse = await _client.GetAsync($"/api/v1/projects/{createdProject.Id}");
    var verifiedProject = await verifyResponse.Content.ReadFromJsonAsync<ProjectRes>();
    verifiedProject!.Name.Should().Be("Alpha Project Updated");
    verifiedProject.Open.Should().BeFalse();

    // Step 5: Delete the project
    var deleteResponse = await _client.DeleteAsync($"/api/v1/projects/{createdProject.Id}");
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Step 6: Verify project is deleted
    var checkResponse = await _client.GetAsync($"/api/v1/projects/{createdProject.Id}");
    checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}
```

## Multi-Step Scenario Example

Testing user registration and profile update flow:

```csharp
[Fact]
public async Task UserRegistrationAndProfileUpdate_CompleteFlow_ShouldSucceed()
{
  // Step 1: Register new user
  var registerRequest = new RegisterUserReq
  {
    Username = "john_doe",
    Email = "john@example.com",
    Password = "SecurePassword123!"
  };
  var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
  registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
  var user = await registerResponse.Content.ReadFromJsonAsync<UserRes>();
  user!.Username.Should().Be("john_doe");
  user.Email.Should().Be("john@example.com");

  // Step 2: Verify email (simulate email verification)
  var verifyResponse = await _client.PostAsync(
    $"/api/v1/auth/verify-email?token={user.VerificationToken}",
    null);
  verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

  // Step 3: Login
  var loginRequest = new LoginReq
  {
    Email = "john@example.com",
    Password = "SecurePassword123!"
  };
  var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
  loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
  var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginRes>();
  loginResult!.Token.Should().NotBeNullOrEmpty();

  // Step 4: Update profile with auth token
  _client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", loginResult.Token);
  var updateRequest = new UpdateProfileReq
  {
    FirstName = "John",
    LastName = "Doe",
    PhoneNumber = "+1234567890"
  };
  var updateResponse = await _client.PutAsJsonAsync($"/api/v1/users/{user.Id}", updateRequest);
  updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

  // Step 5: Verify profile was updated
  var profileResponse = await _client.GetAsync($"/api/v1/users/{user.Id}");
  var profile = await profileResponse.Content.ReadFromJsonAsync<UserRes>();
  profile!.FirstName.Should().Be("John");
  profile.LastName.Should().Be("Doe");
}
```

## Validation Testing

Testing validation errors with Theory:

```csharp
[Theory]
[InlineData("", "Name is required")]
[InlineData("ab", "Name must be at least 3 characters")]
[InlineData("this-is-a-very-long-name-that-exceeds-the-maximum-allowed-length-for-project-names-in-the-system", "Name must not exceed 50 characters")]
[InlineData("Invalid@Name!", "Name contains invalid characters")]
public async Task CreateProject_WithInvalidName_ShouldReturnValidationError(
  string invalidName,
  string expectedErrorMessage)
{
  var request = new CreateProjectReq
  {
    Name = invalidName,
    Open = true
  };

  var response = await _client.PostAsJsonAsync("/api/v1/projects", request);

  response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
  problem!.Status.Should().Be(400);
  problem.Errors.Should().ContainKey("Name");
  problem.Errors["Name"].Should().Contain(expectedErrorMessage);
}

[Theory]
[InlineData("invalid-email", "Email must be a valid email address")]
[InlineData("", "Email is required")]
[InlineData("missing@domain", "Email must have a valid domain")]
public async Task RegisterUser_WithInvalidEmail_ShouldReturnValidationError(
  string invalidEmail,
  string expectedErrorMessage)
{
  var request = new RegisterUserReq
  {
    Username = "testuser",
    Email = invalidEmail,
    Password = "Password123!"
  };

  var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

  response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
  problem!.Errors.Should().ContainKey("Email");
}
```

## Business Rule Violations

Testing uniqueness constraints and conflicts:

```csharp
[Fact]
public async Task CreateProject_WithDuplicateName_ShouldReturnConflict()
{
  // Step 1: Create first project
  var request1 = new CreateProjectReq
  {
    Name = "Unique Project",
    Open = true
  };
  var response1 = await _client.PostAsJsonAsync("/api/v1/projects", request1);
  response1.StatusCode.Should().Be(HttpStatusCode.Created);

  // Step 2: Try to create another project with the same name
  var request2 = new CreateProjectReq
  {
    Name = "Unique Project",
    Open = false
  };
  var response2 = await _client.PostAsJsonAsync("/api/v1/projects", request2);

  response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
  var problem = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
  problem!.Title.Should().Contain("already exists");
}

[Fact]
public async Task Subscribe_WithExistingEmail_ShouldReturnConflict()
{
  // Step 1: Create first subscription
  var request1 = new CreateSubscriptionReq
  {
    Email = "subscriber@example.com",
    Type = "newsletter",
    LegalBasis = LegalBasis.Consent
  };
  var response1 = await _client.PostAsJsonAsync("/api/v1/subscriptions", request1);
  response1.StatusCode.Should().Be(HttpStatusCode.Created);

  // Step 2: Try to subscribe with the same email and type
  var request2 = new CreateSubscriptionReq
  {
    Email = "subscriber@example.com",
    Type = "newsletter",
    LegalBasis = LegalBasis.Consent
  };
  var response2 = await _client.PostAsJsonAsync("/api/v1/subscriptions", request2);

  response2.StatusCode.Should().Be(HttpStatusCode.Conflict);
  var problem = await response2.Content.ReadFromJsonAsync<ProblemDetails>();
  problem!.Title.Should().Contain("already subscribed");
}
```

## Edge Cases

Testing boundary conditions and edge cases:

```csharp
[Fact]
public async Task GetProject_WithNonExistentId_ShouldReturn404()
{
  var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
  var response = await _client.GetAsync($"/api/v1/projects/{nonExistentId}");

  response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

[Fact]
public async Task DeleteProject_ThatDoesNotExist_ShouldReturn404()
{
  var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
  var response = await _client.DeleteAsync($"/api/v1/projects/{nonExistentId}");

  response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}

[Fact]
public async Task SearchProjects_WithNoResults_ShouldReturnEmptyList()
{
  var response = await _client.GetAsync(
    "/api/v1/projects?name=NonExistentProject&limit=100&skip=0");

  response.StatusCode.Should().Be(HttpStatusCode.OK);
  var results = await response.Content.ReadFromJsonAsync<List<ProjectRes>>();
  results.Should().BeEmpty();
}

[Fact]
public async Task UpdateProject_WithNonExistentId_ShouldReturn404()
{
  var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
  var request = new UpdateProjectReq
  {
    Name = "Updated Name",
    Open = true
  };

  var response = await _client.PutAsJsonAsync($"/api/v1/projects/{nonExistentId}", request);

  response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

## Search and Pagination

Testing search functionality with filters and pagination:

```csharp
[Fact]
public async Task SearchProjects_WithMultipleResults_ShouldReturnFilteredAndPaginated()
{
  // Step 1: Create multiple projects
  var projects = new[]
  {
    new CreateProjectReq { Name = "Alpha Project", Open = true },
    new CreateProjectReq { Name = "Beta Project", Open = false },
    new CreateProjectReq { Name = "Gamma Project", Open = true },
    new CreateProjectReq { Name = "Delta Project", Open = true }
  };

  foreach (var project in projects)
  {
    var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", project);
    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
  }

  // Step 2: Search with filter (open projects only)
  var searchResponse = await _client.GetAsync(
    "/api/v1/projects?open=true&limit=100&skip=0");
  searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
  var openProjects = await searchResponse.Content.ReadFromJsonAsync<List<ProjectRes>>();
  openProjects.Should().HaveCount(3);
  openProjects.Should().OnlyContain(p => p.Open == true);

  // Step 3: Search with name filter
  var nameSearchResponse = await _client.GetAsync(
    "/api/v1/projects?name=Alpha&limit=100&skip=0");
  var namedProjects = await nameSearchResponse.Content.ReadFromJsonAsync<List<ProjectRes>>();
  namedProjects.Should().HaveCount(1);
  namedProjects![0].Name.Should().Contain("Alpha");

  // Step 4: Test pagination
  var page1Response = await _client.GetAsync(
    "/api/v1/projects?limit=2&skip=0");
  var page1 = await page1Response.Content.ReadFromJsonAsync<List<ProjectRes>>();
  page1.Should().HaveCount(2);

  var page2Response = await _client.GetAsync(
    "/api/v1/projects?limit=2&skip=2");
  var page2 = await page2Response.Content.ReadFromJsonAsync<List<ProjectRes>>();
  page2.Should().HaveCount(2);

  // Verify no duplicates between pages
  page1.Should().NotIntersectWith(page2, (p1, p2) => p1.Id == p2.Id);
}
```

## Error Handling

Testing error responses and problem details:

```csharp
[Fact]
public async Task CreateProject_WithServerError_ShouldReturn500()
{
  // Simulate scenario that causes server error (e.g., database connection issue)
  // Note: This would require special setup or conditional logic in test environment

  var request = new CreateProjectReq
  {
    Name = "Test Project",
    Open = true
  };

  var response = await _client.PostAsJsonAsync("/api/v1/projects", request);

  if (response.StatusCode == HttpStatusCode.InternalServerError)
  {
    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problem!.Status.Should().Be(500);
    problem.Title.Should().NotBeNullOrEmpty();
  }
}

[Fact]
public async Task GetProject_WithInvalidGuid_ShouldReturn400()
{
  var response = await _client.GetAsync("/api/v1/projects/invalid-guid-format");

  response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
}
```

## Test Class Structure

Complete test class template with proper setup and cleanup:

```csharp
using System.Net;
using System.Net.Http.Json;
using App.Modules.Projects.API.V1;
using App.StartUp.Database;
using FluentAssertions;
using IntTest.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntTest.Scenarios.Projects;

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
    // Optional: Seed common test data here
    await Task.CompletedTask;
  }

  public async Task DisposeAsync()
  {
    // Clean up database after each test
    await _dbContext.Database.EnsureDeletedAsync();
    await _dbContext.Database.EnsureCreatedAsync();
  }

  // ========================================
  // Create Project Scenarios
  // ========================================

  [Fact]
  public async Task CreateProject_WithValidData_ShouldReturnCreated()
  {
    var request = new CreateProjectReq
    {
      Name = "Test Project",
      Open = true
    };

    var response = await _client.PostAsJsonAsync("/api/v1/projects", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var project = await response.Content.ReadFromJsonAsync<ProjectRes>();
    project!.Name.Should().Be("Test Project");
    project.Open.Should().BeTrue();
    project.Id.Should().NotBeEmpty();
  }

  // ========================================
  // Get Project Scenarios
  // ========================================

  [Fact]
  public async Task GetProject_AfterCreation_ShouldReturnProject()
  {
    // Arrange: Create project first
    var createRequest = new CreateProjectReq { Name = "Test", Open = true };
    var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", createRequest);
    var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectRes>();

    // Act: Get the project
    var response = await _client.GetAsync($"/api/v1/projects/{createdProject!.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var project = await response.Content.ReadFromJsonAsync<ProjectRes>();
    project.Should().BeEquivalentTo(createdProject);
  }

  // ========================================
  // Update Project Scenarios
  // ========================================

  [Fact]
  public async Task UpdateProject_WithValidData_ShouldPersistChanges()
  {
    // Arrange: Create project first
    var createRequest = new CreateProjectReq { Name = "Original", Open = true };
    var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", createRequest);
    var project = await createResponse.Content.ReadFromJsonAsync<ProjectRes>();

    // Act: Update the project
    var updateRequest = new UpdateProjectReq { Name = "Updated", Open = false };
    var updateResponse = await _client.PutAsJsonAsync(
      $"/api/v1/projects/{project!.Id}",
      updateRequest);

    // Assert: Update succeeded
    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    // Verify changes persisted
    var getResponse = await _client.GetAsync($"/api/v1/projects/{project.Id}");
    var updated = await getResponse.Content.ReadFromJsonAsync<ProjectRes>();
    updated!.Name.Should().Be("Updated");
    updated.Open.Should().BeFalse();
  }

  // ========================================
  // Delete Project Scenarios
  // ========================================

  [Fact]
  public async Task DeleteProject_WithExistingProject_ShouldRemoveFromDatabase()
  {
    // Arrange: Create project first
    var createRequest = new CreateProjectReq { Name = "ToDelete", Open = true };
    var createResponse = await _client.PostAsJsonAsync("/api/v1/projects", createRequest);
    var project = await createResponse.Content.ReadFromJsonAsync<ProjectRes>();

    // Act: Delete the project
    var deleteResponse = await _client.DeleteAsync($"/api/v1/projects/{project!.Id}");

    // Assert: Delete succeeded
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // Verify project is gone
    var getResponse = await _client.GetAsync($"/api/v1/projects/{project.Id}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }
}
```

## Quick Templates

### Single Endpoint Test

```csharp
[Fact]
public async Task Create{Entity}_WithValidData_ShouldReturnCreated()
{
  var request = new Create{Entity}Req { /* properties */ };
  var response = await _client.PostAsJsonAsync("/api/v1/{endpoint}", request);

  response.StatusCode.Should().Be(HttpStatusCode.Created);
  var result = await response.Content.ReadFromJsonAsync<{Entity}Res>();
  result!.Property.Should().Be("ExpectedValue");
}
```

### Multi-Step Flow Test

```csharp
[Fact]
public async Task {Scenario}Flow_Complete_ShouldSucceed()
{
  // Step 1: First action
  var step1Response = await _client.PostAsJsonAsync("/api/v1/endpoint1", request1);
  step1Response.StatusCode.Should().Be(HttpStatusCode.Created);
  var step1Result = await step1Response.Content.ReadFromJsonAsync<Response1>();

  // Step 2: Second action using result from step 1
  var step2Response = await _client.GetAsync($"/api/v1/endpoint2/{step1Result!.Id}");
  step2Response.StatusCode.Should().Be(HttpStatusCode.OK);

  // Step 3: Final verification
  var verifyResponse = await _client.GetAsync($"/api/v1/verify/{step1Result.Id}");
  var finalResult = await verifyResponse.Content.ReadFromJsonAsync<FinalResponse>();
  finalResult!.Status.Should().Be("Expected");
}
```

### Validation Error Test

```csharp
[Theory]
[InlineData(invalidValue1, expectedError1)]
[InlineData(invalidValue2, expectedError2)]
[InlineData(invalidValue3, expectedError3)]
public async Task Create{Entity}_WithInvalidData_ShouldReturnValidationError(
  string invalidValue,
  string expectedError)
{
  var request = new Create{Entity}Req { Field = invalidValue };
  var response = await _client.PostAsJsonAsync("/api/v1/{endpoint}", request);

  response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
  problem!.Errors.Should().ContainKey("Field");
}
```

### Edge Case Test

```csharp
[Fact]
public async Task Get{Entity}_WithNonExistentId_ShouldReturn404()
{
  var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
  var response = await _client.GetAsync($"/api/v1/{endpoint}/{nonExistentId}");

  response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```
