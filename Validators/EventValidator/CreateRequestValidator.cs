using FluentValidation;
using MarkerspaceFablabPlatform.Dtos.Event;
namespace MarkerspaceFablabPlatform.Validators.EventValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required")
            .NotEqual(Guid.Empty).WithMessage("CategoryId is required");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters");
        
        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");
        
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start Date is required")
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Start Date cannot be in the past");

        // BUGFIX: eski kural EndDate <= Now istiyordu; gelecekteki hiçbir etkinlik geçerli olamazdı
        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End Date is required")
            .GreaterThan(_ => DateTime.UtcNow).WithMessage("End Date cannot be in the past")
            .GreaterThan(x => x.StartDate).WithMessage("End date cannot be before start date");
    }
    
}