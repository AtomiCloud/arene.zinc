namespace Domain.Marketing.Subscribers;

// Subscription log is stored by the data layer; included as VO for computing status
public record SubscriptionEvent
{
  public required string Type { get; init; }

  public required LegalBasis LegalBasis { get; init; }

  public required string Reason { get; init; }

  public required bool Open { get; init; } // created via unauthenticated channel

  public required string Timezone { get; init; }

  public required DateTimeOffset Time { get; init; }
}
