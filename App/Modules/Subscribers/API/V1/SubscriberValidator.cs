using FluentValidation;

namespace App.Modules.Subscribers.API.V1;

public class SearchSubscriberQueryValidator : AbstractValidator<SearchSubscriberQuery>
{
  public SearchSubscriberQueryValidator()
  {
    RuleFor(x => x.Limit).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000).When(x => x.Limit != null);
    RuleFor(x => x.Skip).GreaterThanOrEqualTo(0).When(x => x.Skip != null);
    RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
  }
}

public class RecordSubscriptionReqValidator : AbstractValidator<RecordSubscriptionReq>
{
  public RecordSubscriptionReqValidator()
  {
    RuleFor(x => x.Type).NotEmpty().MaximumLength(128);
    RuleFor(x => x.LegalBasis).InclusiveBetween(0, 6); // LegalBasis enum range
    RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    RuleFor(x => x.Timezone).MaximumLength(128).When(x => x.Timezone != null);
  }
}
