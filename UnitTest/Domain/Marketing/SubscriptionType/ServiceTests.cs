using CarboxylicLithium;
using Domain;
using Domain.Marketing.SubscriptionType;
using FluentAssertions;
using Moq;

namespace UnitTest.Domain.Marketing.SubscriptionType;

public class ServiceTests
{
  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(
    SubscriptionTypeSearch input,
    IEnumerable<SubscriptionTypePrincipal> mockRepoResult,
    IEnumerable<SubscriptionTypePrincipal> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    mockRepo.Setup(x => x.Search(It.IsAny<SubscriptionTypeSearch>()))
      .ReturnsAsync(new Result<IEnumerable<SubscriptionTypePrincipal>>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<SubscriptionTypePrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<SubscriptionTypeSearch>()), Times.Once);
  }

  public static IEnumerable<object[]> ValidSearchTestData()
  {
    var projectId1 = Guid.Parse("a6d00064-0cef-47ce-b794-c14eeefafc06");
    var projectId2 = Guid.Parse("e4963aa0-e87f-4ab5-838e-5c9289023396");

    yield return new object[]
    {
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeSearch { Name = "newsletter", Limit = 100, Skip = 0 },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId1, Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Weekly newsletter" } }
      },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId1, Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Weekly newsletter" } }
      }
    };

    yield return new object[]
    {
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeSearch { Desc = "marketing", Limit = 50, Skip = 0 },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId2, Id = "marketing_emails", Record = new SubscriptionTypeRecord { Desc = "Marketing promotions" } },
        new() { ProjectId = projectId2, Id = "marketing_sms", Record = new SubscriptionTypeRecord { Desc = "Marketing SMS" } }
      },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId2, Id = "marketing_emails", Record = new SubscriptionTypeRecord { Desc = "Marketing promotions" } },
        new() { ProjectId = projectId2, Id = "marketing_sms", Record = new SubscriptionTypeRecord { Desc = "Marketing SMS" } }
      }
    };

    yield return new object[]
    {
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeSearch { Limit = 20, Skip = 0 },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId1, Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Product updates" } },
        new() { ProjectId = projectId1, Id = "alerts", Record = new SubscriptionTypeRecord { Desc = "System alerts" } }
      },
      new List<SubscriptionTypePrincipal>
      {
        new() { ProjectId = projectId1, Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Product updates" } },
        new() { ProjectId = projectId1, Id = "alerts", Record = new SubscriptionTypeRecord { Desc = "System alerts" } }
      }
    };
  }

  [Theory]
  [MemberData(nameof(GetValidTestData))]
  public async Task Get_WithExistingId_ShouldReturnSubscriptionType(
    Guid projectId,
    string id,
    global::Domain.Marketing.SubscriptionType.SubscriptionType? mockRepoResult,
    global::Domain.Marketing.SubscriptionType.SubscriptionType? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<global::Domain.Marketing.SubscriptionType.SubscriptionType?>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Get(projectId, id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<global::Domain.Marketing.SubscriptionType.SubscriptionType?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }

  public static IEnumerable<object[]> GetValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("f7218ea9-f626-4c01-8bbb-e56bf206e2c6"),
      "newsletter",
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("f7218ea9-f626-4c01-8bbb-e56bf206e2c6"), Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Weekly newsletter" } },
        Count = 100
      },
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("f7218ea9-f626-4c01-8bbb-e56bf206e2c6"), Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Weekly newsletter" } },
        Count = 100
      }
    };

    yield return new object[]
    {
      Guid.Parse("71ffc343-1891-4764-be57-728d9cc57794"),
      "marketing",
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("71ffc343-1891-4764-be57-728d9cc57794"), Id = "marketing", Record = new SubscriptionTypeRecord { Desc = "Marketing emails" } },
        Count = 50
      },
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("71ffc343-1891-4764-be57-728d9cc57794"), Id = "marketing", Record = new SubscriptionTypeRecord { Desc = "Marketing emails" } },
        Count = 50
      }
    };

    yield return new object[]
    {
      Guid.Parse("f0e57aeb-a54e-4d98-9148-9ab7f1c37296"),
      "updates",
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("f0e57aeb-a54e-4d98-9148-9ab7f1c37296"), Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Product updates" } },
        Count = 0
      },
      new global::Domain.Marketing.SubscriptionType.SubscriptionType
      {
        Principal = new SubscriptionTypePrincipal { ProjectId = Guid.Parse("f0e57aeb-a54e-4d98-9148-9ab7f1c37296"), Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Product updates" } },
        Count = 0
      }
    };
  }

  [Theory]
  [MemberData(nameof(CreateValidTestData))]
  public async Task Create_WithValidInput_ShouldReturnSubscriptionTypePrincipal(
    Guid projectId,
    string id,
    SubscriptionTypeRecord input,
    SubscriptionTypePrincipal? mockRepoResult,
    SubscriptionTypePrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    mockRepo.Setup(x => x.Create(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionTypeRecord>()))
      .ReturnsAsync(new Result<SubscriptionTypePrincipal?>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Create(projectId, id, input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<SubscriptionTypePrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Create(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionTypeRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> CreateValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("1920030e-134c-4887-b7ae-b4ccebff729a"),
      "new_newsletter",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "New newsletter type" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("1920030e-134c-4887-b7ae-b4ccebff729a"), Id = "new_newsletter", Record = new SubscriptionTypeRecord { Desc = "New newsletter type" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("1920030e-134c-4887-b7ae-b4ccebff729a"), Id = "new_newsletter", Record = new SubscriptionTypeRecord { Desc = "New newsletter type" } }
    };

    yield return new object[]
    {
      Guid.Parse("9851a3ba-9dd4-4586-92c3-5f83f020fa14"),
      "alerts",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "Alert notifications" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("9851a3ba-9dd4-4586-92c3-5f83f020fa14"), Id = "alerts", Record = new SubscriptionTypeRecord { Desc = "Alert notifications" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("9851a3ba-9dd4-4586-92c3-5f83f020fa14"), Id = "alerts", Record = new SubscriptionTypeRecord { Desc = "Alert notifications" } }
    };

    yield return new object[]
    {
      Guid.Parse("705b56d9-0b19-4fb9-aa7b-682f7e55f191"),
      "promotions",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "Special promotions" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("705b56d9-0b19-4fb9-aa7b-682f7e55f191"), Id = "promotions", Record = new SubscriptionTypeRecord { Desc = "Special promotions" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("705b56d9-0b19-4fb9-aa7b-682f7e55f191"), Id = "promotions", Record = new SubscriptionTypeRecord { Desc = "Special promotions" } }
    };
  }

  [Theory]
  [MemberData(nameof(UpdateValidTestData))]
  public async Task Update_WithValidInput_ShouldReturnUpdatedSubscriptionTypePrincipal(
    Guid projectId,
    string id,
    SubscriptionTypeRecord input,
    SubscriptionTypePrincipal? mockRepoResult,
    SubscriptionTypePrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    mockRepo.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionTypeRecord>()))
      .ReturnsAsync(new Result<SubscriptionTypePrincipal?>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Update(projectId, id, input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<SubscriptionTypePrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<SubscriptionTypeRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> UpdateValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("4b5da581-f377-46fb-a1dc-5971e9522720"),
      "newsletter",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "Updated newsletter description" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("4b5da581-f377-46fb-a1dc-5971e9522720"), Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Updated newsletter description" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("4b5da581-f377-46fb-a1dc-5971e9522720"), Id = "newsletter", Record = new SubscriptionTypeRecord { Desc = "Updated newsletter description" } }
    };

    yield return new object[]
    {
      Guid.Parse("6a276632-bcf9-4bc6-ad95-bf0c1a61c531"),
      "marketing",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "Updated marketing description" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("6a276632-bcf9-4bc6-ad95-bf0c1a61c531"), Id = "marketing", Record = new SubscriptionTypeRecord { Desc = "Updated marketing description" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("6a276632-bcf9-4bc6-ad95-bf0c1a61c531"), Id = "marketing", Record = new SubscriptionTypeRecord { Desc = "Updated marketing description" } }
    };

    yield return new object[]
    {
      Guid.Parse("6bedfff9-7cde-4c59-85ac-65f007da4567"),
      "updates",
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypeRecord { Desc = "Updated product updates" },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("6bedfff9-7cde-4c59-85ac-65f007da4567"), Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Updated product updates" } },
      new global::Domain.Marketing.SubscriptionType.SubscriptionTypePrincipal { ProjectId = Guid.Parse("6bedfff9-7cde-4c59-85ac-65f007da4567"), Id = "updates", Record = new SubscriptionTypeRecord { Desc = "Updated product updates" } }
    };
  }

  [Theory]
  [InlineData("949a8248-8e83-4478-ad23-06acd88a8cf3", "newsletter")]
  [InlineData("7968738c-3220-45cb-9842-6e62b746042d", "marketing")]
  [InlineData("05c69a07-a36f-48df-8dee-04ae4ed6c443", "updates")]
  public async Task Delete_WithExistingId_ShouldReturnUnit(string projectIdStr, string id)
  {
    // Arrange
    var projectId = Guid.Parse(projectIdStr);
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    Unit? mockRepoResult = new Unit();
    Unit? expectedOutput = new Unit();
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Delete(projectId, id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("9790c196-25da-49b1-b84f-b45462db1fe7", "nonexistent")]
  [InlineData("4a032a79-8971-4235-b476-088ddfcf47b2", "unknown")]
  [InlineData("2bbefe94-92a0-454f-adbe-085448ed7364", "fake")]
  public async Task Delete_WithNonExistingId_ShouldReturnNull(string projectIdStr, string id)
  {
    // Arrange
    var projectId = Guid.Parse(projectIdStr);
    var mockRepo = new Mock<ISubscriptionTypeRepository>();
    Unit? mockRepoResult = null;
    Unit? expectedOutput = null;
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new SubscriptionTypeService(mockRepo.Object);

    // Act
    var actual = await service.Delete(projectId, id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
  }
}
