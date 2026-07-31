using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Payment;

namespace MakerspaceFablabPlatform.Validators.PaymentValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{

    public CreateRequestValidator()
    {
        RuleFor(request => request.EquipmentRentalId)
            .NotEmpty()
            .NotNull();
        
        RuleFor(request => request.UserId)
            .NotEqual(Guid.Empty)
            .When(request => request.UserId.HasValue);
        
    }
    
}