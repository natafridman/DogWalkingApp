using DogWalking.Application.DTOs;
using FluentValidation;

namespace DogWalking.Application.Validators;

/// <summary>Validates client creation: name, phone format, email format, address.</summary>
public sealed class CreateClientDtoValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^[\d\s\+\-\(\)]{6,20}$").WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.");
    }
}
