namespace App.Modules.SubscriptionTypes.API.V1;

public record SearchSubscriptionTypeQuery(
  string? Name,
  string? Desc,
  int? Limit,
  int? Skip
);

public record CreateSubscriptionTypeReq(string Name, string Desc);

public record UpdateSubscriptionTypeReq(string Desc);

public record SubscriptionTypePrincipalRes(Guid ProjectId, string Id, string Desc);

public record SubscriptionTypeRes(SubscriptionTypePrincipalRes Principal, uint Count);
