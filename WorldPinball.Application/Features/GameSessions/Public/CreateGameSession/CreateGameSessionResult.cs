namespace WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;

public abstract record CreateGameSessionResult
{
    private CreateGameSessionResult() { }
    public sealed record Success(
        Guid SessionId,
        string Nonce,
        DateTimeOffset CreatedAtUtc
    ) : CreateGameSessionResult;
    public sealed record DeviceNotFound(Guid DeviceId) : CreateGameSessionResult;
    public sealed record DeviceNotActive(Guid DeviceId) : CreateGameSessionResult;
    public sealed record MachineNotAttached(Guid DeviceId) : CreateGameSessionResult;
    public sealed record ModeNotFound(string ModeCode) : CreateGameSessionResult;
}