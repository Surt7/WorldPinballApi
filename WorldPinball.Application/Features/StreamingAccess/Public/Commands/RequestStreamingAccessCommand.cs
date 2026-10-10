namespace WorldPinball.Application.Features.StreamingAccess.Public.Commands;

public sealed record RequestStreamingAccessCommand(
    Guid SessionId,
    Guid RequestedByUserId
);