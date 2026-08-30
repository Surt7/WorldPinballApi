using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Persistence;

namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsController(AppDbContext db, IMachineAuthService auth) : ControllerBase
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };


    [HttpPost("start")]
    public async Task<ActionResult<StartSessionResponse>> Start(CancellationToken ct)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(rawBody))
            return BadRequest(new { message = "Body vide" });

        StartSessionRequest? req;
        try
        {
            req = System.Text.Json.JsonSerializer.Deserialize<StartSessionRequest>(rawBody, JsonOpts);
        }
        catch
        {
            return BadRequest(new { message = "JSON invalide" });
        }

        if (req is null)
            return BadRequest(new { message = "Body invalide" });

        MachineAuthResult auth1;
        try
        {
            auth1 = await auth.ValidateAsync(Request, rawBody, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        var mode = await db.GameModes
            .Where(x => x.Code == req.ModeCode && x.IsActive)
            .Select(x => new { x.ModeId })
            .FirstOrDefaultAsync(ct);

        if (mode is null)
            return NotFound(new { message = "modeCode inconnu" });

        var session = new GameSession
        {
            SessionId = Guid.NewGuid(),
            MachineId = auth1.MachineId,
            ModeId = mode.ModeId,
            StartedAtUtc = DateTime.UtcNow,
            State = GameSessionState.Started,
            Nonce = auth1.Nonce,
            ClientVersion = req.ClientVersion
        };

        db.GameSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return Ok(new StartSessionResponse(session.SessionId, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
    }

    [HttpPost("{sessionId:guid}/submit-score")]
    public async Task<ActionResult<SubmitScoreResponse>> SubmitScore(Guid sessionId, CancellationToken ct)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;

        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(rawBody))
            return BadRequest(new { message = "Body vide" });

        SubmitScoreRequest? req;
        try
        {
            req = System.Text.Json.JsonSerializer.Deserialize<SubmitScoreRequest>(rawBody, JsonOpts);
            Console.WriteLine($"ModeCode reçu = '{req}'");
        }
        catch
        {
            return BadRequest(new { message = "JSON invalide" });
        }

        if (req is null)
            return BadRequest(new { message = "Body invalide" });

        MachineAuthResult auth1;
        try
        {
            auth1 = await auth.ValidateAsync(Request, rawBody, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }

        if (req.Score < 0 || req.Score > 2_000_000_000)
            return BadRequest(new { message = "score hors bornes" });

        var session = await db.GameSessions
            .Include(s => s.Machine)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, ct);

        if (session is null) return NotFound(new { message = "session introuvable" });
        if (session.MachineId != auth1.MachineId) return Forbid();
        if (session.State != GameSessionState.Started) return Conflict(new { message = "session déjà clôturée" });

        using var tx = await db.Database.BeginTransactionAsync(ct);

        session.EndedAtUtc = DateTime.UtcNow;
        session.State = GameSessionState.Ended;

        var scoreEntry = new ScoreEntry
        {
            ScoreEntryId = Guid.NewGuid(),
            SessionId = session.SessionId,
            MachineId = session.MachineId,
            IpId = session.Machine.IpId,
            ModeId = session.ModeId,
            Score = req.Score,
            AchievedAtUtc = DateTime.UtcNow,
            IsValidated = true
        };

        db.ScoreEntries.Add(scoreEntry);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new SubmitScoreResponse(scoreEntry.ScoreEntryId, scoreEntry.IsValidated));
    }
}
