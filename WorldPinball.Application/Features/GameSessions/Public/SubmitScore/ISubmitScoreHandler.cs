
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;

namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore
{
    public interface ISubmitScoreHandler
    {
        Task<SubmitScoreResult> HandleAsync(
            SubmitScoreCommand command, CancellationToken token = default
        );
    }
}