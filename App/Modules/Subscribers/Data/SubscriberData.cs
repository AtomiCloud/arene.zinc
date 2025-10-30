using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Modules.Projects.Data;

namespace App.Modules.Subscribers.Data;

public class SubscriberData
{
  [MaxLength(320), EmailAddress] public string Email { get; set; } = default!;

  // Computed fields for fast reads/filters
  [MaxLength(128)] public string TimeZone { get; set; } = "UTC";

  // Denormalized JSON map of timezone counts: { "Europe/Paris": 3, ... }
  [Column(TypeName = "jsonb")] public Dictionary<string, int> TimeZoneCounts { get; set; } = [];

  // FK
  public Guid ProjectId { get; set; }
  public ProjectData Project { get; set; } = default!;
}
