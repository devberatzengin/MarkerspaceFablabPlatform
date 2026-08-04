using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Subscription;
using MakerspaceFablabPlatform.Entities.Enums;

namespace MakerspaceFablabPlatform.Validators.SubscriptionValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.TargetType)
            .IsInEnum();

        RuleFor(x => x.Channel)
            .IsInEnum();

        RuleFor(x => x)
            .Must(x => x.CategoryId.HasValue ^ x.EquipmentId.HasValue);
        
        RuleFor(x => x.CategoryId)
            .NotNull()
            .When(x => x.TargetType == SubscriptionTargetType.Category);

        RuleFor(x => x.EquipmentId)
            .NotNull()
            .When(x => x.TargetType == SubscriptionTargetType.Equipment);
    }
}
