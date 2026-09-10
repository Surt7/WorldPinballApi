using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Public.Dto;

public sealed record EnrollmentTokenDto(
    string TokenHash,
    EnrollmentTokenType TokenType,
    Guid? MachineId,
    Guid? DeviceId 
    );