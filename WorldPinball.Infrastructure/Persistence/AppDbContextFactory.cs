using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace WorldPinball.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var basePath = FindProjectDir("WorldPinball.Api.csproj");


        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(userSecretsId: "ce079cb4-882b-43d0-bf92-82cdc8f8e810", reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var cs = configuration.GetRequiredPostgresConnectionString();


        optionsBuilder
            .UseNpgsql(cs)
            .UseSnakeCaseNamingConvention();

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string FindProjectDir(string csprojName)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (dir != null)
        {
            // cherche dans ce dossier et ses sous-dossiers
            var match = dir.GetFiles(csprojName, SearchOption.AllDirectories).FirstOrDefault();
            if (match != null)
                return match.Directory!.FullName;

            dir = dir.Parent;
        }

        throw new InvalidOperationException($"Could not find {csprojName} from current directory.");
    }
}
