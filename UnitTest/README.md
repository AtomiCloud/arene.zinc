# Unit Test Framework Guidelines

This document defines the unit testing standards and practices for this project.

## Core Principles

### 1. **Fluent Assertions Style**
All assertions use FluentAssertions for readable, expressive tests:
```csharp
actual.Should().Be(expected);
actual.Should().NotBeNull();
result.Should().BeSuccess();
```

### 2. **Mirror Domain Structure**
Test file organization mirrors the domain/module structure:
```
Domain/Projects/Service.cs          → UnitTest/Domain/Projects/ServiceTests.cs
Domain/Projects/Model.cs            → UnitTest/Domain/Projects/ModelTests.cs
App/Modules/Projects/Data/Repository → UnitTest/App/Modules/Projects/Data/RepositoryTests.cs
```

**Naming Convention**: `{ClassName}Tests.cs`

### 3. **AAA Pattern (Arrange-Act-Assert)**
Every test follows the triple-A structure with clear sections:

```csharp
[Theory]
[MemberData(nameof(PositiveNumberTestData))]
public void Add_WithPositiveNumbers_ReturnsCorrectSum(int a, int b, int expected)
{
  // Arrange
  var mockLogger = new Mock<ILogger<Calculator>>();
  var calculator = new Calculator(mockLogger.Object);
  var input = new AddRequest(a, b);

  // Act
  var actual = calculator.Add(input);

  // Assert
  actual.Should().Be(expected);
}
```

**Arrange Section**:
- Create mocks for dependencies
- Construct the subject under test
- Prepare input data
- Define expected output

**Act Section**:
- Execute the method under test
- Store result in `actual` variable
- Single line of execution

**Assert Section**:
- Compare `actual` with `expected`
- Use FluentAssertions for clarity
- Multiple assertions allowed when testing complex objects

---

## 4. **Theory-Based Testing**

**All tests use `[Theory]` with multiple data sets** to triangulate behavior.

### Rule: Never use `[Fact]` for business logic tests

**Why?** Testing with multiple inputs ensures the logic works generally, not just for one case.

### For Primitive Types: Use `[InlineData]`

```csharp
[Theory]
[InlineData(2, 3, 5)]
[InlineData(10, 20, 30)]
[InlineData(100, 200, 300)]
[InlineData(1, 1, 2)]
public void Add_WithPositiveNumbers_ReturnsCorrectSum(int a, int b, int expected)
{
  // Arrange
  var calculator = new Calculator();

  // Act
  var actual = calculator.Add(a, b);

  // Assert
  actual.Should().Be(expected);
}
```

### For Complex Types: Use `[MemberData]` with Static Generators

```csharp
[Theory]
[MemberData(nameof(ValidProjectRecordTestData))]
public void Create_WithValidRecord_ReturnsProjectPrincipal(
  ProjectRecord input,
  ProjectPrincipal expected)
{
  // Arrange
  var mockRepo = new Mock<IProjectRepository>();
  mockRepo.Setup(x => x.Create(It.IsAny<ProjectRecord>()))
    .ReturnsAsync(Result.Ok(expected));
  var service = new ProjectService(mockRepo.Object);

  // Act
  var actual = service.Create(input).Result;

  // Assert
  actual.Should().BeSuccess();
  actual.ValueOrDefault().Should().BeEquivalentTo(expected);
}

public static IEnumerable<object[]> ValidProjectRecordTestData()
{
  yield return new object[]
  {
    new ProjectRecord { Name = "Project Alpha", Open = true },
    new ProjectPrincipal
    {
      Id = Guid.NewGuid(),
      Record = new ProjectRecord { Name = "Project Alpha", Open = true }
    }
  };

  yield return new object[]
  {
    new ProjectRecord { Name = "Project Beta", Open = false },
    new ProjectPrincipal
    {
      Id = Guid.NewGuid(),
      Record = new ProjectRecord { Name = "Project Beta", Open = false }
    }
  };

  yield return new object[]
  {
    new ProjectRecord { Name = "X", Open = true },
    new ProjectPrincipal
    {
      Id = Guid.NewGuid(),
      Record = new ProjectRecord { Name = "X", Open = true }
    }
  };
}
```

**Generator Rules**:
- Must be `public static`
- Return type: `IEnumerable<object[]>`
- Use `yield return` for each test case
- Strongly type the method parameters for clarity

---

## 5. **Test Data Triangulation**

**Goal**: Test 3-4 variations per scenario to ensure general correctness.

### Example Scenarios:

**Scenario 1: Add Positive Numbers**
```csharp
[Theory]
[InlineData(2, 3, 5)]      // Small numbers
[InlineData(10, 20, 30)]   // Medium numbers
[InlineData(100, 200, 300)] // Large numbers
[InlineData(1, 1, 2)]       // Edge case: same number
public void Add_WithPositiveNumbers_ReturnsCorrectSum(int a, int b, int expected)
```

