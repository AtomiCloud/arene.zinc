using System.ComponentModel.DataAnnotations;

namespace App.Modules.Projects.Data;

public class ProjectData
{
  public Guid Id { get; set; } = Guid.Empty;

  [MaxLength(256)] public string Name { get; set; } = string.Empty;

  public bool Open { get; set; }
}
