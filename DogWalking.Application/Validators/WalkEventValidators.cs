using DogWalking.Application.DTOs;
using FluentValidation;

namespace DogWalking.Application.Validators;

/// <summary>Validates walk requests: future date, valid dog, duration between 15–480 min.</summary>
public sealed class CreateWalkEventDtoValidator : AbstractValidator<CreateWalkEventDto>
{
    public CreateWalkEventDtoValidator()
    {
        RuleFor(x => x.DogId)
            .GreaterThan(0).WithMessage("A valid dog must be selected.");

        RuleFor(x => x.WalkDate)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Walk date cannot be in the past.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 480)
            .WithMessage("Duration must be between 15 and 480 minutes.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Walk zone is required.");
    }
}
