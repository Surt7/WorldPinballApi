using Microsoft.EntityFrameworkCore;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Infrastructure.Persistence.Entities;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class RolePermissionRepository(AppDbContext context) : IRolePermissionRepository
{
    public async Task<bool> ExistsAsync(string role, string permission, CancellationToken ct = default) =>
        await context.RolePermissions.AnyAsync(rp => rp.Role == role && rp.Permission == permission, ct);

    public async Task AddAsync(string role, string permission, CancellationToken ct = default)
    {
        context.RolePermissions.Add(new RolePermission { Role = role, Permission = permission });
        await context.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(string role, string permission, CancellationToken ct = default)
    {
        var entity = await context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.Role == role && rp.Permission == permission, ct);

        if (entity is not null)
        {
            context.RolePermissions.Remove(entity);
            await context.SaveChangesAsync(ct);
        }
    }
}
