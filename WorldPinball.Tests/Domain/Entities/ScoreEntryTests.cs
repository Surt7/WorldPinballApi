using FluentAssertions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Events;

namespace WorldPinball.Tests.Domain.Entities;

public class ScoreEntryTests
{
    [Fact]
    public void Constructor_WhenArgumentsAreValid_ShouldInitializeScoreEntry()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var slotNumber = 1;
        var deviceId = Guid.NewGuid();
        var machineId = Guid.NewGuid();
        var ipId = Guid.NewGuid();
        var modeId = Guid.NewGuid();
        var score = 12_500_000;
        var achievedAtUtc = DateTime.UtcNow;
        var userId = "user-123";

        // Act
        var entry = new ScoreEntry(
            sessionId,
            slotNumber,
            deviceId,
            machineId,
            ipId,
            modeId,
            score,
            achievedAtUtc,
            userId);

        // Assert
        entry.ScoreEntryId.Should().NotBeEmpty();
        entry.SessionId.Should().Be(sessionId);
        entry.SlotNumber.Should().Be(slotNumber);
        entry.DeviceId.Should().Be(deviceId);
        entry.MachineId.Should().Be(machineId);
        entry.IpId.Should().Be(ipId);
        entry.ModeId.Should().Be(modeId);
        entry.Score.Should().Be(score);
        entry.AchievedAtUtc.Should().Be(achievedAtUtc);
        entry.UserId.Should().Be(userId);
        entry.IsValidated.Should().BeTrue();
        entry.ValidationReason.Should().BeNull();
        entry.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ScoreSubmittedDomainEvent>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-1)]
    public void Constructor_WhenSlotNumberOutOfRange_ThrowsArgumentOutOfRangeException(int invalidSlot)
    {
        // Act
        Action act = () => new ScoreEntry(
            Guid.NewGuid(),
            invalidSlot,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100_000,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_WhenScoreIsNegative_ThrowsArgumentOutOfRangeException()
    {
        // Act
        Action act = () => new ScoreEntry(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            -1,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Invalidate_WhenReasonProvided_ShouldMarkInvalidated()
    {
        // Arrange
        var entry = new ScoreEntry(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100_000,
            DateTime.UtcNow);

        // Act
        entry.Invalidate("Tilt triggered abnormally.");

        // Assert
        entry.IsValidated.Should().BeFalse();
        entry.ValidationReason.Should().Be("Tilt triggered abnormally.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Invalidate_WhenReasonIsInvalid_ThrowsArgumentException(string? invalidReason)
    {
        // Arrange
        var entry = new ScoreEntry(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            100_000,
            DateTime.UtcNow);

        // Act
        Action act = () => entry.Invalidate(invalidReason!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
