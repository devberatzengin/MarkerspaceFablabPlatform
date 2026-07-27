using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Equipment;

namespace MakerspaceFablabPlatform.Validators.EquipmentValidator;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    UpdateRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty().WithMessage("Id is required");
        
        RuleFor(request => request.RequiredUserLevel)
            .ExclusiveBetween((short)1, (short)10).WithMessage("Required user level is between 1 and 10");
        
    }
}