namespace Domain.Marketing.Subscribers;

// Subscription
public record SubscriberSearch
{
  public Guid? ProjectId { get; init; }
  public IEnumerable<string>? SubscriptionTypeId { get; init; }
  public string? Email { get; init; }
  public int Limit { get; init; }
  public int Skip { get; init; }
}

public record SubscriptionStatus
{
  public required string Type { get; init; }
  public required bool Enabled { get; init; }

  public required LegalBasis LegalBasis { get; init; }

  public required string LegalReason { get; init; }
  public required DateTimeOffset UpdatedAt { get; init; }
}


public record Subscriber
{
  public required SubscriberPrincipal Principal { get; init; }

}

public record SubscriberPrincipal
{
  public required Guid ProjectId { get; init; }
  public required string Email { get; init; }


  public required SubscriberComputed Computed { get; init; }
}

public record SubscriberComputed
{
  public required string TimeZone { get; init; }
  public required IEnumerable<SubscriptionStatus> Subscriptions { get; init; }
}



public enum LegalBasis
{
  // Standard GDPR legal bases
  Consent = 0,
  Contract = 1,
  LegalObligation = 2,
  VitalInterests = 3,
  PublicTask = 4,
  LegitimateInterests = 5,

  // Extended: represent that we do not have a legal basis
  None = 6
}
