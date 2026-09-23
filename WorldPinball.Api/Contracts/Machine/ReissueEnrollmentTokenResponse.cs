namespace WorldPinball.Application.Contracts.Admin.Machine;

public sealed record ReissueEnrollmentTokenResponse(
    Guid MachineId,
    string EnrollmentToken,
    DateTime ExpiresAtUtc
);
