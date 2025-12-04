using FluentValidation;
using WebAPIClient.DTOs;

namespace WebAPIClient.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }
}
