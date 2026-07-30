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
            .NotEmpty()
            .NotNull();

        RuleFor(request => request.RentalFee)
            .NotEmpty()
            .NotNull();
        
        RuleFor(request => request.PaymentMethod)
            .NotEmpty()
            .NotNull();
        
    }
    
}