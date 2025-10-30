using Domain.Marketing.Subscribers;

namespace App.Modules.Subscribers.API.V1;

// QUERY
public record SearchSubscriberQuery(
  Guid? ProjectId,
  string? SubscriptionTypeId,
  string? Email,
  int? Limit,
  int? Skip);

// REQ
public record RecordSubscriptionReq(
  string Type,
  int LegalBasis,
  string Reason,
  bool Open,
  string? Timezone,
  DateTimeOffset? Time);

// RESP
public record SubscriptionStatusRes(
  string Type,
  bool Enabled,
  int LegalBasis,
  string LegalReason,
  DateTimeOffset UpdatedAt);

public record SubscriberComputedRes(
  string TimeZone,
  IEnumerable<SubscriptionStatusRes> Subscriptions);

public record SubscriberPrincipalRes(
  Guid ProjectId,
  string Email,
  SubscriberComputedRes Computed);

public record SubscriberRes(SubscriberPrincipalRes Principal);
