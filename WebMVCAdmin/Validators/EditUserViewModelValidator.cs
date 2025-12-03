using FluentValidation;
using WebMVC_Plans.Models;

namespace WebMVCAdmin.Validators
{
    public class EditUserViewModelValidator : AbstractValidator<EditUserViewModel>
    {
        public EditUserViewModelValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required.")
                .Must(role => role == "user" || role == "admin").WithMessage("Role must be either 'user' or 'admin'.");
                
            RuleFor(x => x.StorageUsed)
                .GreaterThanOrEqualTo(0).WithMessage("Storage used cannot be negative.");
        }
    }
}
