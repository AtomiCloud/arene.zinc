namespace Domain.Marketing.SubscriptionType;

public record SubscriptionTypeSearch
{
  public string? Name { get; init; }
  public string? Desc { get; init; }
  public Guid? Guid { get; init; }

  public int Limit { get; init; }

  public int Skip { get; init; }
}

public record SubscriptionType
{
  public required SubscriptionTypePrincipal Principal { get; init; }
  public required uint Count { get; init; }
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
