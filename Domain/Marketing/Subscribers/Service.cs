using CarboxylicLithium;

namespace Domain.Marketing.Subscribers;

public class SubscriberService(
  ISubscriberRepository subRepo) : ISubscriberService
{
  private string Normalize(string email)
  {
    return email.Trim().ToLowerInvariant();
  }

  public Task<Result<IEnumerable<SubscriberPrincipal>>> Search(SubscriberSearch search)
  {
    var req = search with { Email = search.Email == null ? null : this.Normalize(search.Email) };
    return subRepo.Search(req);
  }

  public Task<Result<Subscriber?>> Get(Guid projectId, string email)
  {
    var normalizedEmail = this.Normalize(email);
    return subRepo.Get(projectId, normalizedEmail);
  }

  public Task<Result<Unit?>> RecordSubscription(Guid projectId, string email, SubscriptionEvent subscription)
  {
    var normalizedEmail = this.Normalize(email);
    return subRepo.RecordSubscription(projectId, normalizedEmail, subscription);
  }

  public Task<Result<Unit?>> Delete(Guid projectId, string email)
  {
    var normalizedEmail = this.Normalize(email);
    return subRepo.Delete(projectId, normalizedEmail);
  }
}
