using FluentValidation;
using WebMVC_Plans.Models;

namespace WebMVCAdmin.Validators
{
    public class CreateSubscriptionViewModelValidator : AbstractValidator<CreateSubscriptionViewModel>
    {
        public CreateSubscriptionViewModelValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage("Please select a valid user.");

            RuleFor(x => x.PlanId)
                .GreaterThan(0).WithMessage("Please select a valid plan.");

            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).When(x => x.EndDate.HasValue)
                .WithMessage("End date must be after the start date.");
        }
    }
}
