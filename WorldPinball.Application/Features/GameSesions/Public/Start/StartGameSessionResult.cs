namespace WorldPinball.Application.Features.GameSesions.Public.Start;

public abstract record StartGameSessionResult
{
    private StartGameSessionResult() { }
    public sealed record Success(
        Guid SessionId,
        string Nonce,
        DateTimeOffset StartedAtUtc
    ) : StartGameSessionResult;
    public sealed record DeviceNotFound(Guid DeviceId) : StartGameSessionResult;
    public sealed record DeviceNotActive(Guid DeviceId) : StartGameSessionResult;
    public sealed record MachineNotAttached(Guid DeviceId) : StartGameSessionResult;
    public sealed record ModeNotFound(string ModeCode) : StartGameSessionResult;
}