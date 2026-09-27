using System.Security.Cryptography;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Api.Controllers.Public;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer;
using WorldPinball.Application.Features.GameSessions.Public.AddPlayer.Commands;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession;
using WorldPinball.Application.Features.GameSessions.Public.CreateGameSession.Commands;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Dtos;
using WorldPinball.Application.Features.GameSessions.Public.GetLobby.Queries;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer;
using WorldPinball.Application.Features.GameSessions.Public.KickPlayer.Commands;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession;
using WorldPinball.Application.Features.GameSessions.Public.LaunchSession.Commands;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore;
using WorldPinball.Application.Features.GameSessions.Public.SubmitScore.Commands;
using WorldPinball.Application.Security.Cryptography;

namespace WorldPinball.Tests.Controller;

public class SessionsControllerTests
{
    private readonly IMachineAuthService _authService = Substitute.For<IMachineAuthService>();
    private readonly ICreateGameSessionHandler _createSessionHandler = Substitute.For<ICreateGameSessionHandler>();
    private readonly ISubmitScoreHandler _submitScoreHandler = Substitute.For<ISubmitScoreHandler>();
    private readonly IGetLobbyHandler _getLobbyHandler = Substitute.For<IGetLobbyHandler>();
    private readonly IAddPlayerHandler _addPlayerHandler = Substitute.For<IAddPlayerHandler>();
    private readonly IKickPlayerHandler _kickPlayerHandler = Substitute.For<IKickPlayerHandler>();
    private readonly ILaunchSessionHandler _launchSessionHandler = Substitute.For<ILaunchSessionHandler>();
    private readonly IValidator<StartSessionRequest> _validatorStartSession = Substitute.For<IValidator<StartSessionRequest>>();
    private readonly IValidator<SubmitScoreRequest> _validatorSubmitScore = Substitute.For<IValidator<SubmitScoreRequest>>();
    private readonly IValidator<AddPlayerRequest> _validatorAddPlayer = Substitute.For<IValidator<AddPlayerRequest>>();

    private SessionsController CreateController()
    {
        _validatorStartSession.ValidateAsync(Arg.Any<StartSessionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _validatorSubmitScore.ValidateAsync(Arg.Any<SubmitScoreRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        _validatorAddPlayer.ValidateAsync(Arg.Any<AddPlayerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var controller = new SessionsController(
            _authService,
            _createSessionHandler,
            _submitScoreHandler,
            _getLobbyHandler,
            _addPlayerHandler,
            _kickPlayerHandler,
            _launchSessionHandler,
            _validatorStartSession,
            _validatorSubmitScore,
            _validatorAddPlayer);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    [Fact]
    public async Task CreateGameSession_WhenRequestIsValidAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var sessionId = Guid.NewGuid();
        string fakeSignature = RandomNumberGenerator.GetHexString(16);
        var now = DateTimeOffset.UtcNow;

        _authService.ValidateAsync(Arg.Any<MachineAuthContext>(), Arg.Any<CancellationToken>())
            .Returns(new MachineAuthResult.Success(machineId, deviceId, nonce, timestamp));

        _createSessionHandler.HandleAsync(Arg.Any<CreateGameSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(new CreateGameSessionResult.Success(sessionId, nonce, now));

        var controller = CreateController();
        controller.HttpContext.Request.Method = "POST";
        controller.HttpContext.Request.Path = "/api/v1/sessions/create";

        var request = new StartSessionRequest("STORY", "1.02.12");

        // Act
        var result = await controller.CreateGameSession(request, deviceId.ToString(), timestamp.ToString(), nonce, fakeSignature, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeAssignableTo<CreateGameSessionResponse>().Which;
        response.SessionId.Should().Be(sessionId);
        response.ServerTime.Should().Be(now.ToUnixTimeSeconds());
    }

    [Fact]
    public async Task SubmitScore_WhenRequestIsValid_ShouldReturnOk()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var scoreGuid = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        _submitScoreHandler.HandleAsync(Arg.Any<SubmitScoreCommand>(), Arg.Any<CancellationToken>())
            .Returns(new SubmitScoreResult.Success([scoreGuid]));

        var controller = CreateController();
        var request = new SubmitScoreRequest(deviceId, [new SlotScoreRequest(1, 5000)]);

        // Act
        var result = await controller.SubmitScore(sessionId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<SubmitScoreResponse>().Which;
        response.ScoreEntryIds.Should().ContainSingle().Which.Should().Be(scoreGuid);
    }

    [Fact]
    public async Task GetLobby_WhenSessionExists_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var lobbyDto = new SessionLobbyDto(sessionId, "Created", []);
        _getLobbyHandler.HandleAsync(Arg.Is<GetLobbyQuery>(q => q.SessionId == sessionId), Arg.Any<CancellationToken>())
            .Returns(new GetLobbyResult.Success(lobbyDto));

        var controller = CreateController();

        // Act
        var result = await controller.GetLobby(sessionId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<SessionLobbyDto>().Which;
        response.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public async Task AddPlayer_WhenRequestIsValid_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _addPlayerHandler.HandleAsync(Arg.Is<AddPlayerCommand>(c => c.SessionId == sessionId && c.SlotNumber == 1), Arg.Any<CancellationToken>())
            .Returns(new AddPlayerResult.Success(sessionId, 1));

        var controller = CreateController();
        var request = new AddPlayerRequest(1, null, "AAA");

        // Act
        var result = await controller.AddPlayer(sessionId, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<AddPlayerResponse>().Which;
        response.SessionId.Should().Be(sessionId);
        response.SlotNumber.Should().Be(1);
    }

    [Fact]
    public async Task KickPlayer_WhenPlayerExists_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _kickPlayerHandler.HandleAsync(Arg.Is<KickPlayerCommand>(c => c.SessionId == sessionId && c.SlotNumber == 2), Arg.Any<CancellationToken>())
            .Returns(new KickPlayerResult.Success(sessionId, 2));

        var controller = CreateController();

        // Act
        var result = await controller.KickPlayer(sessionId, 2, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<KickPlayerResponse>().Which;
        response.SlotNumber.Should().Be(2);
    }

    [Fact]
    public async Task LaunchSession_WhenSessionReady_ShouldReturnOk()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _launchSessionHandler.HandleAsync(Arg.Is<LaunchSessionCommand>(c => c.SessionId == sessionId), Arg.Any<CancellationToken>())
            .Returns(new LaunchSessionResult.Success(sessionId));

        var controller = CreateController();

        // Act
        var result = await controller.LaunchSession(sessionId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var response = okResult.Value.Should().BeOfType<LaunchSessionResponse>().Which;
        response.SessionId.Should().Be(sessionId);
    }
}