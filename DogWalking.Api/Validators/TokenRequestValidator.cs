using DogWalking.Api.DTOs;
using FluentValidation;

namespace DogWalking.Api.Validators;

public sealed class TokenRequestValidator : AbstractValidator<TokenRequest>
{
    public TokenRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(4).WithMessage("Password must be at least 4 characters.");
    }
}
