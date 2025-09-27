using Domain.Marketing.SubscriptionType;

namespace App.Modules.SubscriptionTypes.API.V1;

public static class SubscriptionTypeMapper
{
  public static SubscriptionTypePrincipalRes ToRes(this SubscriptionTypePrincipal p)
    => new(p.ProjectId, p.Id, p.Record.Desc);

  public static SubscriptionTypeRes ToRes(this SubscriptionType p)
    => new(p.Principal.ToRes(), p.Count);

  public static SubscriptionTypeRecord ToRecord(this CreateSubscriptionTypeReq p)
    => new() { Desc = p.Desc };

  public static SubscriptionTypeRecord ToRecord(this UpdateSubscriptionTypeReq p)
    => new() { Desc = p.Desc };

  public static SubscriptionTypeSearch ToDomain(this SearchSubscriptionTypeQuery q, Guid projectId)
    => new()
    {
      Guid = projectId,
      Name = q.Name,
      Desc = q.Desc,
      Limit = q.Limit ?? 100,
      Skip = q.Skip ?? 0
    };
}
