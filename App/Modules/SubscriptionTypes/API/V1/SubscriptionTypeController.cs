using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.Utility;
using Asp.Versioning;
using CSharp_Result;
using Domain.Marketing.SubscriptionType;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.SubscriptionTypes.API.V1;

[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/subscription-types")]
public class SubscriptionTypeController(
  ISubscriptionTypeService service,
  CreateSubscriptionTypeReqValidator createValidator,
  UpdateSubscriptionTypeReqValidator updateValidator,
  SubscriptionTypeSearchQueryValidator searchValidator
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<SubscriptionTypePrincipalRes>>> Search(Guid projectId, [FromQuery] SearchSubscriptionTypeQuery query)
  {
    var x = await searchValidator
      .ValidateAsyncResult(query, "Invalid SearchSubscriptionTypeQuery")
      .ThenAwait(q => service.Search(q.ToDomain(projectId)))
      .Then(x => x.Select(u => u.ToRes()), Errors.MapNone);
    return this.ReturnResult(x);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<SubscriptionTypeRes>> Get(Guid projectId, string id)
  {
    var res = await service.Get(projectId, id)
      .Then(x => x?.ToRes(), Errors.MapNone);
    return this.ReturnNullableResult(res, new EntityNotFound(
      "SubscriptionType Not Found", typeof(SubscriptionType), $"{projectId}:{id}"));
  }

  [HttpPost]
  public async Task<ActionResult<SubscriptionTypePrincipalRes>> Create(Guid projectId, [FromBody] CreateSubscriptionTypeReq req)
  {
    var res = await createValidator
      .ValidateAsyncResult(req, "Invalid CreateSubscriptionTypeReq")
      .ThenAwait(x => service.Create(projectId, req.Name, x.ToRecord()))
      .Then(x => x?.ToRes(), Errors.MapNone);
    return this.ReturnNullableResult(res, new EntityNotFound(
      "Project Not Found", typeof(SubscriptionTypePrincipal), projectId.ToString()));
  }

  [HttpPut("{id}")]
  public async Task<ActionResult<SubscriptionTypePrincipalRes>> Update(Guid projectId, string id, [FromBody] UpdateSubscriptionTypeReq req)
  {
    var res = await updateValidator
      .ValidateAsyncResult(req, "Invalid UpdateSubscriptionTypeReq")
      .ThenAwait(x => service.Update(projectId, id, x.ToRecord()))
      .Then(x => (x?.ToRes()).ToResult());

    return this.ReturnNullableResult(res, new EntityNotFound(
      "Subscription Type Not Found", typeof(SubscriptionTypePrincipal), $"{projectId}:{id}"));
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> Delete(Guid projectId, string id)
  {
    var res = await service.Delete(projectId, id);
    return this.ReturnUnitNullableResult(res, new EntityNotFound(
      "Subscription Type Not Found", typeof(SubscriptionTypePrincipal), $"{projectId}:{id}"));
  }
}
