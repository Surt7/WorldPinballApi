using Microsoft.AspNetCore.Mvc;
using System.Text;
using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Application.Features.GameSessions.Public.Start;
using WorldPinball.Application.Features.GameSessions.Public.Start.Commands;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;


namespace WorldPinball.Api.Controllers.Public;

[ApiController]
[Route("api/v1/sessions")]
public class SessionsController(
    IMachineAuthService authService,
    IStartGameSessionHandler startSessionHandler,
    IValidator<StartSessionRequest> requestValidator
) : ControllerBase
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };


    [HttpPost("start")]
    [EndpointSummary("Start a game session")]
    [EndpointDescription("POST to create new game session")]
    [ProducesResponseType(typeof(StartSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StartSessionResponse>> StartSession(
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
            RawBody: System.Text.Json.JsonSerializer.Serialize(request));

        var authResult = await authService.ValidateAsync(context, ct);
        if (authResult is not MachineAuthResult.Success success)
        {
            return Unauthorized((new { message = "Machine authentification failed." }));
        }
        
        var command = new StartGameSessionCommand(
            success.DeviceId,
            request.ModeCode,
            request.ClientVersion
        );
        
        var sessionResult = await startSessionHandler.HandleAsync(command, ct);
        
        return sessionResult switch
        {
            StartGameSessionResult.Success s => Ok(new StartSessionResponse(
                s.SessionId,
                s.StartedAtUtc.ToUnixTimeSeconds()
            )),
            StartGameSessionResult.ModeNotFound m => NotFound(new
            {
                message = $"Mode '{m.ModeCode}' not found or inactive."
            }),
            StartGameSessionResult.DeviceNotFound d => NotFound(new
            {
                message = $"Device '{d.DeviceId}' not found."
            }),
            StartGameSessionResult.DeviceNotActive d => BadRequest(new
            {
                message = $"Device '{d.DeviceId}' is not active."
            }),
            StartGameSessionResult.MachineNotAttached d => BadRequest(new
            {
                message = $"Device '{d.DeviceId}' is not attached to any machine."
            }),
            _ => throw new InvalidOperationException($"Unsupported session result: {sessionResult.GetType().Name}")
        };
    }
    // [HttpPost("{sessionId:guid}/submit-score")]
    // public async Task<ActionResult<SubmitScoreResponse>> SubmitScore(Guid sessionId, CancellationToken ct)
    // {
    //
    // }
}