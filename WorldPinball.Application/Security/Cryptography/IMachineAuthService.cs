namespace WorldPinball.Application.Security.Cryptography;



/// <summary>
/// Verifies that a signed HTTP request from a pinball machine is authentic, within the valid time window,
/// and comes from an active and attached device.
/// </summary>
public interface IMachineAuthService
{
    Task<MachineAuthResult> ValidateAsync(MachineAuthContext context, CancellationToken ct = default);
}
