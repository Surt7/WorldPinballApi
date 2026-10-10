namespace WorldPinball.Api.Contracts.Sessions;

public sealed record RecordBallScoreRequest(
    int SlotNumber,
    int BallNumber,
    long Score
);