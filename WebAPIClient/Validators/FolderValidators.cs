using FluentValidation;
using WebAPIClient.DTOs;

namespace WebAPIClient.Validators
{
    public class FolderRequestValidator : AbstractValidator<FolderRequest>
    {
        public FolderRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Folder name is required")
                .MaximumLength(255).WithMessage("Folder name cannot exceed 255 characters");
        }
    }
}
