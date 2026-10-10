namespace WorldPinball.Application.Features.GameSessions.Public.RecordBallScore;

public abstract record RecordBallScoreResult
{
    private RecordBallScoreResult() { }
    public sealed record Success(Guid SessionId, int SlotNumber, int BallNumber, long Score) : RecordBallScoreResult;
    public sealed record SessionNotFound(Guid SessionId) : RecordBallScoreResult;
    public sealed record DeviceMisMatch(Guid ExpectedDeviceId, Guid ActualDeviceId) : RecordBallScoreResult;
    public sealed record CannotRecordBallScore(string Reason) : RecordBallScoreResult;
}