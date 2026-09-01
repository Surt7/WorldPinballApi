namespace WorldPinball.Application.Features.GameModes.Public.Dtos;

public sealed record GameModePublicDto(
    Guid ModeId,
    string Code,
    string Name,
    string? Description
);
