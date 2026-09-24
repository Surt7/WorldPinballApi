using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;
using WorldPinball.Application.Interfaces.Repositories;

namespace WorldPinball.Infrastructure.Persistence.Repositories;

public sealed class MachineDashboardReadRepository : IMachineDashboardReadRepository
{
    private const string Sql = """
                               SELECT
                               m.machine_id AS machineId,
                               m.world_pinball_serial_number AS SerialNumber,
                               COALESCE(COUNT(DISTINCT gs.session_id), 0) :: int AS TotalGamesPlayed,
                               COALESCE(AVG(se.score), 0)::bigint AS AverageScore,
                                        COALESCE(COUNT(DISTINCT se.user_id), 0)::int AS TotalUniquePlayers,
                               MAX(gs.started_at_utc) AS LastPlayedAtUtc
                               FROM public.machines m
                               LEFT JOIN public.game_sessions gs ON gs.machine_id = m.machine_id
                               LEFT JOIN public.score_entries se ON se.machine_id = m.machine_id
                               WHERE m.machine_id = @MachineId
                               GROUP BY m.machine_id, m.world_pinball_serial_number
                               """;

    public Task<MachineDashboardDto?> GetMachineStatsAsync(Guid machineId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}