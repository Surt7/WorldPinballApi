using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;

namespace WorldPinball.Api.Validation.Admin;

public sealed class SubmitScoreRequestValidator : AbstractValidator<SubmitScoreRequest>
{
    public SubmitScoreRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("DeviceId is required.");
        
        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Score must be greater than or equal to 0.");
        
        // On a session on 4 place available.
        RuleFor(x => x.SlotNumber)
            .InclusiveBetween(1, 4)
            .WithMessage("SlotNumber must be between 1 and 4.");
    }
}