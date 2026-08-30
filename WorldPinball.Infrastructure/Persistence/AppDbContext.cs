using Microsoft.EntityFrameworkCore;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MachineDevice> MachineDevices => Set<MachineDevice>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionProgress> MissionProgress => Set<MissionProgress>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<IpLicense> IpLicenses => Set<IpLicense>();
    public DbSet<GameMode> GameModes => Set<GameMode>();
    public DbSet<Domain.Entities.Machine> Machines => Set<Domain.Entities.Machine>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ScoreEntry> ScoreEntries => Set<ScoreEntry>();
    public DbSet<EnrollmentToken> EnrollmentTokens => Set<EnrollmentToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
