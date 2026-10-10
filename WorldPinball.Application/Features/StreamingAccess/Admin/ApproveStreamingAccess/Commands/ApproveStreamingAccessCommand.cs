namespace WorldPinball.Application.Features.StreamingAccess.Admin.Commands;

public sealed record ApproveStreamingAccessCommand(
    Guid RequestId,
    TimeSpan Duration
);