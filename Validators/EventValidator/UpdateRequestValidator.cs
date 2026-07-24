using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Event;
namespace MakerspaceFablabPlatform.Validators.EventValidator;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required")
            .NotEqual(Guid.Empty).WithMessage("Id is required");
    }
    
}