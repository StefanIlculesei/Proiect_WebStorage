using FluentValidation;
using WebMVC_Plans.Models;

namespace WebMVCAdmin.Validators
{
    public class PlanViewModelValidator : AbstractValidator<PlanViewModel>
    {
        public PlanViewModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Plan name is required.")
                .Length(2, 100).WithMessage("Plan name must be between 2 and 100 characters.");

            RuleFor(x => x.LimitSizeGB)
                .GreaterThan(0).WithMessage("Storage limit must be greater than 0 GB.");

            RuleFor(x => x.MaxFileSizeMB)
                .GreaterThan(0).WithMessage("Max file size must be greater than 0 MB.");

            RuleFor(x => x.BillingPeriod)
                .NotEmpty().WithMessage("Billing period is required.")
                .Must(x => new[] { "Monthly", "Yearly", "Weekly" }.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Billing period must be Monthly, Yearly, or Weekly.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency code must be 3 characters (e.g., USD, EUR).");
        }
    }
}
