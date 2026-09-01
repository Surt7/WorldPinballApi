namespace WorldPinball.Application.Features.Machines.Admin.Commands;

public sealed record ProvisionMachineCommand(
    string WorldPinballSerialNumber,
    string IpSlug,
    string DisplayName,
    string IssuedByUserId);
