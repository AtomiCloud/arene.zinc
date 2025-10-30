using CarboxylicLithium;

namespace Domain.Marketing.SubscriptionType;

public class SubscriptionTypeService(ISubscriptionTypeRepository subRepo) : ISubscriptionTypeService
{
  public Task<Result<IEnumerable<SubscriptionTypePrincipal>>> Search(SubscriptionTypeSearch search)
  {
    return subRepo.Search(search);
  }

  public Task<Result<SubscriptionType?>> Get(Guid projectId, string subscriptionType)
  {
    return subRepo.Get(projectId, subscriptionType);
  }

  public Task<Result<SubscriptionTypePrincipal?>> Create(Guid projectId, string subName, SubscriptionTypeRecord record)
  {
    return subRepo.Create(projectId, subName, record);
  }

  public Task<Result<SubscriptionTypePrincipal?>> Update(Guid projectId, string subName, SubscriptionTypeRecord record)
  {
    return subRepo.Update(projectId, subName, record);
  }

  public Task<Result<Unit?>> Delete(Guid projectId, string subscriptionType)
  {
    return subRepo.Delete(projectId, subscriptionType);
  }
}
