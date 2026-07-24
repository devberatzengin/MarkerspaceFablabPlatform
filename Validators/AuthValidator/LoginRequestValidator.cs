using MakerspaceFablabPlatform.Dtos.Auth;
using FluentValidation;

namespace MakerspaceFablabPlatform.Validators.AuthValidator;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(100).WithMessage("Maximum length of email is 100")
            .MinimumLength(3).WithMessage("Minimum length of email is 3");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Minimum length of password is 8")
            .MaximumLength(20).WithMessage("Maximum length of password is 20");
        
    }
}