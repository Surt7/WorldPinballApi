using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;
namespace WorldPinball.Api.Validation.Public;
public class RecordBallScoreRequestValidator : AbstractValidator<RecordBallScoreRequest>
{
    public RecordBallScoreRequestValidator()
    {
        RuleFor(x => x.SlotNumber).InclusiveBetween(1, 4);
        RuleFor(x => x.BallNumber).InclusiveBetween(1, 5);
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
    }
}