using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Announcement;

namespace MakerspaceFablabPlatform.Validators.AnnouncementValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters")
            .MinimumLength(5).WithMessage("Title cannot exceed 3 characters");

        RuleFor(x => x.Content)
            .MaximumLength(500).WithMessage("Content cannot exceed 500 characters");
        
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required")
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required");

    }
}