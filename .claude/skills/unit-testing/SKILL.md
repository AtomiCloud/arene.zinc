---
name: unit-testing
description: Write isolated unit tests for Zinc ASP.NET Core 8 API using xUnit, FluentAssertions, and Moq to test domain logic with mocked dependencies
---

# Unit Testing Skill

Use this skill when writing unit tests for the Zinc ASP.NET Core 8 API project.

## Related Documentation

- **[examples.md](examples.md)** - Code examples, templates, and real-world test cases
- **[reference.md](reference.md)** - Links to official documentation for testing frameworks

## Framework & Tools

- **xUnit** with `[Theory]` for parameterized tests
- **FluentAssertions** for readable assertions
- **Moq** for mocking dependencies
- **Result Monad Pattern** from `CarboxylicLithium`

## Unit Testing Philosophy

Unit tests focus on **isolating and testing individual functions/methods in the Domain layer**:

- **Isolation**: Mock all external dependencies (repositories, services, transaction managers)
- **Domain Focus**: Test domain services, models, and business logic
- **Fast Execution**: No database, no HTTP calls, no external systems
- **Single Responsibility**: Each test verifies one specific behavior of one method

## Core Testing Principles

### 1. Mirror Domain Structure

Test files mirror domain/module organization:

- `Domain/Projects/Service.cs` → `UnitTest/Domain/Projects/ServiceTests.cs`
- `Domain/User/Service.cs` → `UnitTest/Domain/User/ServiceTests.cs`
- **Naming Convention**: `{ClassName}Tests.cs`

### 2. AAA Pattern (Arrange-Act-Assert)

Every test follows this structure:

- **Arrange** - Setup mocks, construct objects, prepare input/expected
- **Act** - Execute the method under test
- **Assert** - Compare actual with expected using FluentAssertions

