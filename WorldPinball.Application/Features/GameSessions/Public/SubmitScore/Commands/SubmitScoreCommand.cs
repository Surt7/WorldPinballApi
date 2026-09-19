namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;

public sealed record SubmitScoreCommand(
        Guid SessionId,
        Guid DeviceId,
        long Score,
        int SlotNumber = 1
    );
    