using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;

namespace WorldPinball.Infrastructure.Security.Authentification;

public sealed class MachineClaimsTransformation(
    IMachineStatusCacheService cache,
    IMachineReadRepository repository)
    : IClaimsTransformation
{
    private readonly IMachineStatusCacheService _cache = cache;
    private readonly IMachineReadRepository _repository = repository;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {

        if (principal.HasClaim(c => c.Type == "machine_status"))
        {
            return principal;
        }
        var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // we search  machine Guid
        if (!Guid.TryParse(sub, out var machineId))
        {
            return principal;
        }

        var cachedStatus = await _cache.GetStatusAsync(machineId);
        if (cachedStatus is not null)
        {
            if (principal.Identity is not ClaimsIdentity identity)
            {
                return principal;
            }
            
            identity.AddClaim(new Claim("machine_status", cachedStatus));
            return principal;
        }
        
        var machine = await _repository.GetMachinesWithId(machineId);
        if (machine is null)
        {
            return principal;
        }
        
        var status = machine.MachineStatus.ToString();
        await _cache.SetStatusAsync(machineId, status, TimeSpan.FromMinutes(5));
        
        
        if (principal.Identity is ClaimsIdentity id)
        {
            id.AddClaim(new Claim("machine_status", status));
        }
        return principal;
    }
}