See [examples.md](examples.md#aaa-pattern-example) for complete AAA pattern example.

### 3. Theory-Based Testing

**All tests use `[Theory]` with multiple data sets** for triangulation.

**Primitive types**: Use `[InlineData]`

- 3-4 test data variations per scenario

**Complex types**: Use `[MemberData]` with static generators

- Static methods that yield `object[]` arrays
- Each variation tests a different aspect

See [examples.md](examples.md#theory-based-testing) for detailed examples.

### 4. Test Naming Convention (Behavior-Driven)

**Format**: `{MethodName}_{Scenario}_{ShouldExpectedBehavior}`

Use behavior-driven naming with "Should" to make tests read like assertions:

**Examples**:

- `Create_WithValidRecord_ShouldReturnProjectPrincipal`
- `Create_WithDuplicateName_ShouldReturnConflictError`
- `Search_WithNullName_ShouldReturnAllProjects`
- `Search_WithValidInput_ShouldReturnResults`
- `Search_WithNoMatches_ShouldReturnEmptyList`
- `Delete_WithExistingId_ShouldReturnUnit`
- `Delete_WithNonExistingId_ShouldReturnNull`

The "Should" makes the test name read like an assertion, emphasizing the expected behavior.

### 5. Test Data Triangulation

**Goal**: Test 3-4 variations per scenario to ensure general correctness.

**Scenario types**:

- Valid positive cases (happy path)
- Empty/null results
- Edge cases and boundary values
- Error conditions

### 6. Edge Case Testing

Always test:

- **Null values** (when applicable)
- **Empty collections** (`[]`, `new List<T>()`)
- **Boundary values** (min, max, zero)
- **Non-existent IDs** (should return null)

See [examples.md](examples.md#edge-case-template) for edge case test template.

## Coverage Targets

| Component       | Coverage Target | Priority |
| --------------- | --------------- | -------- |
| Domain Services | 100%            | Critical |
| Domain Models   | 100%            | Critical |
| Repositories    | 90%+            | High     |
| Validators      | 100%            | High     |

## Critical Rules for Test Data

### ❌ NEVER Use Dynamic/Computed Values

**DO NOT use these in test data:**

- `Guid.NewGuid()` - Generates different GUID each run
- `DateTime.Now` - Changes every second
- `DateTime.UtcNow.AddDays(1)` - Computed value
- `new Random().Next()` - Non-deterministic
- Any method/function that returns different values on each execution

### ✅ ALWAYS Use Static, Deterministic Values

**ALWAYS use these in test data:**

- `Guid.Parse("d9c89343-2ef1-4c9e-bad3-c4cc68a52c16")` - Fixed GUID
- `new DateTime(2025, 1, 15, 10, 0, 0)` - Fixed date
- `new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero)` - Fixed offset date
- `"fixed-string-value"` - Fixed string
- `42` - Fixed number

**Why?** Tests must be deterministic and reproducible. Using dynamic values makes tests flaky and unreliable.

See [examples.md](examples.md#test-data-rules) for correct and incorrect examples.

### Getting Real GUIDs for Tests

When you need real GUIDs, fetch them from: `https://www.uuidgenerator.net/api/version4/100`

Then use the actual GUIDs in your tests with `Guid.Parse()`.

## Modern C# 12 Collection Expressions

### Empty Collections

Use collection expressions with explicit type:

```csharp
List<ProjectPrincipal> mockRepoResult = [];
List<UserPrincipal> expectedOutput = [];
```

### Array Collection Expressions

For arrays and simple collections:

```csharp
Scopes = ["read", "write"]
Tags = ["active", "verified"]
```

### Complex Nested Objects in MemberData - Use Traditional Syntax

When creating complex nested objects in MemberData, **you must use** traditional `new List<T> { }` syntax. Collection expressions will not compile.

**Why?** Collection expressions `[...]` require a concrete target type to determine what collection to create. Inside `new object[]`, the target type is `object`, which is not a collection type. This causes compiler error CS9174:

```
error CS9174: Cannot initialize type 'object' with a collection expression
because the type is not constructible.
```

**Example - Won't Compile:**

```csharp
yield return new object[]
{
  new ProjectSearch { ... },
  [ new() { ... } ],  // ❌ ERROR: Cannot convert collection expression to object
  [ new() { ... } ]
};
```

**Example - Correct:**

```csharp
yield return new object[]
{
  new ProjectSearch { ... },
  new List<ProjectPrincipal> { new() { ... } },  // ✅ Works: explicit type
  new List<ProjectPrincipal> { new() { ... } }
};
```

Simple arrays **inside** objects can still use collection expressions:

```csharp
Record = new UserRecord { Scopes = ["read", "write"] }  // ✅ This is fine
```

See [examples.md](examples.md#collection-expressions) for detailed examples.

## Mocking Strategies

### Basic Repository Mock

Setup repository methods to return Result<T> with test data.

### Transaction Manager Mock

Required for Create/Update/Delete operations that use transactions. Mock should execute the func immediately.

### Verify Interactions

Always verify repository and transaction manager interactions with `Times.Once` or appropriate count.

See [examples.md](examples.md#mocking-strategies) for all mocking examples.

## Result Monad Testing

### Success Path

Use `actual.Should().BeEquivalentTo(new Result<T>(expected))` for success cases.

### Failure Path

- Check failure: `actual.Should().BeFailure()`
- Verify error type: `actual.FailureOrDefault().Should().BeOfType<ValidationError>()`

See [examples.md](examples.md#result-monad-testing) for detailed examples.

## Test Organization

### File Structure

Group tests by method being tested with section comments:

```csharp
namespace UnitTest.Domain.Projects;

public class ServiceTests
{
  // ========================================
  // Search Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(...) { }

  public static IEnumerable<object[]> ValidSearchTestData() { }

  // ========================================
  // Get Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(GetValidTestData))]
  public async Task Get_WithExistingId_ShouldReturnProject(...) { }

  public static IEnumerable<object[]> GetValidTestData() { }

  // ========================================
  // Create Method Tests
  // ========================================

  // ... more tests
}
```

**Organization Rules**:

- Group tests by method being tested
- Add section comments for clarity (`// ========================================`)
- Place test data generators near their tests
- Order tests by lifecycle: Search → Get → Create → Update → Delete

## Special Cases

### Namespace Conflicts

When testing types with the same name as their namespace (e.g., `User` type in `Domain.User` namespace), use `global::` prefix to avoid ambiguity.

**When to use**: For types like `User`, `SubscriptionType`, etc. where the type name matches the namespace.

See [examples.md](examples.md#namespace-conflicts) for example.

### Required Properties

Always initialize all required properties on records. Missing required properties will cause compilation errors.

See [examples.md](examples.md#required-properties) for example.

## Running Tests

```bash
# Run all unit tests
pls unit

# Run unit tests and watch for changes
pls unit:watch

# Run unit tests with coverage
pls unit:cover

# Pass additional arguments
pls unit -- --filter "FullyQualifiedName~ServiceTests"
pls unit -- --filter "FullyQualifiedName~Create_WithValidRecord"
```

## Pre-Submission Checklist

Before submitting tests, verify:

- [ ] Test file mirrors domain structure
- [ ] Test class name ends with `Tests`
- [ ] All tests use `[Theory]` (not `[Fact]`)
- [ ] Primitive types use `[InlineData]`
- [ ] Complex types use `[MemberData]` with static generators
- [ ] Each scenario has 3-4 test data variations
- [ ] AAA pattern clearly separated
- [ ] Test names follow `{Method}_{Scenario}_{ShouldExpected}` format with "Should"
- [ ] Edge cases tested (null, empty, boundary values)
- [ ] Both success and failure paths tested (where applicable)
- [ ] FluentAssertions used for all assertions
- [ ] Mocks properly configured with Setup and Verify
- [ ] Transaction manager mocked for Create/Update/Delete operations
- [ ] **NO dynamic values** - All test data is static (no `Guid.NewGuid()`, `DateTime.Now`, etc.)
- [ ] Real GUIDs used (not made-up patterns)
- [ ] All required properties initialized
- [ ] Modern C# 12 collection expressions used correctly
- [ ] Namespace conflicts resolved with `global::`
- [ ] Coverage target met for the component
- [ ] Tests pass: `pls unit`

## Reference Test Files

See these files for complete examples:

- `UnitTest/Domain/Projects/ServiceTests.cs` - Simple CRUD with GUIDs
- `UnitTest/Domain/User/ServiceTests.cs` - String IDs and namespace conflicts
- `UnitTest/Domain/Marketing/Subscribers/ServiceTests.cs` - Complex nested objects
- `UnitTest/Domain/Marketing/SubscriptionType/ServiceTests.cs` - Composite keys
- `UnitTest/Domain/Legal/ServiceTests.cs` - All patterns with transaction manager

## Quick Start

1. **Read** [examples.md](examples.md) for code templates
2. **Reference** [reference.md](reference.md) for official documentation
3. **Follow** the AAA pattern and BDD naming convention
4. **Mock** all dependencies (repositories, services, transaction managers)
5. **Use** static, deterministic test data only
6. **Test** with `pls unit` before submitting