**Scenario 2: Add Negative Numbers**
```csharp
[Theory]
[InlineData(-2, -3, -5)]
[InlineData(-10, -20, -30)]
[InlineData(-100, -200, -300)]
[InlineData(-1, -1, -2)]
public void Add_WithNegativeNumbers_ReturnsCorrectSum(int a, int b, int expected)
```

**Scenario 3: Add Mixed Numbers**
```csharp
[Theory]
[InlineData(2, -3, -1)]
[InlineData(-10, 20, 10)]
[InlineData(100, -200, -100)]
[InlineData(0, 5, 5)]
public void Add_WithMixedNumbers_ReturnsCorrectSum(int a, int b, int expected)
```

---

## 6. **Edge Case Testing**

Always test:
- **Null values** (when applicable)
- **Empty collections** (lists, arrays, enumerables)
- **Boundary values** (min, max, zero)
- **Special characters** (in strings)
- **Invalid inputs** (expect exceptions or error results)

### Example: Testing with Nulls

```csharp
[Theory]
[MemberData(nameof(NullInputTestData))]
public void Search_WithNullName_ReturnsAllProjects(ProjectSearch input, int expectedCount)
{
  // Arrange
  var mockRepo = new Mock<IProjectRepository>();
  mockRepo.Setup(x => x.Search(It.IsAny<ProjectSearch>()))
    .ReturnsAsync(Result.Ok(GetTestProjects()));
  var service = new ProjectService(mockRepo.Object);

  // Act
  var actual = service.Search(input).Result;

  // Assert
  actual.Should().BeSuccess();
  actual.ValueOrDefault().Should().HaveCount(expectedCount);
}

public static IEnumerable<object[]> NullInputTestData()
{
  yield return new object[] { new ProjectSearch { Name = null, Limit = 100, Skip = 0 }, 5 };
  yield return new object[] { new ProjectSearch { Name = null, Id = null, Limit = 50, Skip = 0 }, 5 };
  yield return new object[] { new ProjectSearch { Name = null, Limit = 10, Skip = 0 }, 5 };
}
```

### Example: Testing Empty Collections

```csharp
[Theory]
[InlineData("")]
[InlineData("NonExistentProject")]
[InlineData("ZZZZZ")]
public void Search_WithNoResults_ReturnsEmptyList(string searchName)
{
  // Arrange
  var mockRepo = new Mock<IProjectRepository>();
  mockRepo.Setup(x => x.Search(It.Is<ProjectSearch>(s => s.Name == searchName)))
    .ReturnsAsync(Result.Ok(Enumerable.Empty<ProjectPrincipal>()));
  var service = new ProjectService(mockRepo.Object);
  var input = new ProjectSearch { Name = searchName, Limit = 100, Skip = 0 };

  // Act
  var actual = service.Search(input).Result;

  // Assert
  actual.Should().BeSuccess();
  actual.ValueOrDefault().Should().BeEmpty();
}
```

---

## 7. **100% Domain Logic Coverage**

**Goal**: Every domain service, repository interface, and business logic method must have tests.

### Coverage Requirements:

| Component | Coverage Target | Priority |
|-----------|----------------|----------|
| Domain Services | 100% | Critical |
| Domain Models (behavior) | 100% | Critical |
| Repository Implementations | 90%+ | High |
| API Controllers | 80%+ | High |
| Mappers | 80%+ | Medium |
| Validators | 100% | High |

**Exclude from coverage**:
- DTOs with no behavior
- Auto-generated code
- Infrastructure startup code

---

## 8. **Test Naming Convention**

**Format**: `{MethodName}_{Scenario}_{ExpectedBehavior}`

**Examples**:
```csharp
Create_WithValidRecord_ReturnsProjectPrincipal
Create_WithDuplicateName_ReturnsConflictError
Search_WithNullName_ReturnsAllProjects
Search_WithValidName_ReturnsMatchingProjects
Delete_WithExistingId_ReturnsSuccess
Delete_WithNonExistentId_ReturnsNotFound
```

**Benefits**:
- Instantly understand what's being tested
- Clearly see the scenario and expected outcome
- Self-documenting tests

---

## 9. **Mocking Strategy**

### Use Moq for Interfaces

```csharp
var mockRepo = new Mock<IProjectRepository>();
var mockLogger = new Mock<ILogger<ProjectService>>();
var mockTransaction = new Mock<ITransactionManager>();
```

### Setup Return Values

```csharp
mockRepo.Setup(x => x.Get(It.IsAny<Guid>()))
  .ReturnsAsync(Result.Ok(expectedProject));

mockRepo.Setup(x => x.Create(It.Is<ProjectRecord>(r => r.Name == "Test")))
  .ReturnsAsync(Result.Ok(expectedPrincipal));
```

### Verify Interactions (Optional)

```csharp
// After Act
mockRepo.Verify(x => x.Create(It.IsAny<ProjectRecord>()), Times.Once);
mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
```

---

## 10. **Testing Result Monad Pattern**

