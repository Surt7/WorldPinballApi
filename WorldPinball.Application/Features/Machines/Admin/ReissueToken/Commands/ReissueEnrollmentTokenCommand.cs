namespace WorldPinball.Application.Features.Machines.Admin.ReissueToken.Commands;

public sealed record ReissueEnrollmentTokenCommand(
    Guid MachineId,
    string IssuedByUserId);
