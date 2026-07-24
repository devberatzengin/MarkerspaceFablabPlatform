using MarkerspaceFablabPlatform.Dtos.User;
using FluentValidation;

namespace MarkerspaceFablabPlatform.Validators.UserValidator;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?\d{10,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}