using FluentValidation;
using MakerspaceFablabPlatform.Dtos.Equipment;

namespace MakerspaceFablabPlatform.Validators.EquipmentValidator;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    CreateRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(request => request.Type)
            .NotEmpty().WithMessage("Type is required");

        RuleFor(request => request.PlacementType)
            .NotEmpty().WithMessage("Placement type is required");

        RuleFor(request => request.RequiredUserLevel)
            .ExclusiveBetween((short)1, (short)10).WithMessage("Required user level is between 1 and 10");
    }
}