This codebase uses `Result<T>` pattern. Test both success and failure paths:

### Success Path

```csharp
[Theory]
[MemberData(nameof(ValidInputTestData))]
public void Create_WithValidInput_ReturnsSuccess(ProjectRecord input)
{
  // Arrange
  var mockRepo = new Mock<IProjectRepository>();
  mockRepo.Setup(x => x.Create(It.IsAny<ProjectRecord>()))
    .ReturnsAsync(Result.Ok(new ProjectPrincipal { Id = Guid.NewGuid(), Record = input }));
  var service = new ProjectService(mockRepo.Object);

  // Act
  var actual = service.Create(input).Result;

  // Assert
  actual.Should().BeSuccess();
  actual.ValueOrDefault().Should().NotBeNull();
  actual.ValueOrDefault().Record.Name.Should().Be(input.Name);
}
```

### Failure Path

```csharp
[Theory]
[MemberData(nameof(InvalidInputTestData))]
public void Create_WithInvalidInput_ReturnsFailure(ProjectRecord input, Type expectedErrorType)
{
  // Arrange
  var mockRepo = new Mock<IProjectRepository>();
  mockRepo.Setup(x => x.Create(It.IsAny<ProjectRecord>()))
    .ReturnsAsync(Result.Fail<ProjectPrincipal>(new ValidationError("Invalid input")));
  var service = new ProjectService(mockRepo.Object);

  // Act
  var actual = service.Create(input).Result;

  // Assert
  actual.Should().BeFailure();
  actual.FailureOrDefault().Should().BeOfType(expectedErrorType);
}
```

---

## 11. **Test Organization in Files**

### Structure Within Test Class

```csharp
namespace UnitTest.Domain.Projects;

public class ServiceTests
{
  // ========================================
  // Search Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public void Search_WithValidInput_ReturnsResults(ProjectSearch input, int expectedCount) { }

  [Theory]
  [InlineData(null)]
  public void Search_WithNullInput_ReturnsAllResults(ProjectSearch input) { }

  public static IEnumerable<object[]> ValidSearchTestData() { }

  // ========================================
  // Create Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidCreateTestData))]
  public void Create_WithValidRecord_ReturnsSuccess(ProjectRecord input) { }

  [Theory]
  [MemberData(nameof(InvalidCreateTestData))]
  public void Create_WithInvalidRecord_ReturnsFailure(ProjectRecord input) { }

  public static IEnumerable<object[]> ValidCreateTestData() { }
  public static IEnumerable<object[]> InvalidCreateTestData() { }

  // ========================================
  // Update Method Tests
  // ========================================

  // ... etc
}
```

**Organization Rules**:
- Group tests by method being tested
- Add section comments for clarity
- Place test data generators near their tests
- Order tests by lifecycle: Search → Get → Create → Update → Delete

---

## 12. **Running Tests**

### Run All Tests
```bash
pls exec -- dotnet test
```

### Run Specific Test Class
```bash
pls exec -- dotnet test --filter "FullyQualifiedName~ServiceTests"
```

### Run Specific Test Method
```bash
pls exec -- dotnet test --filter "FullyQualifiedName~Create_WithValidRecord_ReturnsSuccess"
```

### Generate Coverage Report
```bash
pls exec -- dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 13. **Example: Complete Test Class**

See `UnitTest/Domain/Projects/ServiceTests.cs` for a complete example following all these guidelines.

---

## Checklist for Writing Tests

Before submitting tests, verify:

- [ ] Test file mirrors domain structure
- [ ] Test class name ends with `Tests`
- [ ] All tests use `[Theory]` (not `[Fact]`)
- [ ] Primitive types use `[InlineData]`
- [ ] Complex types use `[MemberData]` with static generators
- [ ] Each scenario has 3-4 test data variations
- [ ] AAA pattern clearly separated (comments optional but recommended)
- [ ] Test names follow `{Method}_{Scenario}_{Expected}` format
- [ ] Edge cases tested (null, empty, boundary values)
- [ ] Both success and failure paths tested
- [ ] FluentAssertions used for all assertions
- [ ] Mocks properly configured
- [ ] Coverage target met for the component

---

## Quick Reference

### Theory with InlineData (Primitives)
```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(10, 20, 30)]
public void Method_Scenario_Expected(int a, int b, int expected) { }
```

### Theory with MemberData (Complex)
```csharp
[Theory]
[MemberData(nameof(TestData))]
public void Method_Scenario_Expected(ComplexType input, ComplexType expected) { }

public static IEnumerable<object[]> TestData()
{
  yield return new object[] { input1, expected1 };
  yield return new object[] { input2, expected2 };
}
```

### AAA Template
```csharp
// Arrange
var mock = new Mock<IDependency>();
var subject = new ClassUnderTest(mock.Object);
var input = new Input();
var expected = new Expected();

// Act
var actual = subject.Method(input);

// Assert
actual.Should().Be(expected);
```
