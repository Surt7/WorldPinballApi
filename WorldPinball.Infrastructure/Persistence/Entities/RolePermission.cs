namespace WorldPinball.Infrastructure.Persistence.Entities;

public sealed class RolePermission
{
    public string Role { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
}