using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Public.Dto;

public sealed record EnrollmentTokenDto(
    Guid TokenId,
    EnrollmentTokenType TokenType,
    string IssuedByUserId,
    DateTime IssuedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime? ConsumedAtUtc,
    Guid? ConsumedByDeviceId,
    int UsesCount,
    int MaxUses
);