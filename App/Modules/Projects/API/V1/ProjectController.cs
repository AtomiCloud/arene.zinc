using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.Utility;
using Asp.Versioning;
using CarboxylicLithium;
using Domain.Projects;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Projects.API.V1;

[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProjectController(
  IProjectService service,
  CreateProjectReqValidator createValidator,
  UpdateProjectReqValidator updateValidator,
  ProjectSearchQueryValidator searchValidator
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<ProjectPrincipalRes>>> Search([FromQuery] SearchProjectQuery query)
  {
    var x = await searchValidator
      .ValidateAsyncResult(query, "Invalid SearchProjectQuery")
      .ThenAwait(q => service.Search(q.ToDomain()))
      .Then(x => x.Select(u => u.ToRes()), Errors.MapNone);
    return this.ReturnResult(x);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<ProjectRes>> Get(Guid id)
  {
    var user = await service.Get(id)
      .Then(x => x?.ToRes(), Errors.MapNone);

    return this.ReturnNullableResult(user, new EntityNotFound(
      "Project Not Found", typeof(Project), id.ToString()));
  }

  [HttpPost]
  public async Task<ActionResult<ProjectPrincipalRes>> Create([FromBody] CreateProjectReq req)
  {
    var user = await createValidator
      .ValidateAsyncResult(req, "Invalid CreateProjectReq")
      .ThenAwait(x => service.Create(x.ToRecord()))
      .Then(x => x.ToRes(), Errors.MapNone);

    return this.ReturnResult(user);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult<ProjectPrincipalRes>> Update(Guid id, [FromBody] UpdateProjectReq req)
  {
    var project = await updateValidator
      .ValidateAsyncResult(req, "Invalid UpdateUserReq")
      .ThenAwait(x => service.Update(id, x.ToRecord()))
      .Then(x => (x?.ToRes()).ToResult());

    return this.ReturnNullableResult(project, new EntityNotFound(
      "Project Not Found", typeof(ProjectPrincipal), id.ToString()));
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> Delete(Guid id)
  {
    var user = await service.Delete(id);
    return this.ReturnUnitNullableResult(user, new EntityNotFound(
      "Project Not Found", typeof(Project), id.ToString()));
  }
}
