using Application.DTOs.Auth;
using FluentValidation;

namespace Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUserName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128);
    }
}
