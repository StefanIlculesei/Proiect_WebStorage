using FluentValidation;
using WebAPIClient.DTOs;

namespace WebAPIClient.Validators
{
    public class FileUploadRequestValidator : AbstractValidator<FileUploadRequest>
    {
        public FileUploadRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

            RuleFor(x => x.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than 0");
        }
    }

    public class FileUpdateRequestValidator : AbstractValidator<FileUpdateRequest>
    {
        public FileUpdateRequestValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required")
                .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");
        }
    }
}
