namespace WorldPinball.Api.Contracts.Machine;

public sealed record ProvisionMachineRequest(
    string WorldPinballSerialNumber,
    string IpSlug,
    Guid  IdempotencyKey,
    string DisplayName,
       string? PublicKey
);
