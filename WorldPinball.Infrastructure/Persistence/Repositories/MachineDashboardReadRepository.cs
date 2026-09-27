using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class MachineDashboardReadRepository(IConfiguration config) : IMachineDashboardReadRepository
{
    private const string Sql = """
        SELECT
            m.machine_id AS MachineId,
            m.world_pinball_serial_number AS SerialNumber,
            COALESCE(COUNT(DISTINCT gs.session_id), 0)::int AS TotalGamesPlayed,
            COALESCE(MAX(se.score), 0)::bigint AS HighScore,
            COALESCE(AVG(se.score), 0)::bigint AS AverageScore,
            COALESCE(COUNT(DISTINCT se.user_id), 0)::int AS TotalUniquePlayers,
            MAX(gs.started_at_utc) AS LastPlayedAtUtc
        FROM public.machines m
        LEFT JOIN public.game_sessions gs ON gs.machine_id = m.machine_id
        LEFT JOIN public.score_entries se ON se.machine_id = m.machine_id
        WHERE m.machine_id = @MachineId
        GROUP BY m.machine_id, m.world_pinball_serial_number
        """;

    public async Task<MachineDashboardDto?> GetMachineStatsAsync(Guid machineId, CancellationToken ct = default)
    {
        var cs = config.GetRequiredPostgresConnectionString();
        await using var connection = new NpgsqlConnection(cs);

        var command = new CommandDefinition(Sql, new { MachineId = machineId }, cancellationToken: ct);
        return await connection.QuerySingleOrDefaultAsync<MachineDashboardDto>(command);
    }
}