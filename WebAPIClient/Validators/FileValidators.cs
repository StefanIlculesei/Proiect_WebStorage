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

            // File size validation is performed on request.File.Length in the controller
            // FileSize DTO property is optional and not sent from frontend FormData
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
