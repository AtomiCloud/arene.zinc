using CarboxylicLithium;
using Domain;
using Domain.Legal;
using FluentAssertions;
using Moq;

namespace UnitTest.Domain.Legal;

public class ServiceTests
{
  [Theory]
  [MemberData(nameof(ValidSearchTestData))]
  public async Task Search_WithValidInput_ShouldReturnResults(
    LegalDocumentSearch input,
    IEnumerable<LegalDocumentPrincipal> mockRepoResult,
    IEnumerable<LegalDocumentPrincipal> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Search(It.IsAny<LegalDocumentSearch>()))
      .ReturnsAsync(new Result<IEnumerable<LegalDocumentPrincipal>>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Search(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<LegalDocumentPrincipal>>(expectedOutput));
    mockRepo.Verify(x => x.Search(It.IsAny<LegalDocumentSearch>()), Times.Once);
  }

  public static IEnumerable<object[]> ValidSearchTestData()
  {
    var docId1 = Guid.Parse("d9c89343-2ef1-4c9e-bad3-c4cc68a52c16");
    var docId2 = Guid.Parse("c8afb703-2a09-4e5a-885b-9f75eee35835");
    var docId3 = Guid.Parse("cf5f4b89-72b0-42db-84c9-2b4b7eeaafc3");

    yield return new object[]
    {
      new LegalDocumentSearch { Type = LegalDocumentType.PrivacyPolicy, Limit = 100, Skip = 0 },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy v1",
            Content = "Privacy policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2024, 12, 1)
          }
        }
      },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy v1",
            Content = "Privacy policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2024, 12, 1)
          }
        }
      }
    };

    yield return new object[]
    {
      new LegalDocumentSearch { Status = LegalDocumentStatus.Draft, Limit = 50, Skip = 0 },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Draft,
            Title = "Terms of Use Draft",
            Content = "Terms content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2025, 1, 15),
            UpdatedAt = new DateTime(2025, 1, 15)
          }
        },
        new()
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Draft,
            Title = "Refund Policy Draft",
            Content = "Refund policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 20),
            UpdatedAt = new DateTime(2025, 1, 20)
          }
        }
      },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Draft,
            Title = "Terms of Use Draft",
            Content = "Terms content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2025, 1, 15),
            UpdatedAt = new DateTime(2025, 1, 15)
          }
        },
        new()
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Draft,
            Title = "Refund Policy Draft",
            Content = "Refund policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 20),
            UpdatedAt = new DateTime(2025, 1, 20)
          }
        }
      }
    };

    yield return new object[]
    {
      new LegalDocumentSearch { Limit = 20, Skip = 0 },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy",
            Content = "Content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2025, 1, 1)
          }
        }
      },
      new List<LegalDocumentPrincipal>
      {
        new()
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy",
            Content = "Content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2025, 1, 1)
          }
        }
      }
    };
  }

  [Theory]
  [MemberData(nameof(GetValidTestData))]
  public async Task Get_WithExistingId_ShouldReturnLegalDocument(
    Guid id,
    LegalDocument? mockRepoResult,
    LegalDocument? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Get(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<LegalDocument?>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Get(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocument?>(expectedOutput));
    mockRepo.Verify(x => x.Get(It.IsAny<Guid>()), Times.Once);
  }

  public static IEnumerable<object[]> GetValidTestData()
  {
    var docId1 = Guid.Parse("0c8d9c60-0bb3-466d-a78e-c09afc22edc8");
    var versionId1 = Guid.Parse("03cd3beb-7f8d-4c22-b875-4a2050d4b9eb");

    yield return new object[]
    {
      docId1,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy v2",
            Content = "Updated privacy policy",
            Version = 2,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2025, 1, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 2,
          Title = "Privacy Policy v2",
          Content = "Updated privacy policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2025, 1, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Privacy Policy v2",
            Content = "Updated privacy policy",
            Version = 2,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2025, 1, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 2,
          Title = "Privacy Policy v2",
          Content = "Updated privacy policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2025, 1, 1)
        },
        HistoricalVersions = []
      }
    };

    var docId2 = Guid.Parse("b2e6a0ca-f3f6-4b5d-8c41-64d1d82cd394");
    var versionId2 = Guid.Parse("a318f0d1-85cc-4879-9ef7-b32f3dad21d3");
    var historicalVersionId = Guid.Parse("e1bdef43-2fa8-47d7-b8d0-9c04ea2e5b6f");

    yield return new object[]
    {
      docId2,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Terms of Use v3",
            Content = "Latest terms content",
            Version = 3,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 3,
          Title = "Terms of Use v3",
          Content = "Latest terms content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions =
        [
          new()
          {
            Id = historicalVersionId,
            DocumentId = docId2,
            Version = 2,
            Title = "Terms of Use v2",
            Content = "Previous terms content",
            Status = LegalDocumentStatus.Archived,
            EffectiveDate = new DateTime(2024, 11, 1),
            CreatedAt = new DateTime(2024, 11, 1)
          }
        ]
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Terms of Use v3",
            Content = "Latest terms content",
            Version = 3,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 3,
          Title = "Terms of Use v3",
          Content = "Latest terms content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions =
        [
          new()
          {
            Id = historicalVersionId,
            DocumentId = docId2,
            Version = 2,
            Title = "Terms of Use v2",
            Content = "Previous terms content",
            Status = LegalDocumentStatus.Archived,
            EffectiveDate = new DateTime(2024, 11, 1),
            CreatedAt = new DateTime(2024, 11, 1)
          }
        ]
      }
    };

    var docId3 = Guid.Parse("3b0e5f5e-d7a5-4b8c-9234-fd85ea3e8c76");
    var versionId3 = Guid.Parse("eaa8b6e0-dcd7-4e5f-bfa2-ad2e1c9f5a48");

    yield return new object[]
    {
      docId3,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Draft,
            Title = "Refund Policy v1",
            Content = "Refund policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 15),
            UpdatedAt = new DateTime(2025, 1, 15)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy v1",
          Content = "Refund policy content",
          Status = LegalDocumentStatus.Draft,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 1, 15)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Draft,
            Title = "Refund Policy v1",
            Content = "Refund policy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 15),
            UpdatedAt = new DateTime(2025, 1, 15)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy v1",
          Content = "Refund policy content",
          Status = LegalDocumentStatus.Draft,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 1, 15)
        },
        HistoricalVersions = []
      }
    };
  }

  [Theory]
  [MemberData(nameof(GetByTypeValidTestData))]
  public async Task GetByType_WithValidType_ShouldReturnLegalDocument(
    LegalDocumentType type,
    LegalDocument? mockRepoResult,
    LegalDocument? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.GetByType(It.IsAny<LegalDocumentType>()))
      .ReturnsAsync(new Result<LegalDocument?>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetByType(type);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocument?>(expectedOutput));
    mockRepo.Verify(x => x.GetByType(It.IsAny<LegalDocumentType>()), Times.Once);
  }

  public static IEnumerable<object[]> GetByTypeValidTestData()
  {
    var docId1 = Guid.Parse("ff3c0a5e-59f3-4d7b-921e-8f44bd7e2c91");
    var versionId1 = Guid.Parse("a9b5c3e7-2d4f-4a8e-b1c5-7e9f8d6a4b3c");

    yield return new object[]
    {
      LegalDocumentType.PrivacyPolicy,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Current Privacy Policy",
            Content = "Privacy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2024, 12, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Current Privacy Policy",
          Content = "Privacy content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2024, 12, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Current Privacy Policy",
            Content = "Privacy content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2024, 12, 1),
            UpdatedAt = new DateTime(2024, 12, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Current Privacy Policy",
          Content = "Privacy content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2024, 12, 1)
        },
        HistoricalVersions = []
      }
    };

    var docId2 = Guid.Parse("c2f5a9d8-7e3b-4c1f-a5d9-6e8b7c4f2a1d");
    var versionId2 = Guid.Parse("d3e6b8f9-4c2a-5d7e-b9f1-3a5c7e9b2d4f");

    yield return new object[]
    {
      LegalDocumentType.TermsOfUse,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Current Terms",
            Content = "Terms content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 2,
          Title = "Current Terms",
          Content = "Terms content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Current Terms",
            Content = "Terms content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 2,
          Title = "Current Terms",
          Content = "Terms content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions = []
      }
    };

    var docId3 = Guid.Parse("e4f7c9a8-6d3e-5b2f-c8a1-9f5e7d6c4b3a");
    var versionId3 = Guid.Parse("45f88ea3-2185-4554-9b63-ae976fcca573");

    yield return new object[]
    {
      LegalDocumentType.PaymentServiceRefundPolicy,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Refund Policy",
            Content = "Refund content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 2, 15),
            UpdatedAt = new DateTime(2025, 2, 15)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy",
          Content = "Refund content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 2, 15)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Refund Policy",
            Content = "Refund content",
            Version = 1,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 2, 15),
            UpdatedAt = new DateTime(2025, 2, 15)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy",
          Content = "Refund content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 2, 15)
        },
        HistoricalVersions = []
      }
    };
  }

  [Theory]
  [MemberData(nameof(GetByTypeAndVersionValidTestData))]
  public async Task GetByTypeAndVersion_WithValidTypeAndVersion_ShouldReturnLegalDocument(
    LegalDocumentType type,
    uint version,
    LegalDocument? mockRepoResult,
    LegalDocument? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.GetByTypeAndVersion(It.IsAny<LegalDocumentType>(), It.IsAny<uint>()))
      .ReturnsAsync(new Result<LegalDocument?>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetByTypeAndVersion(type, version);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocument?>(expectedOutput));
    mockRepo.Verify(x => x.GetByTypeAndVersion(It.IsAny<LegalDocumentType>(), It.IsAny<uint>()), Times.Once);
  }

  public static IEnumerable<object[]> GetByTypeAndVersionValidTestData()
  {
    var docId1 = Guid.Parse("0dfab04a-65aa-4b30-b88f-de004d3c4414");
    var versionId1 = Guid.Parse("8e8216ad-c2d3-4d37-aa14-02a4ded27784");

    yield return new object[]
    {
      LegalDocumentType.PrivacyPolicy,
      (uint)1,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Archived,
            Title = "Privacy Policy v1",
            Content = "Old privacy content",
            Version = 1,
            EffectiveDate = new DateTime(2024, 1, 1),
            ExpiryDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2023, 12, 1),
            UpdatedAt = new DateTime(2023, 12, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Privacy Policy v1",
          Content = "Old privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2024, 1, 1),
          ExpiryDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2023, 12, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId1,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PrivacyPolicy,
            Status = LegalDocumentStatus.Archived,
            Title = "Privacy Policy v1",
            Content = "Old privacy content",
            Version = 1,
            EffectiveDate = new DateTime(2024, 1, 1),
            ExpiryDate = new DateTime(2025, 1, 1),
            CreatedAt = new DateTime(2023, 12, 1),
            UpdatedAt = new DateTime(2023, 12, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Privacy Policy v1",
          Content = "Old privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2024, 1, 1),
          ExpiryDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2023, 12, 1)
        },
        HistoricalVersions = []
      }
    };

    var docId2 = Guid.Parse("cbc65485-2039-44ab-8061-07e10675e2c0");
    var versionId2 = Guid.Parse("12716281-5fe3-430d-ac82-bb2ddad8372c");

    yield return new object[]
    {
      LegalDocumentType.TermsOfUse,
      (uint)2,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Terms of Use v2",
            Content = "Terms v2 content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 2,
          Title = "Terms of Use v2",
          Content = "Terms v2 content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId2,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.TermsOfUse,
            Status = LegalDocumentStatus.Active,
            Title = "Terms of Use v2",
            Content = "Terms v2 content",
            Version = 2,
            EffectiveDate = new DateTime(2025, 2, 1),
            CreatedAt = new DateTime(2024, 11, 1),
            UpdatedAt = new DateTime(2025, 2, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId2,
          DocumentId = docId2,
          Version = 2,
          Title = "Terms of Use v2",
          Content = "Terms v2 content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        HistoricalVersions = []
      }
    };

    var docId3 = Guid.Parse("4eeeb772-003e-4280-b34d-9d59a86aad56");
    var versionId3 = Guid.Parse("30fea9cd-8cfc-4558-b788-17ab080f14e1");

    yield return new object[]
    {
      LegalDocumentType.PaymentServiceRefundPolicy,
      (uint)3,
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Refund Policy v3",
            Content = "Latest refund policy",
            Version = 3,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 1),
            UpdatedAt = new DateTime(2025, 3, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 3,
          Title = "Refund Policy v3",
          Content = "Latest refund policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1)
        },
        HistoricalVersions = []
      },
      new LegalDocument
      {
        Principal = new LegalDocumentPrincipal
        {
          Id = docId3,
          Record = new LegalDocumentRecord
          {
            Type = LegalDocumentType.PaymentServiceRefundPolicy,
            Status = LegalDocumentStatus.Active,
            Title = "Refund Policy v3",
            Content = "Latest refund policy",
            Version = 3,
            EffectiveDate = new DateTime(2025, 3, 1),
            CreatedAt = new DateTime(2025, 1, 1),
            UpdatedAt = new DateTime(2025, 3, 1)
          }
        },
        CurrentVersion = new LegalDocumentVersion
        {
          Id = versionId3,
          DocumentId = docId3,
          Version = 3,
          Title = "Refund Policy v3",
          Content = "Latest refund policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1)
        },
        HistoricalVersions = []
      }
    };
  }

  [Theory]
  [MemberData(nameof(CreateValidTestData))]
  public async Task Create_WithValidInput_ShouldReturnLegalDocumentPrincipal(
    CreateLegalDocumentRequest input,
    LegalDocumentPrincipal mockRepoResult,
    LegalDocumentPrincipal expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Create(It.IsAny<CreateLegalDocumentRequest>()))
      .ReturnsAsync(new Result<LegalDocumentPrincipal>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<LegalDocumentPrincipal>>>>()))
      .Returns<Func<Task<Result<LegalDocumentPrincipal>>>>(func => func());
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Create(input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocumentPrincipal>(expectedOutput));
    mockRepo.Verify(x => x.Create(It.IsAny<CreateLegalDocumentRequest>()), Times.Once);
  }

  public static IEnumerable<object[]> CreateValidTestData()
  {
    var docId1 = Guid.Parse("16775e8e-a295-4cb5-905f-e184076a223f");

    yield return new object[]
    {
      new CreateLegalDocumentRequest
      {
        Type = LegalDocumentType.PrivacyPolicy,
        Title = "New Privacy Policy",
        Content = "Privacy policy content",
        EffectiveDate = new DateTime(2026, 1, 1),
        CreatedBy = "admin@example.com"
      },
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "New Privacy Policy",
          Content = "Privacy policy content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 1, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27),
          CreatedBy = "admin@example.com"
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "New Privacy Policy",
          Content = "Privacy policy content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 1, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27),
          CreatedBy = "admin@example.com"
        }
      }
    };

    var docId2 = Guid.Parse("368d2c5a-d259-4f0f-9118-4b54a8aa2628");

    yield return new object[]
    {
      new CreateLegalDocumentRequest
      {
        Type = LegalDocumentType.TermsOfUse,
        Title = "New Terms of Use",
        Content = "Terms content",
        EffectiveDate = new DateTime(2026, 2, 1),
        ExpiryDate = new DateTime(2027, 2, 1),
        CreatedBy = "legal@example.com"
      },
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Draft,
          Title = "New Terms of Use",
          Content = "Terms content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 2, 1),
          ExpiryDate = new DateTime(2027, 2, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27),
          CreatedBy = "legal@example.com"
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Draft,
          Title = "New Terms of Use",
          Content = "Terms content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 2, 1),
          ExpiryDate = new DateTime(2027, 2, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27),
          CreatedBy = "legal@example.com"
        }
      }
    };

    var docId3 = Guid.Parse("5884ebc4-5516-42e9-8def-4d70feb24dc7");

    yield return new object[]
    {
      new CreateLegalDocumentRequest
      {
        Type = LegalDocumentType.PaymentServiceRefundPolicy,
        Title = "Refund Policy 2026",
        Content = "Comprehensive refund policy",
        EffectiveDate = new DateTime(2026, 3, 1)
      },
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Refund Policy 2026",
          Content = "Comprehensive refund policy",
          Version = 1,
          EffectiveDate = new DateTime(2026, 3, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Refund Policy 2026",
          Content = "Comprehensive refund policy",
          Version = 1,
          EffectiveDate = new DateTime(2026, 3, 1),
          CreatedAt = new DateTime(2025, 10, 27),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      }
    };
  }

  [Theory]
  [MemberData(nameof(UpdateValidTestData))]
  public async Task Update_WithValidInput_ShouldReturnUpdatedLegalDocumentPrincipal(
    Guid id,
    UpdateLegalDocumentRequest input,
    LegalDocumentPrincipal? mockRepoResult,
    LegalDocumentPrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.Update(It.IsAny<Guid>(), It.IsAny<UpdateLegalDocumentRequest>()))
      .ReturnsAsync(new Result<LegalDocumentPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<LegalDocumentPrincipal?>>>>()))
      .Returns<Func<Task<Result<LegalDocumentPrincipal?>>>>(func => func());
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Update(id, input);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocumentPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.Update(It.IsAny<Guid>(), It.IsAny<UpdateLegalDocumentRequest>()), Times.Once);
  }

  public static IEnumerable<object[]> UpdateValidTestData()
  {
    var docId1 = Guid.Parse("dbe5a141-347d-4c22-99d9-fb002cc6c703");

    yield return new object[]
    {
      docId1,
      new UpdateLegalDocumentRequest
      {
        Title = "Updated Privacy Policy",
        Content = "Updated privacy content",
        EffectiveDate = new DateTime(2026, 1, 15),
        UpdatedBy = "admin@example.com"
      },
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Updated Privacy Policy",
          Content = "Updated privacy content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 1, 15),
          CreatedAt = new DateTime(2025, 1, 1),
          UpdatedAt = new DateTime(2025, 10, 27),
          UpdatedBy = "admin@example.com"
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Updated Privacy Policy",
          Content = "Updated privacy content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 1, 15),
          CreatedAt = new DateTime(2025, 1, 1),
          UpdatedAt = new DateTime(2025, 10, 27),
          UpdatedBy = "admin@example.com"
        }
      }
    };

    var docId2 = Guid.Parse("f6b5041f-770d-4afd-ab4b-e0cf05b83dad");

    yield return new object[]
    {
      docId2,
      new UpdateLegalDocumentRequest
      {
        Title = "Revised Terms",
        Content = "Revised terms content",
        EffectiveDate = new DateTime(2026, 2, 15),
        ExpiryDate = new DateTime(2027, 2, 15),
        UpdatedBy = "legal@example.com"
      },
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Draft,
          Title = "Revised Terms",
          Content = "Revised terms content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 2, 15),
          ExpiryDate = new DateTime(2027, 2, 15),
          CreatedAt = new DateTime(2025, 2, 1),
          UpdatedAt = new DateTime(2025, 10, 27),
          UpdatedBy = "legal@example.com"
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Draft,
          Title = "Revised Terms",
          Content = "Revised terms content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 2, 15),
          ExpiryDate = new DateTime(2027, 2, 15),
          CreatedAt = new DateTime(2025, 2, 1),
          UpdatedAt = new DateTime(2025, 10, 27),
          UpdatedBy = "legal@example.com"
        }
      }
    };

    var docId3 = Guid.Parse("82a2daf6-07f6-46a0-bd7b-4902bde56e25");

    yield return new object[]
    {
      docId3,
      new UpdateLegalDocumentRequest
      {
        Title = "Updated Refund Policy",
        Content = "Updated refund policy content",
        EffectiveDate = new DateTime(2026, 3, 15)
      },
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Updated Refund Policy",
          Content = "Updated refund policy content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 3, 15),
          CreatedAt = new DateTime(2025, 3, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Updated Refund Policy",
          Content = "Updated refund policy content",
          Version = 2,
          EffectiveDate = new DateTime(2026, 3, 15),
          CreatedAt = new DateTime(2025, 3, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      }
    };
  }

  [Theory]
  [MemberData(nameof(SetStatusValidTestData))]
  public async Task SetStatus_WithValidInput_ShouldReturnUpdatedLegalDocumentPrincipal(
    Guid id,
    LegalDocumentStatus status,
    LegalDocumentPrincipal? mockRepoResult,
    LegalDocumentPrincipal? expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.SetStatus(It.IsAny<Guid>(), It.IsAny<LegalDocumentStatus>()))
      .ReturnsAsync(new Result<LegalDocumentPrincipal?>(mockRepoResult));
    mockTm.Setup(x => x.Start(It.IsAny<Func<Task<Result<LegalDocumentPrincipal?>>>>()))
      .Returns<Func<Task<Result<LegalDocumentPrincipal?>>>>(func => func());
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.SetStatus(id, status);

    // Assert
    actual.Should().BeEquivalentTo(new Result<LegalDocumentPrincipal?>(expectedOutput));
    mockRepo.Verify(x => x.SetStatus(It.IsAny<Guid>(), It.IsAny<LegalDocumentStatus>()), Times.Once);
  }

  public static IEnumerable<object[]> SetStatusValidTestData()
  {
    var docId1 = Guid.Parse("e7987d7a-68a3-4d03-a0ea-b8535fbe0266");

    yield return new object[]
    {
      docId1,
      LegalDocumentStatus.Active,
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Active,
          Title = "Privacy Policy",
          Content = "Privacy content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 1, 1),
          CreatedAt = new DateTime(2025, 1, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId1,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PrivacyPolicy,
          Status = LegalDocumentStatus.Active,
          Title = "Privacy Policy",
          Content = "Privacy content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 1, 1),
          CreatedAt = new DateTime(2025, 1, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      }
    };

    var docId2 = Guid.Parse("f8eeb021-3a90-4e25-adc2-1c470117de53");

    yield return new object[]
    {
      docId2,
      LegalDocumentStatus.Archived,
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Archived,
          Title = "Old Terms",
          Content = "Old terms content",
          Version = 1,
          EffectiveDate = new DateTime(2024, 1, 1),
          ExpiryDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2023, 12, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId2,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.TermsOfUse,
          Status = LegalDocumentStatus.Archived,
          Title = "Old Terms",
          Content = "Old terms content",
          Version = 1,
          EffectiveDate = new DateTime(2024, 1, 1),
          ExpiryDate = new DateTime(2025, 1, 1),
          CreatedAt = new DateTime(2023, 12, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      }
    };

    var docId3 = Guid.Parse("50570723-6733-4b34-aa2b-67043ad330a0");

    yield return new object[]
    {
      docId3,
      LegalDocumentStatus.Draft,
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Draft Refund Policy",
          Content = "Draft content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 3, 1),
          CreatedAt = new DateTime(2025, 10, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      },
      new LegalDocumentPrincipal
      {
        Id = docId3,
        Record = new LegalDocumentRecord
        {
          Type = LegalDocumentType.PaymentServiceRefundPolicy,
          Status = LegalDocumentStatus.Draft,
          Title = "Draft Refund Policy",
          Content = "Draft content",
          Version = 1,
          EffectiveDate = new DateTime(2026, 3, 1),
          CreatedAt = new DateTime(2025, 10, 1),
          UpdatedAt = new DateTime(2025, 10, 27)
        }
      }
    };
  }

  [Theory]
  [InlineData("dcb21177-1198-4b2e-9cee-45ba5358dfe2")]
  [InlineData("49e10930-816b-4a14-984a-440b8c39acb1")]
  [InlineData("1dd69741-8e5e-434a-a3dc-c3d0b31138df")]
  public async Task Delete_WithExistingId_ShouldReturnUnit(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = new Unit();
    Unit? expectedOutput = new Unit();
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
  }

  [Theory]
  [InlineData("ac6b3dce-a273-438b-9d5f-3df202911473")]
  [InlineData("54b9aa8f-ccb4-4697-9eeb-6abed1524c84")]
  [InlineData("9d56defd-d92c-4556-a113-f9d4492c3539")]
  public async Task Delete_WithNonExistingId_ShouldReturnNull(string idStr)
  {
    // Arrange
    var id = Guid.Parse(idStr);
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    Unit? mockRepoResult = null;
    Unit? expectedOutput = null;
    mockRepo.Setup(x => x.Delete(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<Unit?>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.Delete(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<Unit?>(expectedOutput));
    mockRepo.Verify(x => x.Delete(It.IsAny<Guid>()), Times.Once);
  }

  [Theory]
  [MemberData(nameof(GetVersionHistoryValidTestData))]
  public async Task GetVersionHistory_WithValidId_ShouldReturnVersionHistory(
    Guid id,
    IEnumerable<LegalDocumentVersion> mockRepoResult,
    IEnumerable<LegalDocumentVersion> expectedOutput)
  {
    // Arrange
    var mockRepo = new Mock<ILegalDocumentRepository>();
    var mockTm = new Mock<ITransactionManager>();
    mockRepo.Setup(x => x.GetVersionHistory(It.IsAny<Guid>()))
      .ReturnsAsync(new Result<IEnumerable<LegalDocumentVersion>>(mockRepoResult));
    var service = new LegalDocumentService(mockRepo.Object, mockTm.Object);

    // Act
    var actual = await service.GetVersionHistory(id);

    // Assert
    actual.Should().BeEquivalentTo(new Result<IEnumerable<LegalDocumentVersion>>(expectedOutput));
    mockRepo.Verify(x => x.GetVersionHistory(It.IsAny<Guid>()), Times.Once);
  }

  public static IEnumerable<object[]> GetVersionHistoryValidTestData()
  {
    var docId1 = Guid.Parse("2e134f65-4a0a-44be-ae44-64df6abc3ba9");
    var versionId1 = Guid.Parse("b23f9309-2dd6-4d66-94cb-d51efadbdbb5");
    var versionId2 = Guid.Parse("78c99536-4fc0-4fa8-b041-e646b65809d5");
    var versionId3 = Guid.Parse("85d8b2c2-a9d6-4770-913c-6dc61f56e6cb");

    yield return new object[]
    {
      docId1,
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = versionId3,
          DocumentId = docId1,
          Version = 3,
          Title = "Privacy Policy v3",
          Content = "Latest privacy content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1),
          CreatedBy = "admin@example.com"
        },
        new()
        {
          Id = versionId2,
          DocumentId = docId1,
          Version = 2,
          Title = "Privacy Policy v2",
          Content = "Updated privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 2, 1),
          ExpiryDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 2, 1),
          CreatedBy = "admin@example.com"
        },
        new()
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Privacy Policy v1",
          Content = "Initial privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 1, 1),
          ExpiryDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 1, 1),
          CreatedBy = "admin@example.com"
        }
      },
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = versionId3,
          DocumentId = docId1,
          Version = 3,
          Title = "Privacy Policy v3",
          Content = "Latest privacy content",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1),
          CreatedBy = "admin@example.com"
        },
        new()
        {
          Id = versionId2,
          DocumentId = docId1,
          Version = 2,
          Title = "Privacy Policy v2",
          Content = "Updated privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 2, 1),
          ExpiryDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 2, 1),
          CreatedBy = "admin@example.com"
        },
        new()
        {
          Id = versionId1,
          DocumentId = docId1,
          Version = 1,
          Title = "Privacy Policy v1",
          Content = "Initial privacy content",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 1, 1),
          ExpiryDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 1, 1),
          CreatedBy = "admin@example.com"
        }
      }
    };

    var docId2 = Guid.Parse("68c6b02c-b7c6-44cb-952a-c833327beb13");
    var termsVersion1 = Guid.Parse("405d8a93-74f7-497c-a2a4-79f7b33447fe");
    var termsVersion2 = Guid.Parse("4df4b8ac-5a47-4007-8d4c-3da0c4f1b31e");

    yield return new object[]
    {
      docId2,
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = termsVersion2,
          DocumentId = docId2,
          Version = 2,
          Title = "Terms of Use v2",
          Content = "Updated terms",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        new()
        {
          Id = termsVersion1,
          DocumentId = docId2,
          Version = 1,
          Title = "Terms of Use v1",
          Content = "Initial terms",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 1, 1),
          ExpiryDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 1, 1)
        }
      },
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = termsVersion2,
          DocumentId = docId2,
          Version = 2,
          Title = "Terms of Use v2",
          Content = "Updated terms",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 2, 1)
        },
        new()
        {
          Id = termsVersion1,
          DocumentId = docId2,
          Version = 1,
          Title = "Terms of Use v1",
          Content = "Initial terms",
          Status = LegalDocumentStatus.Archived,
          EffectiveDate = new DateTime(2025, 1, 1),
          ExpiryDate = new DateTime(2025, 2, 1),
          CreatedAt = new DateTime(2025, 1, 1)
        }
      }
    };

    var docId3 = Guid.Parse("af3d848d-e8fa-41df-83af-43a41f1e1abe");
    var refundVersion1 = Guid.Parse("4fd25942-a013-4b25-9d5b-e82bb3ab8a77");

    yield return new object[]
    {
      docId3,
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = refundVersion1,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy v1",
          Content = "Initial refund policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1),
          CreatedBy = "legal@example.com"
        }
      },
      new List<LegalDocumentVersion>
      {
        new()
        {
          Id = refundVersion1,
          DocumentId = docId3,
          Version = 1,
          Title = "Refund Policy v1",
          Content = "Initial refund policy",
          Status = LegalDocumentStatus.Active,
          EffectiveDate = new DateTime(2025, 3, 1),
          CreatedAt = new DateTime(2025, 3, 1),
          CreatedBy = "legal@example.com"
        }
      }
    };
  }
}
