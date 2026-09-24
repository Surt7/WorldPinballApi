using FluentAssertions;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Domain.Entities;

public class DeviceTests
{
    private static readonly DateTime NowUtc = new(2026, 9, 24, 12, 0, 0, DateTimeKind.Utc);
    private const string ValidSerial = "DEV-PHYS-001";
    private const string ValidPublicKey = "-----BEGIN PUBLIC KEY-----\nMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE\n-----END PUBLIC KEY-----";
    private const string ValidAlgorithm = "ES256";

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WhenSerialPhysicalIsEmptyOrNull_ShouldThrowArgumentException(string? invalidSerial)
    {
        // Act
        Action act = () => new Device(invalidSerial!, NowUtc);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("serialPhysical");
    }

    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeInPendingStatus()
    {
        // Act
        var device = new Device(ValidSerial, NowUtc, clientVersion: "1.0.0", notes: "Initial device");

        // Assert
        device.DeviceId.Should().NotBeEmpty();
        device.SerialPhysical.Should().Be(ValidSerial);
        device.Status.Should().Be(DeviceStatus.Pending);
        device.CreatedAtUtc.Should().Be(NowUtc);
        device.ClientVersion.Should().Be("1.0.0");
        device.Notes.Should().Be("Initial device");
        device.LastCounter.Should().Be(0);
        device.ActivatedAtUtc.Should().BeNull();
        device.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Activate_WhenDeviceIsPending_ShouldTransitionToActive()
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc);
        var activatedAt = NowUtc.AddMinutes(5);

        // Act
        device.Activate(ValidPublicKey, ValidAlgorithm, activatedAt);

        // Assert
        device.Status.Should().Be(DeviceStatus.Active);
        device.PublicKey.Should().Be(ValidPublicKey);
        device.KeyAlgorithm.Should().Be(ValidAlgorithm);
        device.ActivatedAtUtc.Should().Be(activatedAt);
        device.UpdatedAtUtc.Should().Be(activatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Activate_WhenPublicKeyIsEmptyOrNull_ShouldThrowArgumentException(string? invalidKey)
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc);

        // Act
        Action act = () => device.Activate(invalidKey!, ValidAlgorithm, NowUtc);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("publicKey");
    }

    [Fact]
    public void Activate_WhenDeviceIsRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc);
        device.Revoke(NowUtc.AddMinutes(1));

        // Act
        Action act = () => device.Activate(ValidPublicKey, ValidAlgorithm, NowUtc.AddMinutes(2));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public void Revoke_WhenDeviceIsActive_ShouldTransitionToRevokedWithNotes()
    {
        // Arrange
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm);
        var revokedAt = NowUtc.AddHours(2);

        // Act
        device.Revoke(revokedAt, "Tamper sensor triggered");

        // Assert
        device.Status.Should().Be(DeviceStatus.Revoked);
        device.RevokedAtUtc.Should().Be(revokedAt);
        device.UpdatedAtUtc.Should().Be(revokedAt);
        device.Notes.Should().Contain("Tamper sensor triggered");
    }

    [Fact]
    public void Revoke_WhenDeviceIsAlreadyRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc);
        device.Revoke(NowUtc.AddMinutes(1));

        // Act
        Action act = () => device.Revoke(NowUtc.AddMinutes(2));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public void VerifyAndIncrementCounter_WhenCounterIsStrictlyIncreasing_ShouldUpdateLastCounter()
    {
        // Arrange
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm);

        // Act
        device.VerifyAndIncrementCounter(1);
        device.VerifyAndIncrementCounter(5);
        device.VerifyAndIncrementCounter(42);

        // Assert
        device.LastCounter.Should().Be(42);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(5)]
    public void VerifyAndIncrementCounter_WhenCounterIsEqualOrLowerThanLast_ShouldThrowInvalidOperationException(long invalidCounter)
    {
        // Arrange
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm);
        device.VerifyAndIncrementCounter(10);

        // Act
        Action act = () => device.VerifyAndIncrementCounter(invalidCounter);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*strictly greater*");
    }

    [Fact]
    public void VerifyAndIncrementCounter_WhenDeviceIsNotActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc); // Status is Pending

        // Act
        Action act = () => device.VerifyAndIncrementCounter(1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Active*");
    }

    [Fact]
    public void VerifyAndIncrementCounter_WhenDeviceIsRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm);
        device.Revoke(NowUtc.AddMinutes(1));

        // Act
        Action act = () => device.VerifyAndIncrementCounter(1);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public void RecordHeartbeat_WhenDeviceIsActive_ShouldUpdateLastSeenAndVersion()
    {
        // Arrange
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm);
        var heartbeatTime = NowUtc.AddMinutes(30);

        // Act
        device.RecordHeartbeat(heartbeatTime, "2.0.0-rc1");

        // Assert
        device.LastSeenAtUtc.Should().Be(heartbeatTime);
        device.ClientVersion.Should().Be("2.0.0-rc1");
        device.UpdatedAtUtc.Should().Be(heartbeatTime);
    }

    [Fact]
    public void RecordHeartbeat_WhenDeviceIsRevoked_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var device = new Device(ValidSerial, NowUtc);
        device.Revoke(NowUtc.AddMinutes(1));

        // Act
        Action act = () => device.RecordHeartbeat(NowUtc.AddMinutes(2));

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public void Enroll_WithValidArguments_ShouldReturnEnrolledActiveDevice()
    {
        // Act
        var customId = Guid.NewGuid();
        var device = Device.Enroll(ValidSerial, ValidPublicKey, NowUtc, ValidAlgorithm, customId);

        // Assert
        device.DeviceId.Should().Be(customId);
        device.SerialPhysical.Should().Be(ValidSerial);
        device.Status.Should().Be(DeviceStatus.Active);
        device.PublicKey.Should().Be(ValidPublicKey);
        device.KeyAlgorithm.Should().Be(ValidAlgorithm);
        device.ActivatedAtUtc.Should().Be(NowUtc);
    }
}
