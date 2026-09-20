namespace WorldPinball.Infrastructure;

using Microsoft.Extensions.Configuration;
using Npgsql;

public static class ConnectionStringExtensions
{
    public static string GetRequiredPostgresConnectionString(this IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        var password = config["PostGres:Password"] ?? config["POSTGRES_PASSWORD"];

        return new NpgsqlConnectionStringBuilder(cs)
        {
            Password = string.IsNullOrWhiteSpace(password) ? null : password
        }.ConnectionString;
    }
}