using WorldPinball.Application.Features.GameSessions.Public.RecordBallScore.Commands;
namespace WorldPinball.Application.Features.GameSessions.Public.RecordBallScore;
public interface IRecordBallScoreHandler
{
    Task<RecordBallScoreResult> HandleAsync(RecordBallScoreCommand command, CancellationToken token = default);
}