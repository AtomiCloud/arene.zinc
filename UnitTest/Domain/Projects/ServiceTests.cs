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
          Id = Guid.Parse("a7055c7a-8d94-4229-87a4-3fe21ba56042"),
          Record = new ProjectRecord { Name = "Gamma", Open = true }
        },
        new()
        {
          Id = Guid.Parse("d77c7ca7-93c3-42b8-af91-58a32dc4cdea"),
          Record = new ProjectRecord { Name = "Delta", Open = false }
        },
        new()
        {
          Id = Guid.Parse("274ab829-b787-4ca1-a059-b5eb0c98dc71"),
          Record = new ProjectRecord { Name = "Epsilon", Open = true }
        }
      },
      new List<ProjectPrincipal>
      {
        new()
        {
          Id = Guid.Parse("a7055c7a-8d94-4229-87a4-3fe21ba56042"),
          Record = new ProjectRecord { Name = "Gamma", Open = true }
        },
        new()
        {
          Id = Guid.Parse("d77c7ca7-93c3-42b8-af91-58a32dc4cdea"),
          Record = new ProjectRecord { Name = "Delta", Open = false }
        },
        new()
        {
          Id = Guid.Parse("274ab829-b787-4ca1-a059-b5eb0c98dc71"),
          Record = new ProjectRecord { Name = "Epsilon", Open = true }
        }
      }
    };
  }

  // ========================================
  // Get Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(GetValidTestData))]
  public async Task Get_WithExistingId_ShouldReturnProject(
    Guid id,
    Project? mockRepoResult,
    Project? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Project?>(mockRepoResult));
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Get(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Project?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
  }

  [Theory]
  [InlineData("23870857-32d9-4dda-a95e-63f09e3c9eab")]
  [InlineData("f7a3cd07-b11c-4b68-8032-847b04e7b5fb")]
  [InlineData("6d568059-9423-4388-8cae-1527a1d062a8")]
  public async Task Get_WithNonExistingId_ShouldReturnNull(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Project? mockRepoResult = null;
    Project? expectedOutput = null;
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Project?>(mockRepoResult));
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Get(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Project?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
  }

  public static IEnumerable<object[]> GetValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("48418dec-3519-41a6-aa18-5c3b8fb8e701"),
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("48418dec-3519-41a6-aa18-5c3b8fb8e701"),
          Record = new ProjectRecord { Name = "Project Alpha", Open = true }
        },
        SubscriberCount = 10
      },
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("48418dec-3519-41a6-aa18-5c3b8fb8e701"),
          Record = new ProjectRecord { Name = "Project Alpha", Open = true }
        },
        SubscriberCount = 10
      }
    };

    yield return new object[]
    {
      Guid.Parse("a4692828-0fb9-45d9-bea8-8e4258dde36e"),
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("a4692828-0fb9-45d9-bea8-8e4258dde36e"),
          Record = new ProjectRecord { Name = "Project Beta", Open = false }
        },
        SubscriberCount = 0
      },
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("a4692828-0fb9-45d9-bea8-8e4258dde36e"),
          Record = new ProjectRecord { Name = "Project Beta", Open = false }
        },
        SubscriberCount = 0
      }
    };

    yield return new object[]
    {
      Guid.Parse("be76e6ec-0c4f-4b41-ba15-628228802a0e"),
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("be76e6ec-0c4f-4b41-ba15-628228802a0e"),
          Record = new ProjectRecord { Name = "Project Gamma", Open = true }
        },
        SubscriberCount = 25
      },
      new Project
      {
        Principal = new ProjectPrincipal
        {
          Id = Guid.Parse("be76e6ec-0c4f-4b41-ba15-628228802a0e"),
          Record = new ProjectRecord { Name = "Project Gamma", Open = true }
        },
        SubscriberCount = 25
      }
    };
  }

  // ========================================
  // Create Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(CreateValidTestData))]
  public async Task Create_WithValidInput_ShouldReturnProjectPrincipal(
    ProjectRecord input,
    ProjectPrincipal mockRepoResult,
    ProjectPrincipal expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Create(It.IsAny<ProjectRecord>()))
      .ReturnsAsync(new Result<ProjectPrincipal>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal>>>>()))
      .Returns<Func<Task<Result<ProjectPrincipal>>>>(func => func());
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Create(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal>(expectedOutput));
    mockRepo.Verify(x => x.Create(It.IsAny<ProjectRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> CreateValidTestData()
  {
    yield return new object[]
    {
      new ProjectRecord { Name = "New Project 1", Open = true },
      new ProjectPrincipal
      {
        Id = Guid.Parse("d4f41518-728e-4091-8844-8467101806e8"),
        Record = new ProjectRecord { Name = "New Project 1", Open = true }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("d4f41518-728e-4091-8844-8467101806e8"),
        Record = new ProjectRecord { Name = "New Project 1", Open = true }
      }
    };

    yield return new object[]
    {
      new ProjectRecord { Name = "New Project 2", Open = false },
      new ProjectPrincipal
      {
        Id = Guid.Parse("39c1430c-7a55-4dd8-844b-f8307ebf5b55"),
        Record = new ProjectRecord { Name = "New Project 2", Open = false }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("39c1430c-7a55-4dd8-844b-f8307ebf5b55"),
        Record = new ProjectRecord { Name = "New Project 2", Open = false }
      }
    };

    yield return new object[]
    {
      new ProjectRecord { Name = "Test Project", Open = true },
      new ProjectPrincipal
      {
        Id = Guid.Parse("87a7f105-ad69-42f1-ba7a-a96529ed982a"),
        Record = new ProjectRecord { Name = "Test Project", Open = true }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("87a7f105-ad69-42f1-ba7a-a96529ed982a"),
        Record = new ProjectRecord { Name = "Test Project", Open = true }
      }
    };
  }

  // ========================================
  // Update Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(UpdateValidTestData))]
  public async Task Update_WithValidInput_ShouldReturnUpdatedProjectPrincipal(
    Guid id,
    ProjectRecord input,
    ProjectPrincipal? mockRepoResult,
    ProjectPrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<ProjectRecord>()))
      .ReturnsAsync(new Result<ProjectPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal?>>>>()))
      .Returns<Func<Task<Result<ProjectPrincipal?>>>>(func => func());
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Update(id, input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<ProjectRecord>()), Times.Once);
  }

  [Theory]
  [InlineData("846a7534-d78e-45d5-a643-bf7ff396b852")]
  [InlineData("2efe644f-e55c-4f88-aa18-65b688aaa2e4")]
  [InlineData("7a273940-2b05-4c98-a992-320a4ad5e46a")]
  public async Task Update_WithNonExistingId_ShouldReturnNull(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    ProjectPrincipal? mockRepoResult = null;
    ProjectPrincipal? expectedOutput = null;
    var input = new ProjectRecord { Name = "Updated", Open = true };
    mockRepo.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<ProjectRecord>()))
      .ReturnsAsync(new Result<ProjectPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<ProjectPrincipal?>>>>()))
      .Returns<Func<Task<Result<ProjectPrincipal?>>>>(func => func());
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Update(id, input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<ProjectPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<ProjectRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> UpdateValidTestData()
  {
    yield return new object[]
    {
      Guid.Parse("26553f6c-a1ae-48b2-a902-42a89725542a"),
      new ProjectRecord { Name = "Updated Project 1", Open = true },
      new ProjectPrincipal
      {
        Id = Guid.Parse("26553f6c-a1ae-48b2-a902-42a89725542a"),
        Record = new ProjectRecord { Name = "Updated Project 1", Open = true }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("26553f6c-a1ae-48b2-a902-42a89725542a"),
        Record = new ProjectRecord { Name = "Updated Project 1", Open = true }
      }
    };

    yield return new object[]
    {
      Guid.Parse("f5ae63b0-d32e-498c-9dcc-cc69862d2319"),
      new ProjectRecord { Name = "Updated Project 2", Open = false },
      new ProjectPrincipal
      {
        Id = Guid.Parse("f5ae63b0-d32e-498c-9dcc-cc69862d2319"),
        Record = new ProjectRecord { Name = "Updated Project 2", Open = false }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("f5ae63b0-d32e-498c-9dcc-cc69862d2319"),
        Record = new ProjectRecord { Name = "Updated Project 2", Open = false }
      }
    };

    yield return new object[]
    {
      Guid.Parse("c41ddecf-1b7d-4834-a330-28a03ed704cb"),
      new ProjectRecord { Name = "Updated Project 3", Open = true },
      new ProjectPrincipal
      {
        Id = Guid.Parse("c41ddecf-1b7d-4834-a330-28a03ed704cb"),
        Record = new ProjectRecord { Name = "Updated Project 3", Open = true }
      },
      new ProjectPrincipal
      {
        Id = Guid.Parse("c41ddecf-1b7d-4834-a330-28a03ed704cb"),
        Record = new ProjectRecord { Name = "Updated Project 3", Open = true }
      }
    };
  }

  // ========================================
  // Delete Method Tests
  // ========================================

  [Theory]
  [InlineData("9ddc65be-2890-4c85-b4fe-280418484a24")]
  [InlineData("d914f856-3e38-40ac-80b3-c2cf5dc6a125")]
  [InlineData("87fff8d1-18d4-41d0-8dcf-4be6675b8d09")]
  public async Task Delete_WithExistingId_ShouldReturnUnit(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = new Unit();
    Unit? expectedOutput = new Unit();
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<Unit?>>>>()))
      .Returns<Func<Task<Result<Unit?>>>>(func => func());
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
  }

  [Theory]
  [InlineData("ad1b3211-8eed-42fd-9b91-a8249c9fdb95")]
  [InlineData("2d36793c-1b3b-4060-840d-434954e76daf")]
  [InlineData("dc1cc8e0-e2b4-430c-8bce-ca51a30798a3")]
  public async Task Delete_WithNonExistingId_ShouldReturnNull(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<IProjectRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = null;
    Unit? expectedOutput = null;
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<Unit?>>>>()))
      .Returns<Func<Task<Result<Unit?>>>>(func => func());
    var service = new ProjectService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
  }
}
