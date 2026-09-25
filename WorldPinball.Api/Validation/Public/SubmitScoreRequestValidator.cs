using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;

namespace WorldPinball.Api.Validation.Public;

public sealed class SubmitScoreRequestValidator : AbstractValidator<SubmitScoreRequest>
{
    public SubmitScoreRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty()
            .WithMessage("DeviceId is required.");

        RuleFor(x => x.Scores)
            .NotEmpty()
            .WithMessage("Scores list cannot be empty.");

        RuleForEach(x => x.Scores).ChildRules(score =>
        {
            score.RuleFor(s => s.Score)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Score must be greater than or equal to 0.");

            score.RuleFor(s => s.SlotNumber)
                .InclusiveBetween(1, 4)
                .WithMessage("SlotNumber must be between 1 and 4.");
        });
    }
}