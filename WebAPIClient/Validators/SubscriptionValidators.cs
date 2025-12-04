using FluentValidation;
using WebAPIClient.DTOs;

namespace WebAPIClient.Validators
{
    public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
    {
        public CreateSubscriptionRequestValidator()
        {
            RuleFor(x => x.PlanId)
                .GreaterThan(0).WithMessage("Plan ID must be a positive number");
        }
    }
}
