using CarboxylicLithium;
using Domain;
using Domain.Marketing.Subscribers;
using FluentAssertions;
using Moq;

namespace UnitTest.Domain.Marketing.Subscribers;

public class ServiceTests
{
  // ========================================
  // Search Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(
    SubscriberSearch input,
    IEnumerable<SubscriberPrincipal> mockRepoResult,
    IEnumerable<SubscriberPrincipal> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriberRepository>();
    mockRepo.Setup(x => x.Search(It.IsAny<SubscriberSearch>()))
      .ReturnsAsync(new Result<IEnumerable<SubscriberPrincipal>>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<SubscriberPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<SubscriberSearch>()), Times.Once);
  }

  [Theory]
  [InlineData("test@example.com")]
  [InlineData("nonexistent@test.org")]
  [InlineData("unknown@example.com")]
  public async Task Search_WithNoResults_ShouldReturnEmptyList(string email)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriberRepository>();
    List<SubscriberPrincipal> mockRepoResult = [];
    List<SubscriberPrincipal> expectedOutput = [];
    mockRepo.Setup(x => x.Search(It.IsAny<SubscriberSearch>()))
      .ReturnsAsync(new Result<IEnumerable<SubscriberPrincipal>>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);
    var input = new SubscriberSearch { Email = email, Limit = 100, Skip = 0 };

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<SubscriberPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<SubscriberSearch>()), Times.Once);
  }

  public static IEnumerable<object[]> ValidSearchTestData()
  {
    yield return new object[]
    {
      new SubscriberSearch { Email = "alice@example.com", Limit = 100, Skip = 0 },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("9228b0f7-34e2-4018-9f88-bee5a4799e57"),
          Email = "alice@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "UTC",
            Subscriptions =
            [
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "User opted in",
                UpdatedAt = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("9228b0f7-34e2-4018-9f88-bee5a4799e57"),
          Email = "alice@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "UTC",
            Subscriptions =
            [
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "User opted in",
                UpdatedAt = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      }
    };

    yield return new object[]
    {
      new SubscriberSearch
      {
        ProjectId = Guid.Parse("22594dd5-d820-4a2a-8882-8950df9538fb"),
        Limit = 50,
        Skip = 0
      },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("22594dd5-d820-4a2a-8882-8950df9538fb"),
          Email = "bob@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "America/New_York",
            Subscriptions =
            [
              new()
              {
                Type = "updates",
                Enabled = false,
                LegalBasis = LegalBasis.None,
                LegalReason = "User opted out",
                UpdatedAt = new DateTimeOffset(2025, 1, 16, 14, 20, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("22594dd5-d820-4a2a-8882-8950df9538fb"),
          Email = "bob@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "America/New_York",
            Subscriptions =
            [
              new()
              {
                Type = "updates",
                Enabled = false,
                LegalBasis = LegalBasis.None,
                LegalReason = "User opted out",
                UpdatedAt = new DateTimeOffset(2025, 1, 16, 14, 20, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      }
    };

    yield return new object[]
    {
      new SubscriberSearch { Limit = 20, Skip = 0 },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("e8397c5b-cb42-437b-859c-a48bc288cf7b"),
          Email = "charlie@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "Europe/London",
            Subscriptions =
            [
              new()
              {
                Type = "marketing",
                Enabled = true,
                LegalBasis = LegalBasis.LegitimateInterests,
                LegalReason = "Customer relationship",
                UpdatedAt = new DateTimeOffset(2025, 1, 17, 9, 15, 0, TimeSpan.Zero)
              },
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "Explicit consent",
                UpdatedAt = new DateTimeOffset(2025, 1, 17, 9, 16, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      },
      new List<SubscriberPrincipal>
      {
        new()
        {
          ProjectId = Guid.Parse("e8397c5b-cb42-437b-859c-a48bc288cf7b"),
          Email = "charlie@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "Europe/London",
            Subscriptions =
            [
              new()
              {
                Type = "marketing",
                Enabled = true,
                LegalBasis = LegalBasis.LegitimateInterests,
                LegalReason = "Customer relationship",
                UpdatedAt = new DateTimeOffset(2025, 1, 17, 9, 15, 0, TimeSpan.Zero)
              },
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "Explicit consent",
                UpdatedAt = new DateTimeOffset(2025, 1, 17, 9, 16, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      }
    };
  }

  // ========================================
  // Get Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(GetValidTestData))]
  public async Task Get_WithExistingEmailAndProject_ShouldReturnSubscriber(
    Guid projectId,
    string email,
    Subscriber? mockRepoResult,
    Subscriber? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriberRepository>();
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Subscriber?>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.Get(projectId, email);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Subscriber?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("24945523-bdec-41aa-95cc-d59e0296c89b", "nonexistent@example.com")]
  [InlineData("c8bc465c-f8b9-4cef-b337-4b34097e41b4", "unknown@test.org")]
  [InlineData("7be63fe4-f9e3-4169-84c9-3e8c1eebc231", "fake@example.com")]
  public async Task Get_WithNonExisting_ShouldReturnNull(string projectIdStr, string email)
  {
    // Arrange
    var projectId = Guid.Parse(projectIdStr);
    var mockRepo = new Mock<ISubscriberRepository>();
    Subscriber? mockRepoResult = null;
    Subscriber? expectedOutput = null;
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Subscriber?>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.Get(projectId, email);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Subscriber?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }

  public static IEnumerable<object[]> GetValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("b446b9ab-aa39-4c7e-945e-cc5543a7d01c"),
      "alice@example.com",
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("b446b9ab-aa39-4c7e-945e-cc5543a7d01c"),
          Email = "alice@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "UTC",
            Subscriptions =
            [
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "User signup",
                UpdatedAt = new DateTimeOffset(2025, 1, 18, 10, 0, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      },
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("b446b9ab-aa39-4c7e-945e-cc5543a7d01c"),
          Email = "alice@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "UTC",
            Subscriptions =
            [
              new()
              {
                Type = "newsletter",
                Enabled = true,
                LegalBasis = LegalBasis.Consent,
                LegalReason = "User signup",
                UpdatedAt = new DateTimeOffset(2025, 1, 18, 10, 0, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      }
    };

    yield return new object[]
    {
      Guid.Parse("981ee662-da68-4175-b6d9-c112afcafdfd"),
      "bob@example.com",
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("981ee662-da68-4175-b6d9-c112afcafdfd"),
          Email = "bob@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "America/Los_Angeles",
            Subscriptions = []
          }
        }
      },
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("981ee662-da68-4175-b6d9-c112afcafdfd"),
          Email = "bob@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "America/Los_Angeles",
            Subscriptions = []
          }
        }
      }
    };

    yield return new object[]
    {
      Guid.Parse("9cf4bfe6-7734-4cc1-beab-c542babc7dfb"),
      "charlie@example.com",
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("9cf4bfe6-7734-4cc1-beab-c542babc7dfb"),
          Email = "charlie@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "Asia/Tokyo",
            Subscriptions =
            [
              new()
              {
                Type = "updates",
                Enabled = true,
                LegalBasis = LegalBasis.Contract,
                LegalReason = "Service agreement",
                UpdatedAt = new DateTimeOffset(2025, 1, 19, 15, 30, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      },
      new Subscriber
      {
        Principal = new SubscriberPrincipal
        {
          ProjectId = Guid.Parse("9cf4bfe6-7734-4cc1-beab-c542babc7dfb"),
          Email = "charlie@example.com",
          Computed = new SubscriberComputed
          {
            TimeZone = "Asia/Tokyo",
            Subscriptions =
            [
              new()
              {
                Type = "updates",
                Enabled = true,
                LegalBasis = LegalBasis.Contract,
                LegalReason = "Service agreement",
                UpdatedAt = new DateTimeOffset(2025, 1, 19, 15, 30, 0, TimeSpan.Zero)
              }
            ]
          }
        }
      }
    };
  }

  // ========================================
  // RecordSubscription Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(RecordSubscriptionValidTestData))]
  public async Task RecordSubscription_WithValidInput_ShouldReturnUnit(
    Guid projectId,
    string email,
    SubscriptionEvent subscription,
    Unit? mockRepoResult,
    Unit? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriberRepository>();
    mockRepo.Setup(x => x.RecordSubscription(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionEvent>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.RecordSubscription(projectId, email, subscription);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.RecordSubscription(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionEvent>()), Times.Once);
  }

  public static IEnumerable<object[]> RecordSubscriptionValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("f4d8a6e7-5a79-40db-bad0-081c93bdc2ac"),
      "user1@example.com",
      new SubscriptionEvent
      {
        Type = "newsletter",
        LegalBasis = LegalBasis.Consent,
        Reason = "User opted in",
        Open = false,
        Timezone = "UTC",
        Time = new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero)
      },
      new Unit(),
      new Unit()
    };

    yield return new object[]
    {
      Guid.Parse("d2e9fded-f4b8-4e58-abfc-cb20b31d048f"),
      "user2@example.com",
      new SubscriptionEvent
      {
        Type = "marketing",
        LegalBasis = LegalBasis.LegitimateInterests,
        Reason = "Business relationship",
        Open = true,
        Timezone = "America/New_York",
        Time = new DateTimeOffset(2025, 2, 1, 14, 30, 0, TimeSpan.FromHours(-5))
      },
      new Unit(),
      new Unit()
    };

    yield return new object[]
    {
      Guid.Parse("fcb46b36-253d-4ec0-b49b-53534fe88cf2"),
      "user3@example.com",
      new SubscriptionEvent
      {
        Type = "updates",
        LegalBasis = LegalBasis.Contract,
        Reason = "Service terms",
        Open = false,
        Timezone = "Europe/Paris",
        Time = new DateTimeOffset(2025, 3, 1, 9, 0, 0, TimeSpan.FromHours(1))
      },
      new Unit(),
      new Unit()
    };
  }

  // ========================================
  // Delete Method Tests
  // ========================================

  [Theory]
  [InlineData("5b592352-c9bd-4df2-8d75-6dba0bd4d76b", "test1@example.com")]
  [InlineData("fdc8d93a-9176-45b3-8a61-0f30523e67a4", "test2@example.com")]
  [InlineData("6c9ed397-c517-4113-ade7-17a26905f214", "test3@example.com")]
  public async Task Delete_WithExisting_ShouldReturnUnit(string projectIdStr, string email)
  {
    // Arrange
    var projectId = Guid.Parse(projectIdStr);
    var mockRepo = new Mock<ISubscriberRepository>();
    Unit? mockRepoResult = new Unit();
    Unit? expectedOutput = new Unit();
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.Delete(projectId, email);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("5d06ba24-874a-4720-a423-52d6f83f8d29", "nonexistent@example.com")]
  [InlineData("474a60ff-881b-4be9-9e70-ea91c526a062", "unknown@test.org")]
  [InlineData("c41cfdf5-8785-4fdb-88e8-ef50221bb1e4", "fake@example.com")]
  public async Task Delete_WithNonExisting_ShouldReturnNull(string projectIdStr, string email)
  {
    // Arrange
    var projectId = Guid.Parse(projectIdStr);
    var mockRepo = new Mock<ISubscriberRepository>();
    Unit? mockRepoResult = null;
    Unit? expectedOutput = null;
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new SubscriberService(mockRepo.Object);

    // Act
    var actual = await service.Delete(projectId, email);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }
}
