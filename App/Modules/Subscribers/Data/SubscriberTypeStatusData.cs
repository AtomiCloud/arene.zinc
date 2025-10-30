using System.ComponentModel.DataAnnotations;
using App.Modules.Projects.Data;
using App.Modules.SubscriptionTypes.Data;

namespace App.Modules.Subscribers.Data;

public class SubscriberTypeStatusData
{
  // FK
  public Guid ProjectId { get; set; }
  public ProjectData Project { get; set; } = default!;

  [MaxLength(320)] public string Email { get; set; } = default!;
  public SubscriberData Subscriber { get; set; } = default!;

  [MaxLength(128)] public string SubscriptionTypeId { get; set; } = default!;
  public SubscriptionTypeData SubscriptionType { get; set; } = default!;

  // Computed status fields
  public int LegalBasis { get; set; }
  public bool Enabled { get; set; }
  [MaxLength(512)] public string Reason { get; set; } = default!;
  public DateTimeOffset UpdatedAt { get; set; }
}
