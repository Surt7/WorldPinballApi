

namespace WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;

public sealed record SlotScoreDto(int SlotNumber, long Score);

public sealed record SubmitScoreCommand(
        Guid SessionId,
        Guid DeviceId,
        IReadOnlyList<SlotScoreDto> Scores
    );
    