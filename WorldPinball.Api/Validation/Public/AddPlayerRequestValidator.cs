using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;

namespace WorldPinball.Api.Validation.Public;

public sealed class AddPlayerRequestValidator : AbstractValidator<AddPlayerRequest>
{
    public AddPlayerRequestValidator()
    {
        RuleFor(x => x.SlotNumber)
            .InclusiveBetween(1, 4)
            .WithMessage("Slot number must be between 1 and 4.");

        RuleFor(x => x.GuestInitials)
            .MaximumLength(3)
            .WithMessage("Guest initials cannot exceed 3 characters.")
            .When(x => !string.IsNullOrEmpty(x.GuestInitials));
    }
}
