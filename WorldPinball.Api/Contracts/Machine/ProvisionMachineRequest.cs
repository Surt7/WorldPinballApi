namespace WorldPinball.Api.Contracts.Machine;

public sealed record ProvisionMachineRequest(
    string WorldPinballSerialNumber,
    string IpSlug,
    string DisplayName,
    string? PublicKey,
    int? ValidityHours
);