namespace WorldPinball.Application.Contracts.Admin.Machine;

public sealed record ProvisionMachineResponse(
    Guid MachineId,
    string EnrollmentToken
);
