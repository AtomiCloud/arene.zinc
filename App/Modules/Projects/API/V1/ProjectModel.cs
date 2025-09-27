namespace App.Modules.Projects.API.V1;

public record SearchProjectQuery(
  Guid? Id,
  string? Name,
  int? Limit,
  int? Skip);

// REQ
public record CreateProjectReq(string Name, bool Open); 

public record UpdateProjectReq(string Name, bool Open);

// RESP

public record ProjectPrincipalRes(Guid Id, string Name, bool Open);

public record ProjectRes(ProjectPrincipalRes Principal, uint SubscriberCount);
