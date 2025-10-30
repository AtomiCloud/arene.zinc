namespace Domain.Legal;

public enum LegalDocumentType
{
  PrivacyPolicy,
  TermsOfUse,
  PaymentServiceRefundPolicy
}

public enum LegalDocumentStatus
{
  Draft,
  Active,
  Archived
}

public record LegalDocumentSearch
{
  public Guid? Id { get; init; }
  public LegalDocumentType? Type { get; init; }
  public LegalDocumentStatus? Status { get; init; }
  public DateTime? EffectiveDate { get; init; }
  public bool? IncludeHistorical { get; init; } = false;
  public int Limit { get; init; } = 50;
  public int Skip { get; init; } = 0;
}

public record LegalDocument
{
  public required LegalDocumentPrincipal Principal { get; init; }
  public required LegalDocumentVersion CurrentVersion { get; init; }
  public required IEnumerable<LegalDocumentVersion> HistoricalVersions { get; init; }
}

public record LegalDocumentPrincipal
{
  public required Guid Id { get; init; }
  public required LegalDocumentRecord Record { get; init; }
}

public record LegalDocumentRecord
{
  public required LegalDocumentType Type { get; init; }
  public required LegalDocumentStatus Status { get; init; }
  public required DateTime EffectiveDate { get; init; }
  public DateTime? ExpiryDate { get; init; }
  public required string Title { get; init; }
  public required string Content { get; init; }
  public required uint Version { get; init; }
  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
  public string? CreatedBy { get; init; }
  public string? UpdatedBy { get; init; }
}

public record LegalDocumentVersion
{
  public required Guid Id { get; init; }
  public required Guid DocumentId { get; init; }
  public required uint Version { get; init; }
  public required string Title { get; init; }
  public required string Content { get; init; }
  public required LegalDocumentStatus Status { get; init; }
  public required DateTime EffectiveDate { get; init; }
  public DateTime? ExpiryDate { get; init; }
  public required DateTime CreatedAt { get; init; }
  public string? CreatedBy { get; init; }
}

public record CreateLegalDocumentRequest
{
  public required LegalDocumentType Type { get; init; }
  public required string Title { get; init; }
  public required string Content { get; init; }
  public required DateTime EffectiveDate { get; init; }
  public DateTime? ExpiryDate { get; init; }
  public string? CreatedBy { get; init; }
}

public record UpdateLegalDocumentRequest
{
  public required string Title { get; init; }
  public required string Content { get; init; }
  public required DateTime EffectiveDate { get; init; }
  public DateTime? ExpiryDate { get; init; }
  public string? UpdatedBy { get; init; }
}
