namespace WorldPinball.Api.Contracts.Sessions;

public sealed record RecordBallScoreResponse(
    Guid SessionId,
    int SlotNumber,
    int BallNumber,
    long Score
);