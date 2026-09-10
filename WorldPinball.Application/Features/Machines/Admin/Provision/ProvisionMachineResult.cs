namespace WorldPinball.Application.Features.Machines.Admin.Provision;

public abstract record ProvisionMachineResult
{
    public sealed record Success(
        Guid MachineId,
        string EnrollmentToken)
        : ProvisionMachineResult;
    
    // Ip = Intellectual Property or License ex Resident Evil
    public sealed record UnknownIp(
        string IpSlug):ProvisionMachineResult;
    
    public sealed record DuplicateSerial(
        string SerialNumber
    ):ProvisionMachineResult;
    
    
    public sealed record NonceExist(
        string Nonce
    ):ProvisionMachineResult;
    
    public sealed record SameIdempotencyKey(
        Guid IdempotentKey
    ):ProvisionMachineResult;
    
}