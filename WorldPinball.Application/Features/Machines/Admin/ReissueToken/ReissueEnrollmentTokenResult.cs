using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Admin.ReissueToken;

public abstract record ReissueEnrollmentTokenResult
{
    public sealed record Success(
        Guid MachineId,
        string EnrollmentToken,
        DateTime ExpiresAtUtc)
        : ReissueEnrollmentTokenResult;

    public sealed record MachineNotFound(
        Guid MachineId)
        : ReissueEnrollmentTokenResult;

    public sealed record InvalidMachineStatus(
        Guid MachineId,
        MachineStatus Status)
        : ReissueEnrollmentTokenResult;
}
