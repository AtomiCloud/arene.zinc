using App.Utility;
using FluentValidation;

namespace App.Modules.SubscriptionTypes.API.V1;

public class CreateSubscriptionTypeReqValidator : AbstractValidator<CreateSubscriptionTypeReq>
{
  public CreateSubscriptionTypeReqValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .NameValid();
    RuleFor(x => x.Desc)
      .NotEmpty()
      .MaximumLength(512);
  }
}

public class UpdateSubscriptionTypeReqValidator : AbstractValidator<UpdateSubscriptionTypeReq>
{
  public UpdateSubscriptionTypeReqValidator()
  {
    RuleFor(x => x.Desc)
      .NotEmpty()
      .MaximumLength(512);
  }
}

public class SubscriptionTypeSearchQueryValidator : AbstractValidator<SearchSubscriptionTypeQuery>
{
  public SubscriptionTypeSearchQueryValidator()
  {
    RuleFor(x => x.Name)
      .MinimumLength(1)
      .Unless(x => x.Name == null);
    RuleFor(x => x.Desc)
      .MinimumLength(1)
      .Unless(x => x.Desc == null);
    RuleFor(x => x.Limit)
      .Limit()
      .Unless(x => x == null);
    RuleFor(x => x.Skip)
      .Skip()
      .Unless(x => x == null);
  }
}
