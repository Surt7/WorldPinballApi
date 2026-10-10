using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.StreamingAccess.Public;
using WorldPinball.Application.Features.StreamingAccess.Public.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.StreamingAccess.Public;

public class RequestStreamingAccessHandlerTests
{
    private readonly IGameSessionWriteRepository _sessionRepo = Substitute.For<IGameSessionWriteRepository>();
    private readonly IStreamingAccessRequestRepository _requestRepo = Substitute.For<IStreamingAccessRequestRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 30, 19, 0, 0, TimeSpan.Zero));
    private readonly NullLogger<RequestStreamingAccessHandler> _logger = NullLogger<RequestStreamingAccessHandler>.Instance;
    private readonly RequestStreamingAccessHandler _sut;

    public RequestStreamingAccessHandlerTests()
    {
        _sut = new RequestStreamingAccessHandler(_sessionRepo, _requestRepo, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_ShouldReturnSessionNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _sessionRepo.GetSessionWithMachineAsync(sessionId, Arg.Any<CancellationToken>()).Returns((GameSession?)null);

        // Act
        var result = await _sut.HandleAsync(new RequestStreamingAccessCommand(sessionId, Guid.NewGuid()));

        // Assert
        result.Should().BeOfType<RequestStreamingAccessResult.SessionNotFound>()
            .Which.SessionId.Should().Be(sessionId);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldCreateAndSavePendingRequest()
    {
        // Arrange: active session
        var session = new GameSession(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "1.0", "nonce", _timeProvider.GetUtcNow().UtcDateTime);
        _sessionRepo.GetSessionWithMachineAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns(session);
        _requestRepo.GetLatestRequestAsync(session.SessionId, Arg.Any<CancellationToken>()).Returns((StreamingAccessRequest?)null);

        var userId = Guid.NewGuid();
        var command = new RequestStreamingAccessCommand(session.SessionId, userId);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<RequestStreamingAccessResult.Success>();
        await _requestRepo.Received(1).AddAsync(
            Arg.Is<StreamingAccessRequest>(r => r.SessionId == session.SessionId && r.RequestedByUserId == userId && r.Status == StreamingRequestStatus.Pending),
            Arg.Any<CancellationToken>());
    }
}