using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Equipment;

namespace MakerspaceFablabPlatform.Validators.EquipmentValidator;

public class ScheduleRequestValidator : AbstractValidator<ScheduleRequest>
{
    public ScheduleRequestValidator()
    {
    
        RuleFor(request => request.StartAt)
            .NotEmpty().WithMessage("Start date is required")
            .Must(startAt => startAt.ToUniversalTime() >= DateTime.UtcNow - TimeSpan.FromMinutes(5))
            .WithMessage("Start date cannot past day value");

        
        RuleFor(request => request.Span)
            .GreaterThan(TimeSpan.Zero).WithMessage("Rent time have to be greater than zero");
    }
}
