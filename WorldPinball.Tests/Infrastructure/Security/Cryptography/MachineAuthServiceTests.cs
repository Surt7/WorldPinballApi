using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using NSubstitute;
using WorldPinball.Application.Security.Cryptography;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Persistence;
using WorldPinball.Infrastructure.Security;
using WorldPinball.Infrastructure.Security.Cryptography;

namespace WorldPinball.Tests.Infrastructure.Security.Cryptography;

public class MachineAuthServiceTests
{
    private readonly AppDbContext _db;
    private readonly ICanonicalBuilder _canonicalBuilderMock;
    private readonly ISignatureVerifier _signatureVerifierMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly MachineAuthService _sut;

    public MachineAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _canonicalBuilderMock = Substitute.For<ICanonicalBuilder>();
        _signatureVerifierMock = Substitute.For<ISignatureVerifier>();
        _timeProvider = new FakeTimeProvider();
        
        // Lock the fake system clock to a known point in time
        _timeProvider.SetUtcNow(new DateTimeOffset(2026, 9, 17, 12, 0, 0, TimeSpan.Zero));

        // Note: NuLogger is used to avoid cluttering test outputs
        _sut = new MachineAuthService(
            _db,
            _canonicalBuilderMock,
            _signatureVerifierMock,
            _timeProvider,
            NullLogger<MachineAuthService>.Instance);
    }

    // =========================================================================
    // PROVIDED TESTS (COMPLEX CRYPTO MOCKING AND HAPPY PATH)
    // =========================================================================

    [Fact]
    public async Task ValidateAsync_WithValidSignature_ShouldReturnSuccess()
    {
        // Assemble
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var now = _timeProvider.GetUtcNow();
        var timestamp = now.ToUnixTimeSeconds();
        var nonce = "1234567890123456";

        var device = new Device 
        { 
            DeviceId = deviceId, 
            SerialPhysical = "DEV-001",
            Status = DeviceStatus.Active, 
            PublicKey = "pk", 
            KeyAlgorithm = "algo" 
        };
        var machineDevice = new MachineDevice(machineId, deviceId, now.UtcDateTime);

        _db.Devices.Add(device);
        _db.MachineDevices.Add(machineDevice);
        await _db.SaveChangesAsync();

        var context = new MachineAuthContext(
            deviceId.ToString(),
            timestamp.ToString(),
            nonce,
            "valid-signature-base64",
            "POST",
            "/api/machines/telemetry",
            "{}");

        _canonicalBuilderMock
            .Build(timestamp, nonce, "POST", "/api/machines/telemetry", "{}")
            .Returns(new byte[] { 1, 2, 3 });

        _signatureVerifierMock
            .Verify("pk", Arg.Is<byte[]>(b => b.SequenceEqual(new byte[] { 1, 2, 3 })), "valid-signature-base64", "algo")
            .Returns(true);

        // Act
        var result = await _sut.ValidateAsync(context);

        // Assert
        result.Should().BeOfType<MachineAuthResult.Success>();
        var success = (MachineAuthResult.Success)result;
        success.MachineId.Should().Be(machineId);
        success.DeviceId.Should().Be(deviceId);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidSignature_ShouldReturnInvalidSignature()
    {
        // Assemble
        var machineId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var now = _timeProvider.GetUtcNow();
        var timestamp = now.ToUnixTimeSeconds();
        
        var device = new Device 
        { 
            DeviceId = deviceId, 
            SerialPhysical = "DEV-001",
            Status = DeviceStatus.Active, 
            PublicKey = "pk", 
            KeyAlgorithm = "algo" 
        };
        var machineDevice = new MachineDevice(machineId, deviceId, now.UtcDateTime);

        _db.Devices.Add(device);
        _db.MachineDevices.Add(machineDevice);
        await _db.SaveChangesAsync();

        var context = new MachineAuthContext(
            deviceId.ToString(),
            timestamp.ToString(),
            "1234567890123456",
            "wrong-signature",
            "POST",
            "/api/endpoint",
            "{}");

        _canonicalBuilderMock
            .Build(Arg.Any<long>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new byte[] { 1 });

        _signatureVerifierMock
            .Verify(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        // Act
        var result = await _sut.ValidateAsync(context);

        // Assert
        result.Should().BeOfType<MachineAuthResult.InvalidSignature>();
    }

    // =========================================================================
    // TESTS FOR YOUR PRACTICE (SKIPPED SO GIT PIPELINE STAYS GREEN)
    // =========================================================================

    [Fact(Skip = "TODO: Practice - Verify that an unreadable device ID (e.g. 'not-a-guid') returns InvalidHeader for X-Device-Id.")]
    public async Task ValidateAsync_WhenDeviceIdIsInvalid_ShouldReturnInvalidHeader()
    {
        // Assemble
        // TODO: Create a MachineAuthContext with DeviceIdHeader = "invalid-guid"
        
        // Act
        // TODO: var result = await _sut.ValidateAsync(context);

        // Assert
        // TODO: result.Should().BeOfType<MachineAuthResult.InvalidHeader>();
    }

    [Fact(Skip = "TODO: Practice - Verify that if the time drift is greater than 120s (e.g. Timestamp is 150s late), it returns TimestampOutOfWindow.")]
    public async Task ValidateAsync_WhenTimestampIsTooOld_ShouldReturnTimestampOutOfWindow()
    {
        // Assemble
        // var requestTime = _timeProvider.GetUtcNow().AddSeconds(-150).ToUnixTimeSeconds();
        // ...

        // Act
        
        // Assert
    }

    [Fact(Skip = "TODO: Practice - Insert a Device with Status = DeviceStatus.Revoked in the DB and verify the service returns DeviceNotFoundOrRevoked.")]
    public async Task ValidateAsync_WhenDeviceIsRevoked_ShouldReturnDeviceNotFoundOrRevoked()
    {
        // Assemble
        
        // Act
        
        // Assert
    }
    
    [Fact(Skip = "TODO: Practice - Insert a valid MachineDevice, call 'md.Detach(now)' (which sets IsActive = false), SaveChanges, and verify it returns DeviceNotAttached.")]
    public async Task ValidateAsync_WhenDeviceIsDetached_ShouldReturnDeviceNotAttached()
    {
        // Assemble
        
        // Act
        
        // Assert
    }
}
