using CSharp_Result;

namespace Domain.Projects;

public class ProjectService(
  IProjectRepository repo,
  ITransactionManager tm
) : IProjectService
{
  public Task<Result<IEnumerable<ProjectPrincipal>>> Search(ProjectSearch search)
  {
    return repo.Search(search);
  }

  public Task<Result<Project?>> Get(Guid id)
  {
    return repo.Get(id);
  }

  public Task<Result<ProjectPrincipal>> Create(ProjectRecord record)
  {
    return tm.Start(() => repo.Create(record));
  }

  public Task<Result<ProjectPrincipal?>> Update(Guid id, ProjectRecord record)
  {
    return tm.Start(() => repo.Update(id, record));
  }

  public Task<Result<Unit?>> Delete(Guid id)
  {
    return repo.Delete(id);
  }
}
