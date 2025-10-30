# Testing Examples

Code examples, templates, and patterns for writing tests in the Zinc project.

## Table of Contents

- [AAA Pattern Example](#aaa-pattern-example)
- [Theory-Based Testing](#theory-based-testing)
- [Test Data Rules](#test-data-rules)
- [Collection Expressions](#collection-expressions)
- [Mocking Strategies](#mocking-strategies)
- [Result Monad Testing](#result-monad-testing)
- [Namespace Conflicts](#namespace-conflicts)
- [Required Properties](#required-properties)
- [Complete Real-World Example](#complete-real-world-example)
- [Quick Templates](#quick-templates)

## AAA Pattern Example

```csharp
[Theory]
[MemberData(nameof(ValidTestData))]
public async Task Create_WithValidInput_ShouldReturnSuccess(
  ProjectRecord input,
  ProjectPrincipal mockRepoResult,
  ProjectPrincipal expectedOutput)
{
  // Arrange - setup mocks, construct objects, prepare input/expected
  var mockRepo = new Mock<IProjectRepository>();
  var mockTm = new Mock<ITransactionManager>();
  mockRepo.Setup(x => x.Create(It.IsAny<ProjectRecord>()))
    .ReturnsAsync(new Result<ProjectPrincipal>(mockRepoResult));
  mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal>>>>()))
    .Returns<Func<Task<Result<ProjectPrincipal>>>>(func => func());
  var service = new ProjectService(mockRepo.Object, mockTm.Object);

  // Act - execute the method under test
  var actual = await service.Create(input);

  // Assert - compare actual with expected using FluentAssertions
  actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedOutput));
  mockRepo.Verify(x => x.Create(It.IsAny<ProjectRecord>()), Times.Once);
}
```

## Theory-Based Testing

### Primitive Types with InlineData

```csharp
[Theory]
[InlineData("id1")]
[InlineData("id2")]
[InlineData("id3")]
public async Task GetById_WithExistingId_ShouldReturnUser(string id)
{
  // Test implementation
}
```

### Complex Types with MemberData

```csharp
[Theory]
[MemberData(nameof(ValidSearchTestData))]
public async Task Search_WithValidInput_ShouldReturnResults(
  UserSearch input,
  IEnumerable<UserPrincipal> mockRepoResult,
  IEnumerable<UserPrincipal> expectedOutput)
{
  // Test implementation
}

public static IEnumerable<object[]> ValidSearchTestData()
{
  yield return new object[]
  {
    new UserSearch { Username = "alice", Limit = 100, Skip = 0 },
    new List<UserPrincipal>
    {
      new()
      {
        Id = "ba9da659-2021-4945-812e-dacb30c4c2d2",
        Record = new UserRecord
        {
          Username = "alice",
          Email = "alice@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write"]
        }
      }
    },
    new List<UserPrincipal>
    {
      new()
      {
        Id = "ba9da659-2021-4945-812e-dacb30c4c2d2",
        Record = new UserRecord
        {
          Username = "alice",
          Email = "alice@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write"]
        }
      }
    }
  };

  yield return new object[]
  {
    new UserSearch { Email = "bob@example.com", Limit = 50, Skip = 0 },
    new List<UserPrincipal>
    {
      new()
      {
        Id = "097ad12b-c11c-4936-8cb2-d41ffc947687",
        Record = new UserRecord
        {
          Username = "bob",
          Email = "bob@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["read"]
        }
      }
    },
    new List<UserPrincipal>
    {
      new()
      {
        Id = "097ad12b-c11c-4936-8cb2-d41ffc947687",
        Record = new UserRecord
        {
          Username = "bob",
          Email = "bob@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["read"]
        }
      }
    }
  };

  yield return new object[]
  {
    new UserSearch { Limit = 20, Skip = 0 },
    new List<UserPrincipal> { /* more test data */ },
    new List<UserPrincipal> { /* expected data */ }
  };
}
```

## Test Data Rules

### ❌ WRONG - Dynamic/Computed Values

```csharp
// ❌ BAD - Different GUID each time
var userId = Guid.NewGuid();

// ❌ BAD - Different time each run
CreatedAt = DateTime.Now

// ❌ BAD - Computed value
EffectiveDate = DateTime.UtcNow.AddDays(7)

// ❌ BAD - Non-deterministic
RandomValue = new Random().Next()
```

### ✅ CORRECT - Static, Deterministic Values

```csharp
// ✅ GOOD - Fixed GUID
var userId = Guid.Parse("ba9da659-2021-4945-812e-dacb30c4c2d2");

// ✅ GOOD - Fixed timestamp
CreatedAt = new DateTime(2025, 1, 15, 10, 0, 0)

// ✅ GOOD - Fixed date
EffectiveDate = new DateTime(2026, 1, 1)

// ✅ GOOD - Fixed offset date
Time = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero)

// ✅ GOOD - Fixed values
var docId1 = Guid.Parse("d9c89343-2ef1-4c9e-bad3-c4cc68a52c16");
var docId2 = Guid.Parse("c8afb703-2a09-4e5a-885b-9f75eee35835");
```

## Collection Expressions

### Empty Collections

```csharp
// ✅ CORRECT - C# 12 collection expression
List<ProjectPrincipal> mockRepoResult = [];
List<UserPrincipal> expectedOutput = [];
```

### Array Collection Expressions

```csharp
// ✅ CORRECT - C# 12 collection expression for simple arrays
Scopes = ["read", "write"]
Tags = ["active", "verified"]
```

### Complex Nested Objects in MemberData

```csharp
public static IEnumerable<object[]> ValidSearchTestData()
{
  yield return new object[]
  {
    new UserSearch { Username = "alice", Limit = 100, Skip = 0 },
    // ✅ Use traditional syntax for complex nested objects in test data
    new List<UserPrincipal>
    {
      new()
      {
        Id = "ba9da659-2021-4945-812e-dacb30c4c2d2",
        Record = new UserRecord
        {
          Username = "alice",
          Email = "alice@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write"]  // ✅ But simple arrays can use []
        }
      }
    },
    new List<UserPrincipal>
    {
      new()
      {
        Id = "ba9da659-2021-4945-812e-dacb30c4c2d2",
        Record = new UserRecord
        {
          Username = "alice",
          Email = "alice@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write"]
        }
      }
    }
  };
}
```

### Quick Reference

```csharp
// ✅ Empty collections with explicit type
List<T> items = [];

// ✅ Simple arrays
string[] scopes = ["read", "write"];

// ✅ Complex objects in MemberData - Must use traditional syntax
new List<UserPrincipal>
{
  new() { Id = "1", Record = new UserRecord { Scopes = ["read"] } }
}

// ❌ WRONG - Collection expressions don't compile in object[]
// Compiler error CS9174: Cannot initialize type 'object' with a collection expression
// [
//   new() { Id = "1", Record = new UserRecord { Scopes = ["read"] } }
// ]
```

### Why Collection Expressions Don't Work in `object[]`

Collection expressions require a concrete target type. Inside `new object[]`, the target type is `object`, which is not constructible as a collection:

```csharp
// ❌ Won't compile
yield return new object[]
{
  [ new UserPrincipal { ... } ]  // Error: Cannot initialize type 'object' with collection expression
};

// ✅ Works - explicit type
yield return new object[]
{
  new List<UserPrincipal> { new() { ... } }  // List<T> is assignable to object
};
```

## Mocking Strategies

### Basic Repository Mock

```csharp
var mockRepo = new Mock<IProjectRepository>();
mockRepo.Setup(x => x.Get(It.IsAny<Guid>()))
  .ReturnsAsync(new Result<Project?>(mockRepoResult));
```

### Transaction Manager Mock (for Create/Update/Delete)

```csharp
var mockTm = new Mock<ITransactionManager>();
mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal>>>>()))
  .Returns<Func<Task<Result<ProjectPrincipal>>>>(func => func());
```

### Verify Interactions

```csharp
mockRepo.Verify(x => x.Create(It.IsAny<ProjectRecord>()), Times.Once);
mockTm.Verify(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal>>>>()), Times.Once);
```

## Result Monad Testing

### Success Path

```csharp
actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedOutput));
```

### Failure Path

```csharp
actual.Should().BeFailure();
actual.FailureOrDefault().Should().BeOfType<ValidationError>();
```

## Namespace Conflicts

When testing types with the same name as their namespace (e.g., `User` type in `Domain.User` namespace), use `global::` prefix:

```csharp
// ✅ CORRECT - Use global:: to avoid ambiguity
global::Domain.User.User? mockRepoResult = null;
global::Domain.User.User? expectedOutput = null;

new Result<global::Domain.User.User?>(mockRepoResult)
```

## Required Properties

### ✅ CORRECT - All Required Properties Initialized

```csharp
new SubscriptionEvent
{
  Type = "newsletter",
  LegalBasis = LegalBasis.Consent,
  Reason = "User opted in",
  Open = false,
  Timezone = "UTC",
  Time = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero)  // Don't forget Time!
}
```

### ❌ WRONG - Missing Required Properties

```csharp
new SubscriptionEvent
{
  Type = "newsletter",
  LegalBasis = LegalBasis.Consent,
  Reason = "User opted in",
  Open = false,
  Timezone = "UTC"
  // Missing Time property - will cause compilation error
}
```

## Complete Real-World Example

From `UnitTest/Domain/Projects/ServiceTests.cs`:

```csharp
using CarboxylicLithium;
using Domain;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace UnitTest.Domain.Projects;

public class ServiceTests
{
  // ========================================
  // Search Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(
    ProjectSearch input,
    IEnumerable<ProjectPrincipal> mockRepoResult,
    IEnumerable<ProjectPrincipal> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Search(It.IsAny<ProjectSearch>()))
      .ReturnsAsync(new Result<IEnumerable<ProjectPrincipal>>(mockRepoResult));
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<ProjectPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<ProjectSearch>()), Times.Once);
  }

  [Theory]
  [InlineData("49f8955c-f953-46f2-bb95-e6ce021d2815")]
  [InlineData("ff604367-53e6-4022-b56a-2f86e5bf53de")]
  [InlineData("10c218a0-564e-4aac-80dd-3d3b14ac9e75")]
  public async Task Search_WithNoResults_ShouldReturnEmptyList(string searchIdStr)
  {
    // Arrange
    var searchId = Guid.Parse(searchIdStr);
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    List<ProjectPrincipal> mockRepoResult = [];
    List<ProjectPrincipal> expectedOutput = [];
    mockRepo.Setup(x => x.Search(It.IsAny<ProjectSearch>()))
      .ReturnsAsync(new Result<IEnumerable<ProjectPrincipal>>(mockRepoResult));
    var service = new ProjectService(mockRepo.Object, mockTm.Object);
    var input = new ProjectSearch { Id = searchId, Limit = 100, Skip = 0 };

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<ProjectPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<ProjectSearch>()), Times.Once);
  }

  public static IEnumerable<object[]> ValidSearchTestData()
  {
    yield return new object[]
    {
      new ProjectSearch { Name = "Alpha", Limit = 100, Skip = 0 },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("49745ae9-a929-401e-b894-b36e336538c1"),
          Record = new ProjectRecord { Name = "Alpha Project", Open = true }
        }
      },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("49745ae9-a929-401e-b894-b36e336538c1"),
          Record = new ProjectRecord { Name = "Alpha Project", Open = true }
        }
      }
    };

    yield return new object[]
    {
      new ProjectSearch { Name = "Beta", Limit = 50, Skip = 0 },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("df21fd8e-058d-4a46-8cdc-30682014083e"),
          Record = new ProjectRecord { Name = "Beta Project", Open = false }
        },
        new()
        {
          Id = Guid.Parse("f28051f5-f677-42aa-8b80-bc881931cc1d"),
          Record = new ProjectRecord { Name = "Beta Test", Open = true }
        }
      },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("df21fd8e-058d-4a46-8cdc-30682014083e"),
          Record = new ProjectRecord { Name = "Beta Project", Open = false }
        },
        new()
        {
          Id = Guid.Parse("f28051f5-f677-42aa-8b80-bc881931cc1d"),
          Record = new ProjectRecord { Name = "Beta Test", Open = true }
        }
      }
    };

    yield return new object[]
    {
      new ProjectSearch { Limit = 20, Skip = 0 },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("ca1c4f56-e80b-420b-b24d-83e4c1ee7d29"),
          Record = new ProjectRecord { Name = "Gamma", Open = true }
        }
      },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("ca1c4f56-e80b-420b-b24d-83e4c1ee7d29"),
          Record = new ProjectRecord { Name = "Gamma", Open = true }
        }
      }
    };
  }
}
```

## Quick Templates

### InlineData Template (Primitives)

```csharp
[Theory]
[InlineData("value1")]
[InlineData("value2")]
[InlineData("value3")]
public async Task Method_Scenario_ShouldExpected(string param) { }
```

### MemberData Template (Complex)

```csharp
[Theory]
[MemberData(nameof(TestData))]
public async Task Method_Scenario_ShouldExpected(
  InputType input,
  MockResultType mockRepoResult,
  ExpectedType expectedOutput)
{
  // Arrange
  var mockRepo = new Mock<IRepository>();
  mockRepo.Setup(x => x.Method(It.IsAny<InputType>()))
    .ReturnsAsync(new Result<MockResultType>(mockRepoResult));
  var service = new Service(mockRepo.Object);

  // Act
  var actual = await service.Method(input);

  // Assert
  actual.Should().BeEquivalentTo(new Result<ExpectedType>(expectedOutput));
  mockRepo.Verify(x => x.Method(It.IsAny<InputType>()), Times.Once);
}

public static IEnumerable<object[]> TestData()
{
  yield return new object[] { input1, mockResult1, expected1 };
  yield return new object[] { input2, mockResult2, expected2 };
  yield return new object[] { input3, mockResult3, expected3 };
}
```

### Transaction Manager Template

```csharp
// For Create/Update/Delete operations that use transactions
var mockTm = new Mock<ITransactionManager>();
mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<TReturnType>>>>()))
  .Returns<Func<Task<Result<TReturnType>>>>(func => func());
var service = new Service(mockRepo.Object, mockTm.Object);
```

### Edge Case Template

```csharp
[Theory]
[InlineData("49f8955c-f953-46f2-bb95-e6ce021d2815")]
[InlineData("ff604367-53e6-4022-b56a-2f86e5bf53de")]
[InlineData("10c218a0-564e-4aac-80dd-3d3b14ac9e75")]
public async Task Search_WithNoResults_ShouldReturnEmptyList(string searchIdStr)
{
  // Arrange
  var searchId = Guid.Parse(searchIdStr);
  var mockRepo = new Mock<IProjectRepository>();
  var mockTm = new Mock<ITransactionManager>();
  List<ProjectPrincipal> mockRepoResult = [];
  List<ProjectPrincipal> expectedOutput = [];
  mockRepo.Setup(x => x.Search(It.IsAny<ProjectSearch>()))
    .ReturnsAsync(new Result<IEnumerable<ProjectPrincipal>>(mockRepoResult));
  var service = new ProjectService(mockRepo.Object, mockTm.Object);
  var input = new ProjectSearch { Id = searchId, Limit = 100, Skip = 0 };

  // Act
  var actual = await service.Search(input);

  // Assert
  actual.Should().BeEquivalentTo(new Result<IEnumerable<ProjectPrincipal>>(expectedOutput));
  mockRepo.Verify(x => x.Search(It.IsAny<ProjectSearch>()), Times.Once);
}
```
