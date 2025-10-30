using App.Error.V1;
using App.StartUp.Database;
using App.Utility;
using CarboxylicLithium;
using Domain.Projects;
using Domain.User;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;

namespace App.Modules.Projects.Data;

public class ProjectRepository(MainDbContext db, ILogger<ProjectRepository> logger) : IProjectRepository
{
  public async Task<Result<IEnumerable<ProjectPrincipal>>> Search(ProjectSearch search)
  {
    try
    {
      var query = db.Projects.AsQueryable();
      if (search.Name != null) query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search.Name}%"));
      if (search.Id != null) query = query.Where(x => x.Id == search.Id);
      return await query
        .Skip(search.Skip)
        .Take(search.Limit)
        .Select(x => x.ToPrincipal())
        .ToArrayAsync();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to search projects with params {@Params}", search.ToJson());
      throw;
    }
  }

  public async Task<Result<Project?>> Get(Guid id)
  {
    try
    {
      var found = await
        db.Projects.FirstOrDefaultAsync(x => x.Id == id);
      if (found == null) return (Project?)null;

      return found.ToDomain(0);
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get project with id {@Id}", id);
      throw;
    }
  }

  public async Task<Result<ProjectPrincipal>> Create(ProjectRecord record)
  {
    try
    {
      var data = record.ToData();
      var updated = await db.Projects.AddAsync(data);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to get project with {@Record}", record);
      throw;
    }
  }

  public async Task<Result<ProjectPrincipal?>> Update(Guid id, ProjectRecord record)
  {
    try
    {
      var v1 = await db.Projects.FirstOrDefaultAsync(x => x.Id == id);
      if (v1 == null) return (ProjectPrincipal?)null;
      v1 = v1.Update(record);
      var updated = db.Projects.Update(v1);
      await db.SaveChangesAsync();
      return updated.Entity.ToPrincipal();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to update project {@Id} with {@Record}", id, record);
      throw;
    }
  }

  public async Task<Result<Unit?>> Delete(Guid id)
  {
    try
    {
      var v1 = await db.Projects.FirstOrDefaultAsync(x => x.Id == id);
      if (v1 == null) return (Unit?)null;
      db.Projects.Remove(v1);
      await db.SaveChangesAsync();
      return new Unit();
    }
    catch (Exception e)
    {
      logger.LogError(e, "Failed to update project {@Id}", id);
      throw;
    }
  }
}
