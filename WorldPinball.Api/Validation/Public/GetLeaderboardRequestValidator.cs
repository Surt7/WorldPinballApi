using FluentValidation;
using WorldPinball.Api.Contracts.Leaderboard;

namespace WorldPinball.Api.Validation.Public;

public sealed class GetLeaderboardRequestValidator 
    : AbstractValidator<GetLeaderboardRequest>
{
    public GetLeaderboardRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 200)
            .WithMessage("Limit must be between 1 and 200.");
        
        RuleFor(x => x)
            .Must(x => (x.IpId.HasValue && x.ModeId.HasValue) ||
                       (!string.IsNullOrWhiteSpace(x.IpSlug) && !string.IsNullOrWhiteSpace(x.ModeCode)))
            .WithMessage("Either (IpId and ModeId) or (IpSlug and ModeCode) must be provided.");
        
    }
}