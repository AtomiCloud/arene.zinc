using System.Collections;
using App.Error.V1;
using App.Modules.Projects.Data;
using App.StartUp.Database;
using App.Utility;
using CSharp_Result;
using Domain.Marketing.SubscriptionType;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.SubscriptionTypes.Data;

public class SubscriptionTypeRepository(MainDbContext db, ILogger<SubscriptionTypeRepository> logger)
  : ISubscriptionTypeRepository
{
  public async Task<Result<IEnumerable<SubscriptionTypePrincipal>>> Search(SubscriptionTypeSearch search)
  {
    try
    {
      var query = db.SubscriptionTypes.AsQueryable();

      if (search.Guid.HasValue)
        query = query.Where(x => x.ProjectId == search.Guid.Value);
      if (!string.IsNullOrWhiteSpace(search.Name))
        query = query.Where(x => EF.Functions.ILike(x.Id, $"%{search.Name}%"));
      if (!string.IsNullOrWhiteSpace(search.Desc))
        query = query.Where(x => EF.Functions.ILike(x.Desc, $"%{search.Desc}%"));

      return await query
        .OrderBy(x => x.Id)
        .Skip(search.Skip)
        .Take(search.Limit)
        .Select(x => x.ToPrincipal())
        .ToArrayAsync();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to search subscription types with params {@Params}", search.ToJson());
      throw;
    }
  }

  public async Task<Result<SubscriptionType?>> Get(Guid projectId, string subscriptionType)
  {
    try
    {
      var found = await db.SubscriptionTypes
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == subscriptionType);
      return found?.ToDomain(0);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get subscription type {Type} for project {Project}", subscriptionType, projectId);
      throw;
    }
  }

  public async Task<Result<SubscriptionTypePrincipal?>> Create(Guid projectId, string subName,
    SubscriptionTypeRecord record)
  {
    try
    {
      // Repo-level validation: ensure project exists
      var exists = await db.Projects.AnyAsync(p => p.Id == projectId);
      if (!exists)
        return new EntityNotFound("Project not found", typeof(ProjectData), projectId.ToString())
          .ToException();

      var data = record.ToData();
      data.Id = subName;
      data.ProjectId = projectId;
      var added = await db.SubscriptionTypes.AddAsync(data);
      await db.SaveChangesAsync();
      return added.Entity.ToPrincipal();
    }
    catch (UniqueConstraintException e)
    {
      logger.LogError(e,
        "Failed to create subscription type due to unique constraint: project {ProjectId}, type {Type}", projectId,
        subName);
      return new EntityConflict("Subscription type already exists for project", typeof(SubscriptionTypePrincipal))
        .ToException();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to create subscription type {Type} for project {Project}", subName, projectId);
      throw;
    }
  }

  public async Task<Result<SubscriptionTypePrincipal?>> Update(Guid projectId, string subName,
    SubscriptionTypeRecord record)
  {
    try
    {
      var v1 = await db.SubscriptionTypes
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == subName);
      if (v1 is null) return (SubscriptionTypePrincipal?)null;
      var v2 = v1.Update(record);
      var updated = db.SubscriptionTypes.Update(v2);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to update subscription type {Type} for project {Project}", subName, projectId);
      throw;
    }
  }

  public async Task<Result<Unit?>> Delete(Guid projectId, string subscriptionType)
  {
    try
    {
      var found = await db.SubscriptionTypes
        .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Id == subscriptionType);
      if (found is null) return (Unit?)null;
      db.SubscriptionTypes.Remove(found);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to delete subscription type {Type} for project {Project}", subscriptionType,
        projectId);
      throw;
    }
  }
}
