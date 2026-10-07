using Microsoft.Extensions.Configuration;
using Npgsql;

namespace WorldPinball.Logistics.Infrastructure;

public static class ConnectionStringExtensions
{
    public static string GetRequiredPostgresConnectionString(
        this IConfiguration config,
        string databaseName = "worldpinball_logistics")
    {
        var cs = config.GetConnectionString("LogisticsConnection")
                 ?? config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        
        var builder = new NpgsqlConnectionStringBuilder(cs);

        if (!string.IsNullOrWhiteSpace(databaseName))
        {
            builder.Database = databaseName;
        }
        
        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            var password = config["PostGres:Password"] ?? config["POSTGRES_PASSWORD"];
            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = password;
            }
        }
        
        return builder.ToString();
    }


}