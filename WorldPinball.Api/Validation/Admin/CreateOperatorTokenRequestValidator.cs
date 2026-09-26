using FluentValidation;
using WorldPinball.Api.Contracts.Machine;

namespace WorldPinball.Api.Validation.Admin;

public sealed class CreateOperatorTokenRequestValidator
: AbstractValidator<CreateOperatorTokenRequest>
{
    
    public CreateOperatorTokenRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Token name is required.")
            .MaximumLength(100)
            .WithMessage("Token name cannot exceed 100 characters.");


        When(x => x.ValidityDays.HasValue, () =>
        {
            RuleFor(x => x.ValidityDays)
                .InclusiveBetween(1, 365)
                .WithMessage("Validity days must be between 1 and 365.");
        });
    }
    
}