namespace WorldPinball.Application.Contracts.Admin.Machine;

public sealed record ProvisionMachineRequest(
    string WorldPinballSerialNumber,
    string IpSlug,
    string? Name,
       string? PublicKey
);
