using System.ComponentModel.DataAnnotations;
using App.Modules.Projects.Data;
using App.Modules.SubscriptionTypes.Data;

namespace App.Modules.Subscribers.Data;

public class SubscriptionEventData
{
  public long Id { get; set; }



  // Data fields
  public int LegalBasis { get; set; }
  [MaxLength(512)] public string Reason { get; set; } = string.Empty;
  public bool Open { get; set; }
  [MaxLength(128)] public string Timezone { get; set; } = "UTC";
  public DateTimeOffset Time { get; set; }

  // Foreign keys + Navigations (kept together)
  public Guid ProjectId { get; set; }
  public ProjectData Project { get; set; } = default!;

  [MaxLength(320)] public string Email { get; set; } = string.Empty;
  public SubscriberData Subscriber { get; set; } = default!;

  [MaxLength(128)] public string SubscriptionTypeId { get; set; } = string.Empty;
  public SubscriptionTypeData SubscriptionType { get; set; } = default!;
}
