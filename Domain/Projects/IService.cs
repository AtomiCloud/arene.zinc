using CSharp_Result;

namespace Domain.Projects;

public interface IProjectService
{
  Task<Result<IEnumerable<ProjectPrincipal>>> Search(ProjectSearch search);
  Task<Result<Project?>> Get(Guid id);
  Task<Result<ProjectPrincipal>> Create(ProjectRecord record);
  Task<Result<ProjectPrincipal?>> Update(Guid id, ProjectRecord record);
  Task<Result<Unit?>> Delete(Guid id);
}
