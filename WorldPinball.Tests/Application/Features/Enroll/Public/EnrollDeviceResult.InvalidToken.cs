using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Public;
using WorldPinball.Application.Features.Machines.Public.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.Enroll.Public;

public class EnrollDeviceResultInvalidToken
{
    [Fact]
    public async Task HandleAsync_WhenTokenDoesNotExistInRepository_ShouldReturnInvalidToken()
    {
        var plainText = "plainText";
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        string tokenHash = Convert.ToHexString(hashBytes);


        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll =
            Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        repoDeviceReadEnroll
            .GetTokenByHashAsync(
                tokenHash, Arg.Any<CancellationToken>())
            .Returns((EnrollmentToken?)null);

        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            logger,
            TimeProvider.System
        );
        var result = await sut.HandleAsync(command);


        result.Should().BeOfType<EnrollDeviceResult.InvalidToken>();
    }

    [Fact]
    public async Task HandleAsync_WhenDeviceSerialAlreadyExistsInRepository_ShouldReturnDeviceAlreadyEnrolled()
    {
        var plainText = "plainText";
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        string tokenHash = Convert.ToHexString(hashBytes);
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll =
            Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll =
            Substitute.For<IDeviceEnrollmentWriteRepository>();
        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now,
            expiresAt: now.AddHours(1)
        );

        // null or already exist token case
        repoDeviceReadEnroll
            .GetTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(token);

        // serial device case
        repoDeviceReadEnroll
            .DeviceSerialExistsAsync("DEV-12345", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            logger,
            timeProvider
        );


        var result = await sut.HandleAsync(command);


        result.Should().BeOfType<EnrollDeviceResult.DeviceAlreadyEnrolled>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenAndMachineAreValid_ShouldReturnSuccess()
    {
        //Arrange
        var plainText = "plainText";
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        string tokenHash = Convert.ToHexString(hashBytes);
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll =
            Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll =
            Substitute.For<IDeviceEnrollmentWriteRepository>();
        
        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now,
            expiresAt: now.AddHours(1)
        );


        var tokenFetched = repoDeviceReadEnroll.GetTokenByHashAsync(tokenHash).Returns(token);
        repoDeviceReadEnroll.DeviceSerialExistsAsync(token.TokenHash, Arg.Any<CancellationToken>()).Returns(false);


        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );
        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            logger,
            timeProvider
        );
        var result = await sut.HandleAsync(command);
        result.Should().BeOfType<EnrollDeviceResult.Success>();
        token.UsesCount.Should().Be(1);

        var device = new Device
        {
            DeviceId = Guid.NewGuid(),
            SerialPhysical = command.DeviceSerialNumber,
            PublicKey = command.PublicKeyPem,
            CreatedAtUtc = now
        };
        var machineDevice = new MachineDevice
        {
            MachineId = command.MachineId,
            DeviceId = device.DeviceId,
            AttachedAtUtc = now,
            IsActive = true
        };


        await repoDeviceWriteEnroll.Received(1).SaveEnrollmentAsync(
            Arg.Any<Device>(),
            Arg.Any<MachineDevice>(),
            token,
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsExpired_ShouldReturnInvalidToken()
    {
        var plainText = "plainText";
        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        string tokenHash = Convert.ToHexString(hashBytes);
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now,
            expiresAt: now.AddHours(-1)
        );
        
        
        var tokenFetched = repoDeviceReadEnroll.GetTokenByHashAsync(tokenHash).Returns(token);
        repoDeviceReadEnroll.DeviceSerialExistsAsync("DEV-12345", Arg.Any<CancellationToken>()).Returns(false);
        
        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            logger,
            timeProvider
        );


        var result = await sut.HandleAsync(command);
 
        result.Should().BeOfType<EnrollDeviceResult.InvalidToken>();
        token.UsesCount.Should().Be(0);

        await repoDeviceWriteEnroll.DidNotReceive().SaveEnrollmentAsync(
            Arg.Any<Device>(),
            Arg.Any<MachineDevice>(),
            Arg.Any<EnrollmentToken>(),
            Arg.Any<CancellationToken>()
        );
        
    
    }
}