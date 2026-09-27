namespace WorldPinball.Infrastructure;

using Microsoft.Extensions.Configuration;
using Npgsql;

public static class ConnectionStringExtensions
{
    public static string GetRequiredPostgresConnectionString(this IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        var builder = new NpgsqlConnectionStringBuilder(cs);
        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            var password = config["PostGres:Password"] ?? config["POSTGRES_PASSWORD"];
            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = password;
            }
        }

        return builder.ConnectionString;
    }
}