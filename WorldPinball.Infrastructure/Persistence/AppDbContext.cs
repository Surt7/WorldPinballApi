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
    public DbSet<CaCertificate> CaCertificates => Set<CaCertificate>();
    public DbSet<DeviceCertificate> DeviceCertificates => Set<DeviceCertificate>();
    public DbSet<CertificateRevocation> CertificateRevocations => Set<CertificateRevocation>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<ScoreEntry> ScoreEntries => Set<ScoreEntry>();
    public DbSet<EnrollmentToken> EnrollmentTokens => Set<EnrollmentToken>();
    public DbSet<OperatorToken> OperatorTokens => Set<OperatorToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<GameSessionPlayer> GameSessionPlayers => Set<GameSessionPlayer>();
    public DbSet<StreamingPartner> StreamingPartners => Set<StreamingPartner>();
    public DbSet<StreamingAccessRequest> StreamingAccessRequests => Set<StreamingAccessRequest>();
    
    public DbSet<StreamAccessGrant> StreamAccessGrants => Set<StreamAccessGrant>();
    
    public DbSet<StreamSubscription> StreamSubscriptions => Set<StreamSubscription>();
    public DbSet<WorldPinball.Infrastructure.Persistence.Entities.RolePermission> RolePermissions => Set<WorldPinball.Infrastructure.Persistence.Entities.RolePermission>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
