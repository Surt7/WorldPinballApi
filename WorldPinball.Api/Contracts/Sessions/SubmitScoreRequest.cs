namespace WorldPinball.Api.Contracts.Sessions;

public sealed record SlotScoreRequest(int SlotNumber, long Score);

public sealed record SubmitScoreRequest(
    Guid DeviceId,
    List<SlotScoreRequest> Scores
);