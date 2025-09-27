namespace Domain.Marketing.SubscriptionType;

public record SubscriptionTypeSearch
{
  public string? Name { get; init; }
  public string? Desc { get; init; }
  public Guid? Guid { get; init; }
}

public record SubscriberType
{
  public required SubscriptionTypePrincipal Principal { get; init; }
}

public record SubscriptionTypePrincipal
{
  public required Guid ProjectId { get; init; }
  public required string Id { get; init; }

  public required SubscriptionTypeRecord Record { get; init; }
}

public record SubscriptionTypeRecord
{
  public required string Desc { get; init; }
}
