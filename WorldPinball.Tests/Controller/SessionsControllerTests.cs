using System.Security.Cryptography;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Api.Controllers.Public;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;

namespace WorldPinball.Tests.Controller;

public class SessionsControllerTests
{
    [Fact]
    public async Task CreateGameSession_WhenRequestIsValidAuthenticated_ShouldReturnOk()
    {
        //Arrange
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sessionId = Guid.NewGuid();
        string fakeSignature = RandomNumberGenerator.GetHexString(16);

        var now = DateTimeOffset.UtcNow;
        var expectedServerTime = now.ToUnixTimeSeconds();
        var validatorStartSession = Substitute.For<IValidator<StartSessionRequest>>();
        var validatorSubmitScore = Substitute.For<IValidator<SubmitScoreRequest>>();

        //validator
        validatorStartSession.ValidateAsync(
                Arg.Any<StartSessionRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        validatorSubmitScore.ValidateAsync(
                Arg.Any<SubmitScoreRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());


        var authService = Substitute.For<IMachineAuthService>();
        authService.ValidateAsync(Arg.Any<MachineAuthContext>(), Arg.Any<CancellationToken>())
            .Returns(new MachineAuthResult.Success(machineId, deviceId, nonce, timestamp));

        var submitHandler = Substitute.For<ISubmitScoreHandler>();
        var handler = Substitute.For<ICreateGameSessionHandler>();
        handler.HandleAsync(Arg.Any<CreateGameSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new CreateGameSessionResult.Success(sessionId, nonce, now));


        var request = new StartSessionRequest(
            "STORY",
            "1.02.12"
        );

        var controller = new SessionsController(
            authService,
            handler,
            submitHandler,
            validatorStartSession,
            validatorSubmitScore);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Method = "POST";
        controller.HttpContext.Request.Path = "/api/v1/sessions/create";

        // Act
        var result = await controller
            .CreateGameSession(
                request,
                deviceId.ToString(),
                timestamp.ToString(),
                nonce, fakeSignature,
                CancellationToken.None
            );
        //Assert
        var okResult = result
            .Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which;

        var response = okResult
            .Value
            .Should()
            .BeAssignableTo<CreateGameSessionResponse>()
            .Which;

        response.SessionId.Should().Be(sessionId);
        response.ServerTime.Should().Be(expectedServerTime);
    }


    [Fact]
    public async Task SubmitScore_WhenRequestIsValid_ShouldReturnOk()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var scoreGuid = Guid.NewGuid();
        var score = 5_002_214;
        int slotNumber = 3;
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sessionId = Guid.NewGuid();

        //validator
        var validatorStartSession = Substitute.For<IValidator<StartSessionRequest>>();
        var validatorSubmitScore = Substitute.For<IValidator<SubmitScoreRequest>>();
        validatorStartSession.ValidateAsync(
                Arg.Any<StartSessionRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        validatorSubmitScore.ValidateAsync(
                Arg.Any<SubmitScoreRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var authService = Substitute.For<IMachineAuthService>();
        authService.ValidateAsync(Arg.Any<MachineAuthContext>(), Arg.Any<CancellationToken>())
            .Returns(new MachineAuthResult.Success(machineId, deviceId, nonce, timestamp));

        var command = new SubmitScoreCommand(sessionId, deviceId, [new SlotScoreDto(slotNumber, score)]);

        var submitScoreHandler = Substitute.For<ISubmitScoreHandler>();
        var handler = Substitute.For<ICreateGameSessionHandler>();
       
        submitScoreHandler.HandleAsync(
                Arg.Is<SubmitScoreCommand>(c => c.SessionId == sessionId && c.DeviceId == deviceId && c.Scores.Count == 1),
                Arg.Any<CancellationToken>())
            .Returns(new SubmitScoreResult.Success([scoreGuid]));

        var request = new SubmitScoreRequest(
            deviceId,
            [new SlotScoreRequest(slotNumber, score)]
        );

        var controller = new SessionsController(
            authService,
            handler,
            submitScoreHandler,
            validatorStartSession,
            validatorSubmitScore);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.HttpContext.Request.Method = "POST";
        controller.HttpContext.Request.Path = "/api/v1/{sessionId:guid}/submit-score";

        // Act
        var result = await controller
            .SubmitScore(
                sessionId,
                request,
                CancellationToken.None
            );

        //Assert
        var okResult = result
            .Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which;

        var response = okResult
            .Value
            .Should()
            .BeOfType<SubmitScoreResponse>()
            .Which;

        response.ScoreEntryIds.Should().ContainSingle().Which.Should().Be(scoreGuid);
    }
}