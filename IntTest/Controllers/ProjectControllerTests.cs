using System.Net;
using System.Net.Http.Json;
using App.Modules.Projects.API.V1;
using App.StartUp.Database;
using FluentAssertions;
using IntTest.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace IntTest.Controllers;

public class ProjectControllerTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client = factory.CreateClient();
  private readonly TestWebApplicationFactory _factory = factory;

  public async Task InitializeAsync()
  {
    // Clear database before each test
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
    dbContext.Projects.RemoveRange(dbContext.Projects);
    await dbContext.SaveChangesAsync();
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  // ========================================
  // Search Tests
  // ========================================

  [Fact]
  public async Task Search_WithNoFilters_ShouldReturnEmptyList()
  {
    // Arrange & Act
    var response = await _client.GetAsync("/api/v1/Project");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var projects = await response.Content.ReadFromJsonAsync<List<ProjectPrincipalRes>>();
    projects.Should().NotBeNull();
    projects.Should().BeEmpty();
  }

  [Fact]
  public async Task Search_AfterCreatingProject_ShouldReturnProject()
  {
    // Arrange
    var createReq = new CreateProjectReq("Test Project", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    createResponse.EnsureSuccessStatusCode();

    // Act
    var response = await _client.GetAsync("/api/v1/Project");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var projects = await response.Content.ReadFromJsonAsync<List<ProjectPrincipalRes>>();
    projects.Should().NotBeNull();
    projects.Should().HaveCount(1);
    projects![0].Name.Should().Be("Test Project");
    projects[0].Open.Should().BeTrue();
  }

  [Fact]
  public async Task Search_WithNameFilter_ShouldReturnMatchingProjects()
  {
    // Arrange
    await _client.PostAsJsonAsync("/api/v1/Project", new CreateProjectReq("Alpha Project", true));
    await _client.PostAsJsonAsync("/api/v1/Project", new CreateProjectReq("Beta Project", false));
    await _client.PostAsJsonAsync("/api/v1/Project", new CreateProjectReq("Alpha Two", true));

    // Act
    var response = await _client.GetAsync("/api/v1/Project?Name=Alpha");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var projects = await response.Content.ReadFromJsonAsync<List<ProjectPrincipalRes>>();
    projects.Should().NotBeNull();
    projects.Should().HaveCount(2);
    projects!.Should().AllSatisfy(p => p.Name.Should().Contain("Alpha"));
  }

  [Fact]
  public async Task Search_WithLimitAndSkip_ShouldPaginate()
  {
    // Arrange
    for (int i = 1; i <= 5; i++)
    {
      await _client.PostAsJsonAsync("/api/v1/Project", new CreateProjectReq($"Project {i}", true));
    }

    // Act
    var response = await _client.GetAsync("/api/v1/Project?Limit=2&Skip=2");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var projects = await response.Content.ReadFromJsonAsync<List<ProjectPrincipalRes>>();
    projects.Should().NotBeNull();
    projects.Should().HaveCount(2);
  }

  // ========================================
  // Get By ID Tests
  // ========================================

  [Fact]
  public async Task Get_WithValidId_ShouldReturnProject()
  {
    // Arrange
    var createReq = new CreateProjectReq("Get Test Project", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Act
    var response = await _client.GetAsync($"/api/v1/Project/{created!.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var project = await response.Content.ReadFromJsonAsync<ProjectRes>();
    project.Should().NotBeNull();
    project!.Principal.Id.Should().Be(created.Id);
    project.Principal.Name.Should().Be("Get Test Project");
    project.Principal.Open.Should().BeTrue();
  }

  [Fact]
  public async Task Get_WithNonExistentId_ShouldReturn404()
  {
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync($"/api/v1/Project/{nonExistentId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Should().NotBeNull();
    problemDetails!.Title.Should().Contain("Not Found");
  }

  [Fact]
  public async Task Get_WithInvalidId_ShouldReturn404()
  {
    // Arrange & Act
    var response = await _client.GetAsync("/api/v1/Project/invalid-guid");

    // Assert
    // ASP.NET returns 404 for invalid route parameters that can't be bound
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  // ========================================
  // Create Tests
  // ========================================

  [Fact]
  public async Task Create_WithValidData_ShouldReturn200AndProject()
  {
    // Arrange
    var createReq = new CreateProjectReq("New Project", true);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/Project", createReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await response.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    created.Should().NotBeNull();
    created!.Id.Should().NotBeEmpty();
    created.Name.Should().Be("New Project");
    created.Open.Should().BeTrue();
  }

  [Fact]
  public async Task Create_WithEmptyName_ShouldReturn400()
  {
    // Arrange
    var createReq = new CreateProjectReq("", true);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/Project", createReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Should().NotBeNull();
  }

  [Fact]
  public async Task Create_WithClosedProject_ShouldSucceed()
  {
    // Arrange
    var createReq = new CreateProjectReq("Closed Project", false);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/Project", createReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await response.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    created.Should().NotBeNull();
    created!.Open.Should().BeFalse();
  }

  [Theory]
  [InlineData("Project Alpha")]
  [InlineData("Project Beta")]
  [InlineData("Project Gamma")]
  public async Task Create_WithDifferentNames_ShouldSucceed(string projectName)
  {
    // Arrange
    var createReq = new CreateProjectReq(projectName, true);

    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/Project", createReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await response.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    created.Should().NotBeNull();
    created!.Name.Should().Be(projectName);
  }

  // ========================================
  // Update Tests
  // ========================================

  [Fact]
  public async Task Update_WithValidData_ShouldReturn200AndUpdatedProject()
  {
    // Arrange
    var createReq = new CreateProjectReq("Original Name", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    var updateReq = new UpdateProjectReq("Updated Name", false);

    // Act
    var response = await _client.PutAsJsonAsync($"/api/v1/Project/{created!.Id}", updateReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var updated = await response.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    updated.Should().NotBeNull();
    updated!.Id.Should().Be(created.Id);
    updated.Name.Should().Be("Updated Name");
    updated.Open.Should().BeFalse();
  }

  [Fact]
  public async Task Update_WithNonExistentId_ShouldReturn404()
  {
    // Arrange
    var nonExistentId = Guid.NewGuid();
    var updateReq = new UpdateProjectReq("Updated Name", true);

    // Act
    var response = await _client.PutAsJsonAsync($"/api/v1/Project/{nonExistentId}", updateReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Should().NotBeNull();
  }

  [Fact]
  public async Task Update_WithEmptyName_ShouldReturn400()
  {
    // Arrange
    var createReq = new CreateProjectReq("Original Name", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    var updateReq = new UpdateProjectReq("", true);

    // Act
    var response = await _client.PutAsJsonAsync($"/api/v1/Project/{created!.Id}", updateReq);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task Update_ToggleOpenStatus_ShouldSucceed()
  {
    // Arrange
    var createReq = new CreateProjectReq("Toggle Project", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Act - Close the project
    var updateReq1 = new UpdateProjectReq("Toggle Project", false);
    var response1 = await _client.PutAsJsonAsync($"/api/v1/Project/{created!.Id}", updateReq1);
    var updated1 = await response1.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Act - Reopen the project
    var updateReq2 = new UpdateProjectReq("Toggle Project", true);
    var response2 = await _client.PutAsJsonAsync($"/api/v1/Project/{created.Id}", updateReq2);
    var updated2 = await response2.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Assert
    updated1!.Open.Should().BeFalse();
    updated2!.Open.Should().BeTrue();
  }

  // ========================================
  // Delete Tests
  // ========================================

  [Fact]
  public async Task Delete_WithValidId_ShouldReturn204()
  {
    // Arrange
    var createReq = new CreateProjectReq("To Delete", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Act
    var response = await _client.DeleteAsync($"/api/v1/Project/{created!.Id}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task Delete_WithNonExistentId_ShouldReturn404()
  {
    // Arrange
    var nonExistentId = Guid.NewGuid();

    // Act
    var response = await _client.DeleteAsync($"/api/v1/Project/{nonExistentId}");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    problemDetails.Should().NotBeNull();
  }

  [Fact]
  public async Task Delete_ThenGet_ShouldReturn404()
  {
    // Arrange
    var createReq = new CreateProjectReq("To Delete Then Get", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();

    // Act - Delete
    await _client.DeleteAsync($"/api/v1/Project/{created!.Id}");

    // Act - Try to get deleted project
    var getResponse = await _client.GetAsync($"/api/v1/Project/{created.Id}");

    // Assert
    getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  // ========================================
  // End-to-End Workflow Tests
  // ========================================

  [Fact]
  public async Task FullWorkflow_CreateUpdateDeleteProject_ShouldSucceed()
  {
    // 1. Create
    var createReq = new CreateProjectReq("Workflow Project", true);
    var createResponse = await _client.PostAsJsonAsync("/api/v1/Project", createReq);
    createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var created = await createResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    created!.Name.Should().Be("Workflow Project");

    // 2. Get
    var getResponse = await _client.GetAsync($"/api/v1/Project/{created.Id}");
    getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var retrieved = await getResponse.Content.ReadFromJsonAsync<ProjectRes>();
    retrieved!.Principal.Id.Should().Be(created.Id);

    // 3. Update
    var updateReq = new UpdateProjectReq("Updated Workflow Project", false);
    var updateResponse = await _client.PutAsJsonAsync($"/api/v1/Project/{created.Id}", updateReq);
    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectPrincipalRes>();
    updated!.Name.Should().Be("Updated Workflow Project");
    updated.Open.Should().BeFalse();

    // 4. Delete
    var deleteResponse = await _client.DeleteAsync($"/api/v1/Project/{created.Id}");
    deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

    // 5. Verify deletion
    var finalGetResponse = await _client.GetAsync($"/api/v1/Project/{created.Id}");
    finalGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Concurrency_CreateMultipleProjects_ShouldAllSucceed()
  {
    // Arrange
    var tasks = Enumerable.Range(1, 10)
      .Select(i => _client.PostAsJsonAsync("/api/v1/Project", new CreateProjectReq($"Project {i}", true)))
      .ToList();

    // Act
    var responses = await Task.WhenAll(tasks);

    // Assert
    responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

    // Verify all created
    var searchResponse = await _client.GetAsync("/api/v1/Project");
    var projects = await searchResponse.Content.ReadFromJsonAsync<List<ProjectPrincipalRes>>();
    projects.Should().HaveCount(10);
  }
}
