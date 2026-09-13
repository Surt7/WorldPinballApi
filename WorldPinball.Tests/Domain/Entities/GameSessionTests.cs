using FluentAssertions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Domain.Entities;

public class GameSessionTests
{
    private static GameSession CreateValidSession(DateTime? startedAt = null)
    {
        return new GameSession(
            machineId: Guid.NewGuid(),
            deviceId: Guid.NewGuid(),
            modeId: Guid.NewGuid(),
            userId: "player-42",
            clientVersion: "1.0.0",
            nonce: "nonce-random-xyz",
            startedAtUtc: startedAt ?? DateTime.UtcNow
        );
    }

    [Fact]
    public void Constructor_WhenArgumentsAreValid_ShouldInitializeSessionInStartedState()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var userId = "player-42";
        var clientVersion = "1.0.0";
        var nonce = "nonce-random-xyz";
        var startedAtUtc = DateTime.UtcNow;

        // Act
        var session = new GameSession(
            machineId,
            deviceId,
            modeId,
            userId,
            clientVersion,
            nonce,
            startedAtUtc
        );

        // Assert
        session.SessionId.Should().NotBeEmpty();
        session.MachineId.Should().Be(machineId);
        session.DeviceId.Should().Be(deviceId);
        session.ModeId.Should().Be(modeId);
        session.UserId.Should().Be(userId);
        session.ClientVersion.Should().Be(clientVersion);
        session.Nonce.Should().Be(nonce);
        session.StartedAtUtc.Should().Be(startedAtUtc);
        session.EndedAtUtc.Should().BeNull();
        session.State.Should().Be(GameSessionState.Started);
        session.ScoreEntry.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenMachineIdIsEmpty_ThrowsArgumentException()
    {
        // Act
        Action act = () => new GameSession(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "player-42",
            "1.0.0",
            "nonce-xyz",
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenDeviceIdIsEmpty_ThrowsArgumentException()
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            "player-42",
            "1.0.0",
            "nonce-xyz",
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenModeIdIsEmpty_ThrowsArgumentException()
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            "player-42",
            "1.0.0",
            "nonce-xyz",
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenUserIdIsInvalid_ThrowsArgumentException(string? invalidUserId)
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidUserId!,
            "1.0.0",
            "nonce-xyz",
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenClientVersionIsInvalid_ThrowsArgumentException(string? invalidClientVersion)
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "player-42",
            invalidClientVersion!,
            "nonce-xyz",
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WhenNonceIsInvalid_ThrowsArgumentException(string? invalidNonce)
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "player-42",
            "1.0.0",
            invalidNonce!,
            DateTime.UtcNow
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhenStartedAtUtcIsDefault_ThrowsArgumentException()
    {
        // Act
        Action act = () => new GameSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "player-42",
            "1.0.0",
            "nonce-xyz",
            default
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void End_WhenSessionIsStarted_ShouldSetEndedAtAndTransitionToEndedState()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        var endedAt = startedAt.AddMinutes(10);

        // Act
        session.End(endedAt);

        // Assert
        session.EndedAtUtc.Should().Be(endedAt);
        session.State.Should().Be(GameSessionState.Ended);
    }

    [Fact]
    public void End_WhenSessionIsResumed_ShouldTransitionToEndedState()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        session.Pause();
        session.Resume();
        var endedAt = startedAt.AddMinutes(15);

        // Act
        session.End(endedAt);

        // Assert
        session.EndedAtUtc.Should().Be(endedAt);
        session.State.Should().Be(GameSessionState.Ended);
    }

    [Fact]
    public void End_WhenSessionIsPaused_ShouldTransitionToEndedState()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        session.Pause();
        var endedAt = startedAt.AddMinutes(10);

        // Act
        session.End(endedAt);

        // Assert
        session.EndedAtUtc.Should().Be(endedAt);
        session.State.Should().Be(GameSessionState.Ended);
    }

    [Fact]
    public void End_WhenEndedAtEqualsStartedAt_ShouldSucceed()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);

        // Act
        session.End(startedAt);

        // Assert
        session.EndedAtUtc.Should().Be(startedAt);
        session.State.Should().Be(GameSessionState.Ended);
    }

    [Fact]
    public void End_WhenSessionIsNotInActiveState_ThrowsInvalidOperationException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        session.End(startedAt.AddMinutes(5));

        // Act: Attempt to close an already ended session
        Action act = () => session.End(startedAt.AddMinutes(10));

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void End_WhenEndedAtIsEarlierThanStartedAt_ThrowsArgumentException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);

        // Act: End date earlier than start date
        Action act = () => session.End(startedAt.AddMinutes(-5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Pause_WhenSessionIsStarted_ShouldTransitionToPausedState()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        session.Pause();

        // Assert
        session.State.Should().Be(GameSessionState.Paused);
    }

    [Fact]
    public void Pause_WhenSessionIsResumed_ShouldTransitionToPausedState()
    {
        // Arrange
        var session = CreateValidSession();
        session.Pause();
        session.Resume();

        // Act
        session.Pause();

        // Assert
        session.State.Should().Be(GameSessionState.Paused);
    }

    [Fact]
    public void Pause_WhenSessionIsNotActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var session = CreateValidSession();
        session.Pause(); // Already paused

        // Act: Attempt to pause again
        Action act = () => session.Pause();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Resume_WhenSessionIsPaused_ShouldTransitionToResumedState()
    {
        // Arrange
        var session = CreateValidSession();
        session.Pause();

        // Act
        session.Resume();

        // Assert
        session.State.Should().Be(GameSessionState.Resumed);
    }

    [Fact]
    public void Resume_WhenSessionIsNotPaused_ThrowsInvalidOperationException()
    {
        // Arrange
        var session = CreateValidSession(); // Started state

        // Act: Attempt to resume without pausing first
        Action act = () => session.Resume();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Abandon_WhenSessionIsActive_ShouldTransitionToAbandonedState()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        var abandonedAt = startedAt.AddMinutes(3);

        // Act
        session.Abandon(abandonedAt);

        // Assert
        session.EndedAtUtc.Should().Be(abandonedAt);
        session.State.Should().Be(GameSessionState.Abandoned);
    }

    [Fact]
    public void Abandon_WhenSessionIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        session.Abandon(startedAt.AddMinutes(5));

        // Act: Attempt to abandon an already closed session
        Action act = () => session.Abandon(startedAt.AddMinutes(10));

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Abandon_WhenAbandonedAtIsEarlierThanStartedAt_ThrowsArgumentException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);

        // Act: Abandon date earlier than start date
        Action act = () => session.Abandon(startedAt.AddMinutes(-5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Abort_WhenSessionIsActive_ShouldTransitionToAbortedState()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        var abortedAt = startedAt.AddMinutes(2);

        // Act
        session.Abort(abortedAt);

        // Assert
        session.EndedAtUtc.Should().Be(abortedAt);
        session.State.Should().Be(GameSessionState.Aborted);
    }

    [Fact]
    public void Abort_WhenSessionIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);
        session.Abort(startedAt.AddMinutes(5));

        // Act: Attempt to abort an already closed session
        Action act = () => session.Abort(startedAt.AddMinutes(10));

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Abort_WhenAbortedAtIsEarlierThanStartedAt_ThrowsArgumentException()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;
        var session = CreateValidSession(startedAt);

        // Act: Abort date earlier than start date
        Action act = () => session.Abort(startedAt.AddMinutes(-5));

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
