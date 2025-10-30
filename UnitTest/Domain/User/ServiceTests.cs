using CarboxylicLithium;
using Domain;
using Domain.User;
using FluentAssertions;
using Moq;

namespace UnitTest.Domain.User;

public class ServiceTests
{
  // ========================================
  // Search Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(
    UserSearch input,
    IEnumerable<UserPrincipal> mockRepoResult,
    IEnumerable<UserPrincipal> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Search(It.IsAny<UserSearch>()))
      .ReturnsAsync(new Result<IEnumerable<UserPrincipal>>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<UserPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<UserSearch>()), Times.Once);
  }

  [Theory]
  [InlineData("10570549-b926-419c-9967-f259165cdc68")]
  [InlineData("504cf036-efbf-4e50-a975-0b11000fd83c")]
  [InlineData("8d87d63e-e63f-41e4-a49a-98294841f746")]
  public async Task Search_WithNoResults_ShouldReturnEmptyList(string searchId)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    List<UserPrincipal> mockRepoResult = [];
    List<UserPrincipal> expectedOutput = [];
    mockRepo.Setup(x => x.Search(It.IsAny<UserSearch>()))
      .ReturnsAsync(new Result<IEnumerable<UserPrincipal>>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);
    var input = new UserSearch { Id = searchId, Limit = 100, Skip = 0 };

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<UserPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<UserSearch>()), Times.Once);
  }

  public static IEnumerable<object[]> ValidSearchTestData()
  {
    yield return new object[]
    {
      new global::Domain.User.UserSearch { Username = "alice", Limit = 100, Skip = 0 },
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
      new global::Domain.User.UserSearch { Email = "bob@example.com", Limit = 50, Skip = 0 },
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
      new global::Domain.User.UserSearch { Active = true, Limit = 20, Skip = 0 },
      new List<UserPrincipal>
      {
        new()
        {
          Id = "b2582179-9c67-47de-a071-876d80568f94",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["admin"]
          }
        },
        new()
        {
          Id = "273fda0a-efd4-43b8-915e-81f8240a84ae",
          Record = new UserRecord
          {
            Username = "diana",
            Email = "diana@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["read", "write", "delete"]
          }
        }
      },
      new List<UserPrincipal>
      {
        new()
        {
          Id = "b2582179-9c67-47de-a071-876d80568f94",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["admin"]
          }
        },
        new()
        {
          Id = "273fda0a-efd4-43b8-915e-81f8240a84ae",
          Record = new UserRecord
          {
            Username = "diana",
            Email = "diana@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["read", "write", "delete"]
          }
        }
      }
    };
  }

  // ========================================
  // GetById Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(GetByIdValidTestData))]
  public async Task GetById_WithExistingId_ShouldReturnUser(
    string id,
    global::Domain.User.User? mockRepoResult,
    global::Domain.User.User? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.GetById(It.IsAny<string>()))
      .ReturnsAsync(new Result<global::Domain.User.User?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetById(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<global::Domain.User.User?>(expectedOutput));
    mockRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("c149a9b3-4af4-4b21-8d07-43d7f090c8fb")]
  [InlineData("889ba64b-6bc7-49f3-a712-0f818fb2fbe0")]
  [InlineData("8fca6f03-dc03-4fa4-af51-4c9f100c3bee")]
  public async Task GetById_WithNonExistingId_ShouldReturnNull(string id)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    global::Domain.User.User? mockRepoResult = null;
    global::Domain.User.User? expectedOutput = null;
    mockRepo.Setup(x => x.GetById(It.IsAny<string>()))
      .ReturnsAsync(new Result<global::Domain.User.User?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetById(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<global::Domain.User.User?>(expectedOutput));
    mockRepo.Verify(x => x.GetById(It.IsAny<string>()), Times.Once);
  }

  public static IEnumerable<object[]> GetByIdValidTestData()
  {
    yield return new object[]
    {
      "3e85e167-ab1b-4fe8-96d4-b30a2428a76c",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "3e85e167-ab1b-4fe8-96d4-b30a2428a76c",
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
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "3e85e167-ab1b-4fe8-96d4-b30a2428a76c",
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
      "fa407d2e-11db-465b-9e27-1999a494ca5f",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "fa407d2e-11db-465b-9e27-1999a494ca5f",
          Record = new UserRecord
          {
            Username = "bob",
            Email = "bob@example.com",
            EmailVerified = false,
            Active = false,
            Scopes = ["read"]
          }
        }
      },
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "fa407d2e-11db-465b-9e27-1999a494ca5f",
          Record = new UserRecord
          {
            Username = "bob",
            Email = "bob@example.com",
            EmailVerified = false,
            Active = false,
            Scopes = ["read"]
          }
        }
      }
    };

    yield return new object[]
    {
      "bbfbb892-47e9-4729-b15c-3d5f9cef4bf5",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "bbfbb892-47e9-4729-b15c-3d5f9cef4bf5",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["admin", "write"]
          }
        }
      },
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "bbfbb892-47e9-4729-b15c-3d5f9cef4bf5",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = true,
            Scopes = ["admin", "write"]
          }
        }
      }
    };
  }

  // ========================================
  // GetByUsername Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(GetByUsernameValidTestData))]
  public async Task GetByUsername_WithExistingUsername_ShouldReturnUser(
    string username,
    global::Domain.User.User? mockRepoResult,
    global::Domain.User.User? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.GetByUsername(It.IsAny<string>()))
      .ReturnsAsync(new Result<global::Domain.User.User?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetByUsername(username);

    // Assert
    actual.Should().BeEquivalentTo(new Result<global::Domain.User.User?>(expectedOutput));
    mockRepo.Verify(x => x.GetByUsername(It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("nonexistent")]
  [InlineData("unknown_user")]
  [InlineData("test123")]
  public async Task GetByUsername_WithNonExistingUsername_ShouldReturnNull(string username)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    global::Domain.User.User? mockRepoResult = null;
    global::Domain.User.User? expectedOutput = null;
    mockRepo.Setup(x => x.GetByUsername(It.IsAny<string>()))
      .ReturnsAsync(new Result<global::Domain.User.User?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetByUsername(username);

    // Assert
    actual.Should().BeEquivalentTo(new Result<global::Domain.User.User?>(expectedOutput));
    mockRepo.Verify(x => x.GetByUsername(It.IsAny<string>()), Times.Once);
  }

  public static IEnumerable<object[]> GetByUsernameValidTestData()
  {
    yield return new object[]
    {
      "alice",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "d9add42e-aeed-4f88-961e-62744c737178",
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
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "d9add42e-aeed-4f88-961e-62744c737178",
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
      "bob",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "7aaa8765-78c6-416a-a865-d0fb27acbbd1",
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
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "7aaa8765-78c6-416a-a865-d0fb27acbbd1",
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
      "charlie",
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "5f8a1bea-c2f8-479f-9795-d9571d977371",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = false,
            Scopes = ["admin"]
          }
        }
      },
      new global::Domain.User.User
      {
        Principal = new UserPrincipal
        {
          Id = "5f8a1bea-c2f8-479f-9795-d9571d977371",
          Record = new UserRecord
          {
            Username = "charlie",
            Email = "charlie@example.com",
            EmailVerified = true,
            Active = false,
            Scopes = ["admin"]
          }
        }
      }
    };
  }

  // ========================================
  // Create Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(CreateValidTestData))]
  public async Task Create_WithValidInput_ShouldReturnUserPrincipal(
    string id,
    UserRecord input,
    UserPrincipal mockRepoResult,
    UserPrincipal expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<UserRecord>()))
      .ReturnsAsync(new Result<UserPrincipal>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<UserPrincipal>>>>()))
      .Returns<Func<Task<Result<UserPrincipal>>>>(func => func());
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Create(id, input, null);

    // Assert
    actual.Should().BeEquivalentTo(new Result<UserPrincipal>(expectedOutput));
    mockRepo.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<UserRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> CreateValidTestData()
  {
    yield return new object[]
    {
      "5710e904-a05b-4eab-912e-c02ffb41d83c",
      new global::Domain.User.UserRecord
      {
        Username = "newuser1",
        Email = "newuser1@example.com",
        EmailVerified = false,
        Active = true,
        Scopes = ["read"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "5710e904-a05b-4eab-912e-c02ffb41d83c",
        Record = new UserRecord
        {
          Username = "newuser1",
          Email = "newuser1@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["read"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "5710e904-a05b-4eab-912e-c02ffb41d83c",
        Record = new UserRecord
        {
          Username = "newuser1",
          Email = "newuser1@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["read"]
        }
      }
    };

    yield return new object[]
    {
      "2729805a-adad-43af-814d-22d0f7407edf",
      new global::Domain.User.UserRecord
      {
        Username = "admin_user",
        Email = "admin@example.com",
        EmailVerified = true,
        Active = true,
        Scopes = ["admin", "read", "write"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "2729805a-adad-43af-814d-22d0f7407edf",
        Record = new UserRecord
        {
          Username = "admin_user",
          Email = "admin@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["admin", "read", "write"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "2729805a-adad-43af-814d-22d0f7407edf",
        Record = new UserRecord
        {
          Username = "admin_user",
          Email = "admin@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["admin", "read", "write"]
        }
      }
    };

    yield return new object[]
    {
      "9c727e0b-3c8f-4a8b-a1bd-03ba8e3c7b88",
      new global::Domain.User.UserRecord
      {
        Username = "test_user",
        Email = "test@example.com",
        EmailVerified = false,
        Active = false,
        Scopes = ["read"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "9c727e0b-3c8f-4a8b-a1bd-03ba8e3c7b88",
        Record = new UserRecord
        {
          Username = "test_user",
          Email = "test@example.com",
          EmailVerified = false,
          Active = false,
          Scopes = ["read"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "9c727e0b-3c8f-4a8b-a1bd-03ba8e3c7b88",
        Record = new UserRecord
        {
          Username = "test_user",
          Email = "test@example.com",
          EmailVerified = false,
          Active = false,
          Scopes = ["read"]
        }
      }
    };
  }

  // ========================================
  // Update Method Tests
  // ========================================

  [Theory]
  [MemberData(nameof(UpdateValidTestData))]
  public async Task Update_WithValidInput_ShouldReturnUpdatedUserPrincipal(
    string id,
    UserRecord input,
    UserPrincipal? mockRepoResult,
    UserPrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<UserRecord>()))
      .ReturnsAsync(new Result<UserPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<UserPrincipal?>>>>()))
      .Returns<Func<Task<Result<UserPrincipal?>>>>(func => func());
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Update(id, input, null);

    // Assert
    actual.Should().BeEquivalentTo(new Result<UserPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<UserRecord>()), Times.Once);
  }

  [Theory]
  [InlineData("5f74baf2-c0c1-49d5-b21d-3a489e63c636")]
  [InlineData("adf22632-407e-49ca-843b-5d056b211c83")]
  [InlineData("17b7a990-12b0-402a-9bac-5a481dc41cae")]
  public async Task Update_WithNonExistingId_ShouldReturnNull(string id)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    UserPrincipal? mockRepoResult = null;
    UserPrincipal? expectedOutput = null;
    var input = new UserRecord
    {
      Username = "updated",
      Email = "updated@example.com",
      EmailVerified = true,
      Active = true,
      Scopes = ["read"]
    };
    mockRepo.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<UserRecord>()))
      .ReturnsAsync(new Result<UserPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<UserPrincipal?>>>>()))
      .Returns<Func<Task<Result<UserPrincipal?>>>>(func => func());
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Update(id, input, null);

    // Assert
    actual.Should().BeEquivalentTo(new Result<UserPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<UserRecord>()), Times.Once);
  }

  public static IEnumerable<object[]> UpdateValidTestData()
  {
    yield return new object[]
    {
      "fa10e023-dc86-47f9-a9c3-b46dc3175c62",
      new global::Domain.User.UserRecord
      {
        Username = "alice_updated",
        Email = "alice_updated@example.com",
        EmailVerified = true,
        Active = true,
        Scopes = ["read", "write", "admin"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "fa10e023-dc86-47f9-a9c3-b46dc3175c62",
        Record = new UserRecord
        {
          Username = "alice_updated",
          Email = "alice_updated@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write", "admin"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "fa10e023-dc86-47f9-a9c3-b46dc3175c62",
        Record = new UserRecord
        {
          Username = "alice_updated",
          Email = "alice_updated@example.com",
          EmailVerified = true,
          Active = true,
          Scopes = ["read", "write", "admin"]
        }
      }
    };

    yield return new object[]
    {
      "e8225eb7-1a45-4187-bac5-b4beaf744127",
      new global::Domain.User.UserRecord
      {
        Username = "bob",
        Email = "bob_new@example.com",
        EmailVerified = true,
        Active = false,
        Scopes = ["read"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "e8225eb7-1a45-4187-bac5-b4beaf744127",
        Record = new UserRecord
        {
          Username = "bob",
          Email = "bob_new@example.com",
          EmailVerified = true,
          Active = false,
          Scopes = ["read"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "e8225eb7-1a45-4187-bac5-b4beaf744127",
        Record = new UserRecord
        {
          Username = "bob",
          Email = "bob_new@example.com",
          EmailVerified = true,
          Active = false,
          Scopes = ["read"]
        }
      }
    };

    yield return new object[]
    {
      "030c99bd-44a0-4793-b4d8-4dda5b29a7c1",
      new global::Domain.User.UserRecord
      {
        Username = "charlie",
        Email = "charlie@example.com",
        EmailVerified = false,
        Active = true,
        Scopes = ["admin"]
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "030c99bd-44a0-4793-b4d8-4dda5b29a7c1",
        Record = new UserRecord
        {
          Username = "charlie",
          Email = "charlie@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["admin"]
        }
      },
      new global::Domain.User.UserPrincipal
      {
        Id = "030c99bd-44a0-4793-b4d8-4dda5b29a7c1",
        Record = new UserRecord
        {
          Username = "charlie",
          Email = "charlie@example.com",
          EmailVerified = false,
          Active = true,
          Scopes = ["admin"]
        }
      }
    };
  }

  // ========================================
  // Delete Method Tests
  // ========================================

  [Theory]
  [InlineData("ac6c0d15-3e52-4a05-ad5c-48c70728bb96")]
  [InlineData("83892187-d855-4e6e-b0e0-a92d55708108")]
  [InlineData("c8bbf1ed-5cb2-4b8d-9385-5a61c8fbd8fd")]
  public async Task Delete_WithExistingId_ShouldReturnUnit(string id)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = new Unit();
    Unit? expectedOutput = new Unit();
    mockRepo.Setup(x => x.Delete(It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
  }

  [Theory]
  [InlineData("01214c15-0efe-45c3-8f2c-e82a5f4e496c")]
  [InlineData("26a7831d-78b7-4222-ac12-541c5f61f6ab")]
  [InlineData("5c29c085-2ab1-4533-a102-445ec2d33001")]
  public async Task Delete_WithNonExistingId_ShouldReturnNull(string id)
  {
    // Arrange
    var mockRepo = new Mock<IUserRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = null;
    Unit? expectedOutput = null;
    mockRepo.Setup(x => x.Delete(It.IsAny<string>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new UserService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
  }
}
