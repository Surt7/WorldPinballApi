namespace WorldPinball.Application.Features.Machines.Admin;

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
    
}