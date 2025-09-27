using App.Modules.Common;
using Domain.Projects;
using Domain.User;

namespace App.Modules.Projects.API.V1;

public static class ProjectMapper
{
  // RES
  public static ProjectPrincipalRes ToRes(this ProjectPrincipal p)
    => new(p.Id, p.Record.Name, p.Record.Open);

  public static ProjectRes ToRes(this Project p)
    => new(p.Principal.ToRes(), p.SubscriberCount);


  // REQ
  public static ProjectRecord ToRecord(this CreateProjectReq p) =>
    new() { Name = p.Name, Open = p.Open };

  public static ProjectRecord ToRecord(this UpdateProjectReq p) =>
    new() { Name = p.Name, Open = p.Open };
  
  public static ProjectSearch ToDomain(this SearchProjectQuery query) =>
    new()
    {
      Id = query.Id, 
      Name = query.Name, 
      Limit = query.Limit ?? 100, 
      Skip = query.Skip ?? 0,
    };
}
