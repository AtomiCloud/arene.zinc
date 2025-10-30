using Domain.Marketing.Subscribers;

namespace App.Modules.Subscribers.API.V1;

public static class SubscriberMapper
{
  // Query -> Domain
  public static SubscriberSearch ToDomain(this SearchSubscriberQuery query)
  {
    return new SubscriberSearch
    {
      ProjectId = query.ProjectId,
      SubscriptionTypeId = query.SubscriptionTypeId?.Split(',').Select(x => x.Trim()),
      Email = query.Email,
      Limit = query.Limit ?? 100,
      Skip = query.Skip ?? 0
    };
  }

  // Req -> Domain
  public static SubscriptionEvent ToDomain(this RecordSubscriptionReq req)
  {
    return new SubscriptionEvent
    {
      Type = req.Type,
      LegalBasis = (LegalBasis)req.LegalBasis,
      Reason = req.Reason,
      Open = req.Open,
      Timezone = req.Timezone ?? "UTC",
      Time = req.Time ?? DateTimeOffset.UtcNow
    };
  }

  // Domain -> Res
  public static SubscriberPrincipalRes ToRes(this SubscriberPrincipal principal)
  {
    return new SubscriberPrincipalRes(
      principal.ProjectId,
      principal.Email,
      principal.Computed.ToRes()
    );
  }

  public static SubscriberComputedRes ToRes(this SubscriberComputed computed)
  {
    return new SubscriberComputedRes(
      computed.TimeZone,
      computed.Subscriptions.Select(x => x.ToRes())
    );
  }

  public static SubscriptionStatusRes ToRes(this SubscriptionStatus status)
  {
    return new SubscriptionStatusRes(
      status.Type,
      status.Enabled,
      (int)status.LegalBasis,
      status.LegalReason,
      status.UpdatedAt
    );
  }

  public static SubscriberRes ToRes(this Subscriber subscriber)
  {
    return new SubscriberRes(subscriber.Principal.ToRes());
  }
}
