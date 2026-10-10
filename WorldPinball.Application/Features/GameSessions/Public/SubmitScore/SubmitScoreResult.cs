namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore;

public abstract record SubmitScoreResult
{
    private SubmitScoreResult() { }

    public sealed record Success(IReadOnlyList<Guid> ScoreEntryIds) : SubmitScoreResult;

    public sealed record SessionNotFound(Guid GameSessionId) : SubmitScoreResult;
    
    public sealed record SessionAlreadyEnded(Guid GameSessionId) : SubmitScoreResult;
    
    public sealed record SessionNotStarted(Guid GameSessionId) : SubmitScoreResult;
    public sealed record DeviceMisMatch(Guid ExpectedDeviceId, Guid ActualDeviceId) : SubmitScoreResult;

    public sealed record SlotNotFound(int SlotNumber) : SubmitScoreResult;

    public sealed record EmptyScores : SubmitScoreResult;
}