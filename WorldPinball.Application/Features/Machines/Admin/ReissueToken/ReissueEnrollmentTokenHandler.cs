using Microsoft.Extensions.Logging;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Admin.ReissueToken;

public sealed class ReissueEnrollmentTokenHandler(
    IMachineWriteRepository machineWriteRepository,
    ISecretService secretService,
    ILogger<ReissueEnrollmentTokenHandler> logger,
    TimeProvider timeProvider
) : IReissueEnrollmentTokenHandler
{
    private static class ErrorMessages
    {
        public const string MachineNotFound = "Machine {0} was not found.";
        public const string InvalidMachineStatus = "Machine {0} is in status {1} (expected PendingEnrollment).";
    }

    private static readonly EventId EnrollmentTokenReissued =
        new(2050, nameof(EnrollmentTokenReissued));

    private static readonly EventId MachineNotFound =
        new(2051, nameof(MachineNotFound));

    private static readonly EventId MachineStatusInvalid =
        new(2052, nameof(MachineStatusInvalid));

    public async Task<ReissueEnrollmentTokenResult> HandleAsync(
        ReissueEnrollmentTokenCommand cmd,
        CancellationToken ct = default)
    {
        var machine = await machineWriteRepository.GetByIdAsync(cmd.MachineId, ct);
        if (machine is null)
        {
            logger.LogWarning(
                MachineNotFound,
                ErrorMessages.MachineNotFound,
                cmd.MachineId);
            return new ReissueEnrollmentTokenResult.MachineNotFound(cmd.MachineId);
        }

        if (machine.Status != MachineStatus.PendingEnrollment)
        {
            logger.LogWarning(
                MachineStatusInvalid,
                ErrorMessages.InvalidMachineStatus,
                cmd.MachineId,
                machine.Status);
            return new ReissueEnrollmentTokenResult.InvalidMachineStatus(cmd.MachineId, machine.Status);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddHours(24);

        var secret = secretService.Generate();

        var enrollmentToken = new EnrollmentToken(
            tokenHash: secret.HashHex,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: cmd.IssuedByUserId,
            issuedAt: now,
            expiresAt: expiresAt,
            machineId: machine.MachineId
        );

        await machineWriteRepository.AddEnrollmentTokenAsync(enrollmentToken, ct);

        logger.LogInformation(
            EnrollmentTokenReissued,
            "Enrollment token reissued for machine {MachineId} by user {UserId}.",
            machine.MachineId,
            cmd.IssuedByUserId);

        return new ReissueEnrollmentTokenResult.Success(
            MachineId: machine.MachineId,
            EnrollmentToken: secret.PlainBase64,
            ExpiresAtUtc: expiresAt
        );
    }
}
