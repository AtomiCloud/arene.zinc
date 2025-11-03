# Integration Testing References

Official documentation and resources for integration testing frameworks and tools.

## Core Testing Frameworks

### xUnit

- **Official Documentation**: https://xunit.net/
- **Getting Started**: https://xunit.net/docs/getting-started/netcore/cmdline
- **Test Fixtures**: https://xunit.net/docs/shared-context#class-fixture
- **Collection Fixtures**: https://xunit.net/docs/shared-context#collection-fixture
- **Theory and InlineData**: https://xunit.net/docs/getting-started/netcore/cmdline#write-first-theory

### FluentAssertions

- **Official Documentation**: https://fluentassertions.com/
- **Introduction**: https://fluentassertions.com/introduction
- **Collections**: https://fluentassertions.com/collections/
- **Object Graph Comparison**: https://fluentassertions.com/objectgraphs/
- **Exceptions**: https://fluentassertions.com/exceptions/

## ASP.NET Core Testing

### WebApplicationFactory

- **Microsoft Docs**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- **Integration Tests in ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#introduction-to-integration-tests
- **Custom WebApplicationFactory**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory

### HttpClient Testing

- **HttpClient in Tests**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#set-up-the-test-fixture
- **Sending HTTP Requests**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
- **JSON Extensions**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions

### Problem Details (RFC 7807)

- **RFC 7807**: https://datatracker.ietf.org/doc/html/rfc7807
- **Problem Details in ASP.NET Core**: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails
- **ValidationProblemDetails**: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.validationproblemdetails

## Entity Framework Core Testing

### In-Memory Database

- **EF Core In-Memory Database**: https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#in-memory-as-a-database-fake
- **Testing with In-Memory**: https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#inmemory-provider
- **Database Providers**: https://learn.microsoft.com/en-us/ef/core/providers/in-memory/

### Database Management in Tests

- **EnsureCreated and EnsureDeleted**: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.infrastructure.databasefacade.ensurecreated
- **Managing Database State**: https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database#handling-database-state

## HTTP Status Codes

### Standard Status Codes

- **HTTP Status Codes**: https://developer.mozilla.org/en-US/docs/Web/HTTP/Status
- **200 OK**: Success
- **201 Created**: Resource created successfully
- **204 No Content**: Success with no response body (typically DELETE)
- **400 Bad Request**: Validation error or malformed request
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Authenticated but not authorized
- **404 Not Found**: Resource does not exist
- **409 Conflict**: Resource conflict (e.g., duplicate)
- **500 Internal Server Error**: Server error

## Testing Best Practices

### Integration Testing Patterns

- **Martin Fowler - Integration Testing**: https://martinfowler.com/bliki/IntegrationTest.html
- **Test Pyramid**: https://martinfowler.com/articles/practical-test-pyramid.html
- **Given-When-Then**: https://martinfowler.com/bliki/GivenWhenThen.html

### API Testing

- **RESTful API Testing**: https://www.blazemeter.com/blog/rest-api-testing
- **API Test Automation Best Practices**: https://testautomationu.applitools.com/
- **End-to-End Testing**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Scenario-Based Testing

### Behavior-Driven Development (BDD)

- **BDD Introduction**: https://cucumber.io/docs/bdd/
- **User Stories and Scenarios**: https://cucumber.io/docs/terms/user-story/
- **Writing Good Scenarios**: https://automationpanda.com/2017/01/30/bdd-101-writing-good-gherkin/

### User Journey Testing

- **User Journey Mapping**: https://uxplanet.org/a-beginners-guide-to-user-journey-mapping-bd914f4c517c
- **Testing User Flows**: https://www.nngroup.com/articles/user-journey-mapping/

## Test Organization

### Test Structure

- **Arrange-Act-Assert**: https://automationpanda.com/2020/07/07/arrange-act-assert-a-pattern-for-writing-good-tests/
- **Test Organization**: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

## .NET Testing Tools

### System.Net.Http.Json

- **HttpClient JSON Extensions**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json
- **PostAsJsonAsync**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions.postasjsonasync
- **ReadFromJsonAsync**: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpcontentjsonextensions.readfromjsonasync

### IAsyncLifetime

- **xUnit IAsyncLifetime**: https://xunit.net/docs/shared-context#async-lifetime
- **Async Setup and Teardown**: https://andrewlock.net/exploring-the-new-project-file-program-and-the-generic-host-in-asp-net-core-3/#using-iasynclifetime-for-async-setup-and-teardown
