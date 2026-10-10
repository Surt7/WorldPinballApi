namespace WorldPinball.Application.Features.GameSessions.Public.RecordBallScore.Commands;

public record RecordBallScoreCommand(
    Guid SessionId,
    Guid DeviceId,
    int SlotNumber,
    int BallNumber,
    long Score
    );