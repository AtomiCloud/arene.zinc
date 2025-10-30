using Domain.Projects;
using Domain.User;

namespace App.Modules.Projects.Data;

public static class ProjectMapper
{
  public static ProjectRecord ToRecord(this ProjectData data) => new() { Open = data.Open, Name = data.Name, };

  public static ProjectPrincipal ToPrincipal(this ProjectData data) =>
    new() { Id = data.Id, Record = data.ToRecord(), };


  public static Project ToDomain(this ProjectData data, uint count) => new()
  {
    Principal = data.ToPrincipal(),
    SubscriberCount = count,
  };

  public static ProjectData ToData(this ProjectRecord record) => new() { Open = record.Open, Name = record.Name, };

  public static ProjectData Update(this ProjectData data, ProjectRecord record)
  {
    data.Name = record.Name;
    data.Open = record.Open;
    return data;
  }
}
