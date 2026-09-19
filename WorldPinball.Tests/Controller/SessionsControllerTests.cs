using System.Security.Cryptography;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Application.Features.GameSessions.Public.Start;
using WorldPinball.Application.Features.GameSessions.Public.Start.Commands;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Api.Controllers.Public;

namespace WorldPinball.Tests.Controller;

public class SessionsControllerTests
{
    [Fact]
    public async Task StartSession_WhenRequestIsValidAuthenticated_ShouldReturnOk()
    {
        
        var machineId = new Guid();
        var deviceId = new Guid();
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sessionId = new Guid();
string fakeSignature = RandomNumberGenerator.GetHexString(16);

var now = DateTimeOffset.UtcNow;
var expectedServerTime = now.ToUnixTimeSeconds();

        var validator = Substitute.For<IValidator<StartSessionRequest>>();
        validator.ValidateAsync(
                Arg.Any<StartSessionRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var authService = Substitute.For<IMachineAuthService>();
        authService.ValidateAsync(Arg.Any<MachineAuthContext>(), Arg.Any<CancellationToken>())
            .Returns(new MachineAuthResult.Success(machineId, deviceId, nonce, timestamp));


        var handler = Substitute.For<IStartGameSessionHandler>();
        handler.HandleAsync(Arg.Any<StartGameSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new StartGameSessionResult.Success(sessionId, nonce, now));


        var request = new StartSessionRequest(
            "STORY",
            "1.02.12"
        );

        var controller = new SessionsController(
            authService,
            handler,
            validator);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Method = "POST";
        controller.HttpContext.Request.Path = "/api/v1/sessions/start";


        var result = await controller
            .StartSession(
                request,
                deviceId.ToString(),
                timestamp.ToString(),
                nonce,fakeSignature,
                CancellationToken.None
                );
        
        var okResult = result
            .Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which;

        var response = okResult
            .Value
            .Should()
            .BeOfType<StartSessionResponse>()
            .Which;

        response.SessionId.Should().Be(sessionId);
        response.ServerTime.Should().Be(expectedServerTime);
        
        
    }
}