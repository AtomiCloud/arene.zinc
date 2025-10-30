using System.Net.Mime;
using App.Error.V1;
using App.Modules.Common;
using App.Utility;
using Asp.Versioning;
using CarboxylicLithium;
using Domain.Marketing.Subscribers;
using Microsoft.AspNetCore.Mvc;

namespace App.Modules.Subscribers.API.V1;

[ApiVersion(1.0)]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SubscriberController(
  ISubscriberService service,
  SearchSubscriberQueryValidator searchValidator,
  RecordSubscriptionReqValidator recordValidator
) : AtomiControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<SubscriberPrincipalRes>>> Search([FromQuery] SearchSubscriberQuery query)
  {
    var x = await searchValidator
      .ValidateAsyncResult(query, "Invalid SearchSubscriberQuery")
      .ThenAwait(q => service.Search(q.ToDomain()))
      .Then(x => x.Select(u => u.ToRes()), Errors.MapNone);
    return this.ReturnResult(x);
  }

  [HttpGet("{projectId:guid}/{email}")]
  public async Task<ActionResult<SubscriberRes>> Get(Guid projectId, string email)
  {
    var subscriber = await service.Get(projectId, email)
      .Then(x => x?.ToRes(), Errors.MapNone);

    return this.ReturnNullableResult(subscriber, new EntityNotFound(
      "Subscriber Not Found", typeof(Subscriber), $"{projectId}/{email}"));
  }

  [HttpPost("{projectId:guid}/{email}/subscriptions")]
  public async Task<ActionResult> RecordSubscription(Guid projectId, string email, [FromBody] RecordSubscriptionReq req)
  {
    var result = await recordValidator
      .ValidateAsyncResult(req, "Invalid RecordSubscriptionReq")
      .ThenAwait(x => service.RecordSubscription(projectId, email, x.ToDomain()));

    return this.ReturnUnitNullableResult(result, new EntityNotFound(
      "Subscriber Not Found", typeof(Subscriber), $"{projectId}/{email}"));
  }

  [HttpDelete("{projectId:guid}/{email}")]
  public async Task<ActionResult> Delete(Guid projectId, string email)
  {
    var result = await service.Delete(projectId, email);
    return this.ReturnUnitNullableResult(result, new EntityNotFound(
      "Subscriber Not Found", typeof(Subscriber), $"{projectId}/{email}"));
  }
}
