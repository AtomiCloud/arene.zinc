using Domain.Marketing.SubscriptionType;

namespace App.Modules.SubscriptionTypes.Data;

public static class SubscriptionTypeMapper
{
  // To Domain
  public static SubscriptionType ToDomain(this SubscriptionTypeData d, uint count) => new()
  {
    Principal = d.ToPrincipal(), Count = count,
  };

  public static SubscriptionTypePrincipal ToPrincipal(this SubscriptionTypeData d) => new()
  {
    ProjectId = d.ProjectId, Id = d.Id, Record = d.ToRecord(),
  };

  public static SubscriptionTypeRecord ToRecord(this SubscriptionTypeData p) => new() { Desc = p.Desc };

  // ToData

  public static SubscriptionTypeData ToData(this SubscriptionTypeRecord r) => new() { Desc = r.Desc };

  public static SubscriptionTypeData Update(this SubscriptionTypeData d, SubscriptionTypeRecord r)
  {
    d.Desc = r.Desc;
    return d;
  }
}
