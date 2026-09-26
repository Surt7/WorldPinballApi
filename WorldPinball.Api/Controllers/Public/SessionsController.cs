using Microsoft.AspNetCore.Mvc;

using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Security.Cryptography;



namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsController(
    IMachineAuthService authService,
    ICreateGameSessionHandler createGameSessionHandler,
    ISubmitScoreHandler submitScoreHandler,
    IValidator<StartSessionRequest> requestValidator,
    IValidator<SubmitScoreRequest> reqValidator
) : ControllerBase
{
    
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    [HttpPost("create")]
    [HttpPost("start")]
    [EndpointSummary("Create a game session")]
    [EndpointDescription("POST to create new game session")]
    [ProducesResponseType(typeof(CreateGameSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateGameSessionResponse>> CreateGameSession(
        [FromBody] StartSessionRequest request,
        [FromHeader(Name = "X-Device-Id")] string? deviceId,
        [FromHeader(Name = "X-Timestamp")] string? timestamp,
        [FromHeader(Name = "X-Nonce")] string? nonce,
        [FromHeader(Name = "X-Signature")] string? signature,
        CancellationToken ct)
    {
        var validationResult = await requestValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var context = new MachineAuthContext(
            DeviceIdHeader: deviceId,
            TimestampHeader: timestamp,
            NonceHeader: nonce,
            SignatureHeader: signature,
            HttpMethod: Request.Method,
            RequestPath: Request.Path.Value ?? string.Empty,
            RawBody: JsonSerializer.Serialize(request,JsonOpts));

        var authResult = await authService.ValidateAsync(context, ct);
        if (authResult is not MachineAuthResult.Success success)
        {
            return Unauthorized((new { message = "Machine authentification failed." }));
        }
        
        var command = new CreateGameSessionCommand(
            success.DeviceId,
            request.ModeCode,
            request.ClientVersion
        );
        
        var sessionResult = await createGameSessionHandler.HandleAsync(command, ct);
        
        return sessionResult switch
        {
            CreateGameSessionResult.Success s => Ok(new CreateGameSessionResponse(
                s.SessionId,
                s.CreatedAtUtc.ToUnixTimeSeconds()
            )),
            CreateGameSessionResult.ModeNotFound m => NotFound(new
            {
                message = $"Mode '{m.ModeCode}' not found or inactive."
            }),
            CreateGameSessionResult.DeviceNotFound d => NotFound(new
            {
                message = $"Device '{d.DeviceId}' not found."
            }),
            CreateGameSessionResult.DeviceNotActive d => BadRequest(new
            {
                message = $"Device '{d.DeviceId}' is not active."
            }),
            CreateGameSessionResult.MachineNotAttached d => BadRequest(new
            {
                message = $"Device '{d.DeviceId}' is not attached to any machine."
            }),
            _ => throw new InvalidOperationException($"Unsupported session result: {sessionResult.GetType().Name}")
        };
    }
    [HttpPost("{sessionId:guid}/submit-score")]
    [Authorize(Policy = "ActiveMachineOnly")]
    [EndpointSummary("Submit Score Request")]
    [EndpointDescription("POST to create new score from a game session that just ended")]
    [ProducesResponseType(typeof(SubmitScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitScoreResponse>> SubmitScore(
        [FromRoute] Guid sessionId,
        [FromBody]  SubmitScoreRequest request,
        CancellationToken ct)
    {
        var validationResult = await reqValidator.ValidateAsync(request,ct);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(
                    failure.PropertyName,
                    failure.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var command = new SubmitScoreCommand(
            sessionId,
            request.DeviceId,
            request.Scores.Select(s => new SlotScoreDto(s.SlotNumber, s.Score)).ToList());
        
        var scoreResult = await submitScoreHandler.HandleAsync(command, ct);


        return scoreResult switch
        {
            SubmitScoreResult.Success s => Ok(new SubmitScoreResponse(
                s.ScoreEntryIds)),
            SubmitScoreResult.SessionNotFound s => NotFound(new
            {
                message = $"Session '{s.GameSessionId}' not found."
            }),
            SubmitScoreResult.SessionAlreadyEnded s => Conflict(new
            {
                message = $"Session '{s.GameSessionId}' already ended."
            }),
            SubmitScoreResult.DeviceMisMatch s => Conflict(new
            {
                message = $"Device '{s.ActualDeviceId}' mismatch (expected '{s.ExpectedDeviceId}')."
            }),
            SubmitScoreResult.SlotNotFound s => Conflict(new
            {
                message = $"Slot number: '{s.SlotNumber}' not found."
            }),
            SubmitScoreResult.EmptyScores => BadRequest(new
            {
                message = "Scores list cannot be empty."
            }),
            _ => throw new InvalidOperationException($"Unsupported session result: {scoreResult.GetType().Name}")
        };
    }
}