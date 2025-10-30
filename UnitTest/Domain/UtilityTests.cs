using CarboxylicLithium;
using Domain;
using Domain.Exceptions;
using Domain.Projects;
using FluentAssertions;

namespace UnitTest.Domain;

public class UtilityTests
{
  // ========================================
  // NullToError with Value Tests
  // ========================================

  [Theory]
  [MemberData(nameof(NullToErrorWithNonNullValueTestData))]
  public void NullToError_WithNonNullValue_ShouldReturnSuccess(
    ProjectPrincipal value,
    string identifier,
    ProjectPrincipal expectedValue)
  {
    // Arrange & Act
    var actual = Utility.NullToError(value, identifier);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedValue));
  }

  public static IEnumerable<object[]> NullToErrorWithNonNullValueTestData()
  {
    yield return new object[]
    {
      new ProjectPrincipal
      {
        Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
        Record = new ProjectRecord { Name = "Test Project 1", Open = true }
      },
      "test-id-1",
      new ProjectPrincipal
      {
        Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d"),
        Record = new ProjectRecord { Name = "Test Project 1", Open = true }
      }
    };

    yield return new object[]
    {
      new ProjectPrincipal
      {
        Id = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"),
        Record = new ProjectRecord { Name = "Test Project 2", Open = false }
      },
      "test-id-2",
      new ProjectPrincipal
      {
        Id = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d0e-1f2a3b4c5d6e"),
        Record = new ProjectRecord { Name = "Test Project 2", Open = false }
      }
    };

    yield return new object[]
    {
      new ProjectPrincipal
      {
        Id = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e1f-2a3b4c5d6e7f"),
        Record = new ProjectRecord { Name = "Test Project 3", Open = true }
      },
      "test-id-3",
      new ProjectPrincipal
      {
        Id = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e1f-2a3b4c5d6e7f"),
        Record = new ProjectRecord { Name = "Test Project 3", Open = true }
      }
    };
  }

  [Theory]
  [InlineData("identifier-1")]
  [InlineData("identifier-2")]
  [InlineData("identifier-3")]
  public void NullToError_WithNullValue_ShouldReturnNotFoundException(string identifier)
  {
    // Arrange
    ProjectPrincipal? value = null;

    // Act
    var actual = Utility.NullToError(value, identifier);

    // Assert
    actual.IsSuccess().Should().BeFalse();
    actual.FailureOrDefault().Should().BeOfType<NotFoundException>();
    var notFoundError = actual.FailureOrDefault() as NotFoundException;
    notFoundError!.Type.Should().Be(typeof(ProjectPrincipal));
    notFoundError.RequestIdentifier.Should().Be(identifier);
  }

  // ========================================
  // NullToError with Result Tests
  // ========================================

  [Theory]
  [MemberData(nameof(NullToErrorWithResultNonNullTestData))]
  public void NullToError_WithResultNonNullValue_ShouldReturnSuccess(
    Result<ProjectPrincipal?> resultValue,
    string identifier,
    ProjectPrincipal expectedValue)
  {
    // Arrange & Act
    var actual = resultValue.NullToError(identifier);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedValue));
  }

  public static IEnumerable<object[]> NullToErrorWithResultNonNullTestData()
  {
    yield return new object[]
    {
      new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f2a-3b4c5d6e7f8a"),
        Record = new ProjectRecord { Name = "Result Project 1", Open = true }
      }),
      "result-id-1",
      new ProjectPrincipal
      {
        Id = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f2a-3b4c5d6e7f8a"),
        Record = new ProjectRecord { Name = "Result Project 1", Open = true }
      }
    };

    yield return new object[]
    {
      new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a3b-4c5d6e7f8a9b"),
        Record = new ProjectRecord { Name = "Result Project 2", Open = false }
      }),
      "result-id-2",
      new ProjectPrincipal
      {
        Id = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a3b-4c5d6e7f8a9b"),
        Record = new ProjectRecord { Name = "Result Project 2", Open = false }
      }
    };

    yield return new object[]
    {
      new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b4c-5d6e7f8a9b0c"),
        Record = new ProjectRecord { Name = "Result Project 3", Open = true }
      }),
      "result-id-3",
      new ProjectPrincipal
      {
        Id = Guid.Parse("f6a7b8c9-d0e1-4f5a-3b4c-5d6e7f8a9b0c"),
        Record = new ProjectRecord { Name = "Result Project 3", Open = true }
      }
    };
  }

  [Theory]
  [InlineData("result-null-1")]
  [InlineData("result-null-2")]
  [InlineData("result-null-3")]
  public void NullToError_WithResultNullValue_ShouldReturnNotFoundException(string identifier)
  {
    // Arrange
    var resultValue = new Result<ProjectPrincipal?>(null as ProjectPrincipal);

    // Act
    var actual = resultValue.NullToError(identifier);

    // Assert
    actual.IsSuccess().Should().BeFalse();
    actual.FailureOrDefault().Should().BeOfType<NotFoundException>();
    var notFoundError = actual.FailureOrDefault() as NotFoundException;
    notFoundError!.Type.Should().Be(typeof(ProjectPrincipal));
    notFoundError.RequestIdentifier.Should().Be(identifier);
  }

  // ========================================
  // NullToError with Task<Result> Tests
  // ========================================

  [Theory]
  [MemberData(nameof(NullToErrorWithTaskResultNonNullTestData))]
  public async Task NullToError_WithTaskResultNonNullValue_ShouldReturnSuccess(
    Task<Result<ProjectPrincipal?>> taskResultValue,
    string identifier,
    ProjectPrincipal expectedValue)
  {
    // Arrange & Act
    var actual = await taskResultValue.NullToError(identifier);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedValue));
  }

  public static IEnumerable<object[]> NullToErrorWithTaskResultNonNullTestData()
  {
    yield return new object[]
    {
      Task.FromResult(new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c5d-6e7f8a9b0c1d"),
        Record = new ProjectRecord { Name = "Task Project 1", Open = true }
      })),
      "task-id-1",
      new ProjectPrincipal
      {
        Id = Guid.Parse("a7b8c9d0-e1f2-4a5b-4c5d-6e7f8a9b0c1d"),
        Record = new ProjectRecord { Name = "Task Project 1", Open = true }
      }
    };

    yield return new object[]
    {
      Task.FromResult(new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("b8c9d0e1-f2a3-4b5c-5d6e-7f8a9b0c1d2e"),
        Record = new ProjectRecord { Name = "Task Project 2", Open = false }
      })),
      "task-id-2",
      new ProjectPrincipal
      {
        Id = Guid.Parse("b8c9d0e1-f2a3-4b5c-5d6e-7f8a9b0c1d2e"),
        Record = new ProjectRecord { Name = "Task Project 2", Open = false }
      }
    };

    yield return new object[]
    {
      Task.FromResult(new Result<ProjectPrincipal?>(new ProjectPrincipal
      {
        Id = Guid.Parse("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"),
        Record = new ProjectRecord { Name = "Task Project 3", Open = true }
      })),
      "task-id-3",
      new ProjectPrincipal
      {
        Id = Guid.Parse("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"),
        Record = new ProjectRecord { Name = "Task Project 3", Open = true }
      }
    };
  }

  [Theory]
  [InlineData("task-null-1")]
  [InlineData("task-null-2")]
  [InlineData("task-null-3")]
  public async Task NullToError_WithTaskResultNullValue_ShouldReturnNotFoundException(string identifier)
  {
    // Arrange
    var taskResultValue = Task.FromResult(new Result<ProjectPrincipal?>(null as ProjectPrincipal));

    // Act
    var actual = await taskResultValue.NullToError(identifier);

    // Assert
    actual.IsSuccess().Should().BeFalse();
    actual.FailureOrDefault().Should().BeOfType<NotFoundException>();
    var notFoundError = actual.FailureOrDefault() as NotFoundException;
    notFoundError!.Type.Should().Be(typeof(ProjectPrincipal));
    notFoundError.RequestIdentifier.Should().Be(identifier);
  }

  // ========================================
  // ToZonedDateTime Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ToZonedDateTimeTestData))]
  public void ToZonedDateTime_WithValidInputs_ShouldConvertToUtc(
    DateOnly date,
    TimeOnly time,
    TimeZoneInfo timezone,
    DateTime expectedUtc)
  {
    // Arrange & Act
    var actual = date.ToZonedDateTime(time, timezone);

    // Assert
    actual.Should().Be(expectedUtc);
    actual.Kind.Should().Be(DateTimeKind.Utc);
  }

  public static IEnumerable<object[]> ToZonedDateTimeTestData()
  {
    // Test case 1: PST timezone (UTC-8)
    var pst = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");
    yield return new object[]
    {
      new DateOnly(2025, 1, 15),
      new TimeOnly(10, 30, 0),
      pst,
      new DateTime(2025, 1, 15, 18, 30, 0, DateTimeKind.Utc) // 10:30 PST = 18:30 UTC
    };

    // Test case 2: UTC timezone
    var utc = TimeZoneInfo.Utc;
    yield return new object[]
    {
      new DateOnly(2025, 6, 1),
      new TimeOnly(14, 45, 30),
      utc,
      new DateTime(2025, 6, 1, 14, 45, 30, DateTimeKind.Utc)
    };

    // Test case 3: JST timezone (UTC+9)
    var jst = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
    yield return new object[]
    {
      new DateOnly(2025, 12, 25),
      new TimeOnly(23, 59, 59),
      jst,
      new DateTime(2025, 12, 25, 14, 59, 59, DateTimeKind.Utc) // 23:59 JST = 14:59 UTC
    };
  }

  [Theory]
  [InlineData(2025, 1, 1, 0, 0, 0)]
  [InlineData(2025, 6, 15, 12, 30, 45)]
  [InlineData(2025, 12, 31, 23, 59, 59)]
  public void ToZonedDateTime_WithUtcTimezone_ShouldReturnSameTime(
    int year,
    int month,
    int day,
    int hour,
    int minute,
    int second)
  {
    // Arrange
    var date = new DateOnly(year, month, day);
    var time = new TimeOnly(hour, minute, second);
    var timezone = TimeZoneInfo.Utc;
    var expected = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

    // Act
    var actual = date.ToZonedDateTime(time, timezone);

    // Assert
    actual.Should().Be(expected);
  }
}
