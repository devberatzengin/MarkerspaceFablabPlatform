using MarkerspaceFablabPlatform.Dtos.Category;
using FluentValidation;

namespace MarkerspaceFablabPlatform.Validators.CategoryValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(20).WithMessage("Name cannot exceed 20 characters")
            .MinimumLength(3).WithMessage("Name cannot exceed 3 characters");
        
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .IsInEnum().WithMessage("Type is required");
    }
}