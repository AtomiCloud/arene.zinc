using System.ComponentModel.DataAnnotations;

namespace App.Modules.SubscriptionTypes.Data;

public class SubscriptionTypeData
{
  public Guid ProjectId { get; set; }

  [MaxLength(256)] public string Id { get; set; } = string.Empty; // Type name/key

  [MaxLength(512)] public string Desc { get; set; } = string.Empty;
}

