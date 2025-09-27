using CSharp_Result;

namespace Domain.Marketing.Subscribers;

public interface ISubscriberService
{
  Task<Result<IEnumerable<SubscriberPrincipal>>> Search(SubscriberSearch search);
  Task<Result<Subscriber?>> Get(Guid projectId, string email);

  // Append a subscription log entry and recompute effective status
  Task<Result<Unit?>> RecordSubscription(Guid projectId, string email, SubscriptionEvent subscription);

  Task<Result<Unit?>> Delete(Guid projectId, string email);
}
