namespace WorldPinball.Api.Contracts.Sessions;

public sealed record SubmitScoreRequest(
    Guid DeviceId,
    long Score,
    int SlotNumber = 1
);