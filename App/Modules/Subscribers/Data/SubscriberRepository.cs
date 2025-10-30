using App.Error.V1;
using App.Modules.SubscriptionTypes.Data;
using App.StartUp.Database;
using App.Utility;
using CarboxylicLithium;
using Domain.Marketing.Subscribers;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Subscribers.Data;

public class SubscriberRepository(MainDbContext db, ILogger<SubscriberRepository> logger)
  : ISubscriberRepository
{
  public async Task<Result<IEnumerable<SubscriberPrincipal>>> Search(SubscriberSearch search)
  {
    try
    {
      var query = db.Subscribers.AsQueryable();
      if (search.ProjectId is not null)
        query = query.Where(x => x.ProjectId == search.ProjectId.Value);
      if (!string.IsNullOrWhiteSpace(search.Email))
        query = query.Where(x => EF.Functions.ILike(x.Email, $"%{search.Email}%"));

      // Filter by computed subscription types (enabled)
      if (search.SubscriptionTypeId is not null && search.SubscriptionTypeId.Any())
      {
        var types = search.SubscriptionTypeId.ToArray();
        query = query.Where(s => db.SubscriberTypeStatuses
          .Any(st => st.ProjectId == s.ProjectId && st.Email == s.Email && types.Contains(st.SubscriptionTypeId) && st.Enabled));
      }

      var rows = await query
        .OrderBy(x => x.Email)
        .Skip(search.Skip)
        .Take(search.Limit)
        .ToArrayAsync();

      // Gather statuses in one shot
      var keys = rows.Select(r => new { r.ProjectId, r.Email }).ToArray();
      var statuses = await db.SubscriberTypeStatuses
        .Where(st => keys.Contains(new { st.ProjectId, st.Email }))
        .ToArrayAsync();

      var result = rows
        .Select(r => r.ToPrincipal(statuses.Where(st => st.ProjectId == r.ProjectId && st.Email == r.Email)))
        .ToArray();

      return result;
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to search subscribers with {@Search}", search);
      throw;
    }
  }

  public async Task<Result<Subscriber?>> Get(Guid projectId, string email)
  {
    try
    {
      var sub = await db.Subscribers
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Email == email);
      if (sub is null) return (Subscriber?)null;

      var statuses = await db.SubscriberTypeStatuses
        .Where(st => st.ProjectId == projectId && st.Email == email)
        .ToArrayAsync();

      return new Subscriber { Principal = sub.ToPrincipal(statuses) };
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get subscriber {Project}:{Email}", projectId, email);
      throw;
    }
  }

  public async Task<Result<Unit?>> RecordSubscription(Guid projectId, string email, SubscriptionEvent subscription)
  {
    try
    {
      await using var tx = await db.Database.BeginTransactionAsync();

      // Validate subscription type existence at repo level
      var typeExists = await db.SubscriptionTypes.AnyAsync(t => t.ProjectId == projectId && t.Id == subscription.Type);
      if (!typeExists)
        return new EntityNotFound("Subscription type not found", typeof(SubscriptionTypeData), $"{projectId}:{subscription.Type}")
          .ToException();

      // Upsert subscriber
      var sub = await db.Subscribers
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Email == email);
      if (sub is null)
      {
        sub = SubscriberMapper.NewSubscriberData(projectId, email);
        await db.Subscribers.AddAsync(sub);
      }

      // Append log
      var log = subscription.ToEventData();
      log.ProjectId = projectId;
      log.Email = email;
      await db.SubscriptionEvents.AddAsync(log);

      // Update timezone counts and compute best timezone
      if (!string.IsNullOrWhiteSpace(subscription.Timezone))
      {
        var tz = subscription.Timezone.Trim();
        if (!sub.TimeZoneCounts.TryGetValue(tz, out var count)) count = 0;
        sub.TimeZoneCounts[tz] = count + 1;
        // pick max-count timezone (stable tie-breaker by string)
        var best = sub.TimeZoneCounts
          .OrderByDescending(kv => kv.Value)
          .ThenBy(kv => kv.Key, StringComparer.Ordinal)
          .First().Key;
        sub.TimeZone = best;
        db.Subscribers.Update(sub);
      }

      // Upsert computed status row
      var status = await db.SubscriberTypeStatuses
        .FirstOrDefaultAsync(st => st.ProjectId == projectId && st.Email == email && st.SubscriptionTypeId == subscription.Type);
      if (status is null)
      {
        status = subscription.ToTypeData();
        status.ProjectId = projectId;
        status.Email = email;
        await db.SubscriberTypeStatuses.AddAsync(status);
      }
      else
      {
        status.LegalBasis = (int)subscription.LegalBasis;
        status.Reason = subscription.Reason;
        status.UpdatedAt = subscription.Time;
        status.Enabled = subscription.LegalBasis != LegalBasis.None;
        db.SubscriberTypeStatuses.Update(status);
      }

      await db.SaveChangesAsync();
      await tx.CommitAsync();
      return new Unit();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(e, "Unique constraint while recording subscription {Project}:{Email}:{Type}", projectId, email,
        subscription.Type);
      throw;
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to record subscription {Project}:{Email}:{Type}", projectId, email, subscription.Type);
      throw;
    }
  }

  public async Task<Result<Unit?>> Delete(Guid projectId, string email)
  {
    try
    {
      var sub = await db.Subscribers
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Email == email);
      if (sub is null) return (Unit?)null;

      // Remove statuses and logs, then subscriber
      var statuses = db.SubscriberTypeStatuses.Where(st => st.ProjectId == projectId && st.Email == email);
      var logs = db.SubscriptionEvents.Where(e => e.ProjectId == projectId && e.Email == email);
      db.SubscriberTypeStatuses.RemoveRange(statuses);
      db.SubscriptionEvents.RemoveRange(logs);
      db.Subscribers.Remove(sub);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete subscriber {Project}:{Email}", projectId, email);
      throw;
    }
  }
}
