using CarboxylicLithium;
using Domain.Marketing.Subscribers;

namespace Domain.Marketing.SubscriptionType;

public interface ISubscriptionTypeRepository
{
  Task<Result<IEnumerable<SubscriptionTypePrincipal>>> Search(SubscriptionTypeSearch search);

  Task<Result<SubscriptionType?>> Get(Guid projectId, string subscriptionType);

  Task<Result<SubscriptionTypePrincipal?>> Create(Guid projectId, string subName, SubscriptionTypeRecord record);

  Task<Result<SubscriptionTypePrincipal?>> Update(Guid projectId, string subName, SubscriptionTypeRecord record);

  Task<Result<Unit?>> Delete(Guid projectId, string subscriptionType);
}

