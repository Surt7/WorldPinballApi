namespace WorldPinball.Application.Features.Machines.Admin.Provision.Commands;

public sealed record ProvisionMachineCommand(
    string WorldPinballSerialNumber,
    string IpSlug,
    string DisplayName,
    Guid IdempotencyKey,
    string IssuedByUserId);
