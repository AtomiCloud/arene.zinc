using App.Utility;
using FluentValidation;

namespace App.Modules.Projects.API.V1;

public class CreateProjectReqValidator : AbstractValidator<CreateProjectReq>
{
  public CreateProjectReqValidator()
  {
    this.RuleFor(x => x.Name)
      .NotNull()
      .NameValid();
    this.RuleFor(x => x.Open)
      .NotNull();
  }
}

public class UpdateProjectReqValidator : AbstractValidator<UpdateProjectReq>
{
  public UpdateProjectReqValidator()
  {
    this.RuleFor(x => x.Name)
      .NotNull()
      .NameValid();
    this.RuleFor(x => x.Open)
      .NotNull();
  }
}

public class ProjectSearchQueryValidator : AbstractValidator<SearchProjectQuery>
{
  public ProjectSearchQueryValidator()
  {
    
    this.RuleFor(x => x.Name)
      .MinimumLength(1)
      .Unless(x => x.Name == null);
    this.RuleFor(x => x.Limit)
      .Limit();
    this.RuleFor(x => x.Skip)
      .Skip();
  }
}
