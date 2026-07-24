using MarkerspaceFablabPlatform.Dtos.Announcement;
using FluentValidation;

namespace MarkerspaceFablabPlatform.Validators.AnnouncementValidator;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{

    public UpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required")
            .NotEqual(Guid.Empty).WithMessage("Id cannot be empty");
        
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters long")
            .MaximumLength(100).WithMessage("Title must be less than 100 characters long");
        

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(500).WithMessage("Content must be less than 500 characters long");
        
        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required");
            
    }
    
}