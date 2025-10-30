using Domain.Marketing.Subscribers;

namespace App.Modules.Subscribers.Data;

public static class SubscriberMapper
{
  public static SubscriptionStatus ToStatus(this SubscriberTypeStatusData s) => new()
  {
    Type = s.SubscriptionTypeId,
    Enabled = s.Enabled,
    LegalBasis = (LegalBasis)s.LegalBasis,
    LegalReason = s.Reason,
    UpdatedAt = s.UpdatedAt
  };

  public static SubscriberComputed ToComputed(this SubscriberData d, IEnumerable<SubscriberTypeStatusData> statuses)
    => new()
    {
      TimeZone = d.TimeZone,
      Subscriptions = statuses.Select(ToStatus).ToArray()
    };

  public static SubscriberPrincipal ToPrincipal(this SubscriberData d, IEnumerable<SubscriberTypeStatusData> statuses)
    => new()
    {
      ProjectId = d.ProjectId,
      Email = d.Email,
      Computed = d.ToComputed(statuses)
    };

  public static Subscriber ToDomain(this SubscriberData d, IEnumerable<SubscriberTypeStatusData> statuses)
    => new()
    {
      Principal = d.ToPrincipal(statuses)
    };

  public static SubscriptionEventData ToEventData(this SubscriptionEvent e)
    => new()
    {
      SubscriptionTypeId = e.Type,
      LegalBasis = (int)e.LegalBasis,
      Reason = e.Reason,
      Open = e.Open,
      Timezone = e.Timezone,
      Time = e.Time
    };

  public static SubscriberTypeStatusData ToTypeData(this SubscriptionEvent e)
    => new()
    {
      SubscriptionTypeId = e.Type,
      LegalBasis = (int)e.LegalBasis,
      Reason = e.Reason,
      Enabled = e.LegalBasis != LegalBasis.None,
      UpdatedAt = e.Time
    };

  public static SubscriberData NewSubscriberData(Guid projectId, string email)
    => new()
    {
      ProjectId = projectId,
      Email = email,
      TimeZone = "UTC",
      TimeZoneCounts = []
    };
}
