using MakerspaceFablabPlatform.Dtos.User;
using FluentValidation;

namespace MakerspaceFablabPlatform.Validators.UserValidator;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{10,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}