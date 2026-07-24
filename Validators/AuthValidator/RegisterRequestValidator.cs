using MarkerspaceFablabPlatform.Dtos.Auth;
using FluentValidation;

namespace MarkerspaceFablabPlatform.Validators.AuthValidator;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{

    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("User name is required")
            .MinimumLength(6).WithMessage("User name must be at least 6 characters long")
            .MaximumLength(30).WithMessage("User name must be no more than 30 characters long");
        
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(100).WithMessage("Maximum length of email is 100")
            .MinimumLength(3).WithMessage("Minimum length of email is 3");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Minimum length of password is 8")
            .MaximumLength(20).WithMessage("Maximum length of password is 20");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(20).WithMessage("Maximum length of first name is 20");
        
        RuleFor(x => x.LastName)
            .MaximumLength(20).WithMessage("Maximum length of last name is 20");
        
    }
    
}