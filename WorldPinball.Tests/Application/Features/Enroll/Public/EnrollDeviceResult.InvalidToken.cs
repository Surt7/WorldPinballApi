using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Public;
using WorldPinball.Application.Features.Machines.Public.Enroll;
using WorldPinball.Application.Features.Machines.Public.Enroll.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.CA;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;
using WorldPinball.Infrastructure.Security.Secrets;

namespace WorldPinball.Tests.Application.Features.Enroll.Public;

public class EnrollDeviceResultInvalidToken
{
    private readonly ISecretService _secretService = new SecretService(new EphemeralDataProtectionProvider());

    private static (string PlainToken, string TokenHash) GenerateValidTokenPair()
    {
        byte[] rawBytes = new byte[32];
        RandomNumberGenerator.Fill(rawBytes);
        string plainToken = Convert.ToBase64String(rawBytes);
        string tokenHash = Convert.ToHexString(SHA256.HashData(rawBytes)).ToLowerInvariant();
        return (plainToken, tokenHash);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenFormatIsInvalid_ShouldReturnInvalidToken()
    {
        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var caService = Substitute.For<ICertificateAuthorityService>();

        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: "not-a-valid-base64-token",
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            caService,
            _secretService,
            logger,
            TimeProvider.System
        );
        var result = await sut.HandleAsync(command);

        result.Should().BeOfType<EnrollDeviceResult.InvalidToken>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenDoesNotExistInRepository_ShouldReturnInvalidToken()
    {
        var (plainText, tokenHash) = GenerateValidTokenPair();

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var caService = Substitute.For<ICertificateAuthorityService>();

        repoDeviceReadEnroll
            .GetTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>())
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
            caService,
            _secretService,
            logger,
            TimeProvider.System
        );
        var result = await sut.HandleAsync(command);

        result.Should().BeOfType<EnrollDeviceResult.InvalidToken>();
    }

    [Fact]
    public async Task HandleAsync_WhenDeviceSerialAlreadyExistsInRepository_ShouldReturnDeviceAlreadyEnrolled()
    {
        var (plainText, tokenHash) = GenerateValidTokenPair();
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var caService = Substitute.For<ICertificateAuthorityService>();

        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now,
            expiresAt: now.AddHours(1)
        );

        repoDeviceReadEnroll
            .GetTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(token);

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
            caService,
            _secretService,
            logger,
            timeProvider
        );

        var result = await sut.HandleAsync(command);

        result.Should().BeOfType<EnrollDeviceResult.DeviceAlreadyEnrolled>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenAndMachineAreValid_ShouldReturnSuccess()
    {
        var (plainText, tokenHash) = GenerateValidTokenPair();
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var caService = Substitute.For<ICertificateAuthorityService>();

        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now,
            expiresAt: now.AddHours(1)
        );

        repoDeviceReadEnroll
            .GetTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(token);

        repoDeviceReadEnroll
            .DeviceSerialExistsAsync("DEV-12345", Arg.Any<CancellationToken>())
            .Returns(false);

        caService.IssueDeviceCertificateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new IssuedCertificate(
                CertPem: "-----BEGIN CERTIFICATE-----\nTEST_CERT\n-----END CERTIFICATE-----",
                SerialNumber: "cert-serial-123",
                ThumbprintSha256: "thumbprint-123",
                SubjectDn: "CN=DEV-12345",
                IssuerDn: "CN=WorldPinball Intermediate CA",
                NotBeforeUtc: now,
                NotAfterUtc: now.AddYears(1)));

        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            caService,
            _secretService,
            logger,
            timeProvider
        );

        var result = await sut.HandleAsync(command);

        result.Should().BeOfType<EnrollDeviceResult.Success>();
        token.UsesCount.Should().Be(1);

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
        var (plainText, tokenHash) = GenerateValidTokenPair();
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var logger = NullLogger<EnrollDeviceHandler>.Instance;
        var repoDeviceReadEnroll = Substitute.For<IDeviceEnrollmentReadRepository>();
        var repoDeviceWriteEnroll = Substitute.For<IDeviceEnrollmentWriteRepository>();
        var caService = Substitute.For<ICertificateAuthorityService>();

        var token = new EnrollmentToken(
            tokenHash: tokenHash,
            tokenType: EnrollmentTokenType.DeviceEnroll,
            issuedByUserId: "admin-1",
            issuedAt: now.AddHours(-2),
            expiresAt: now.AddHours(-1)
        );

        repoDeviceReadEnroll
            .GetTokenByHashAsync(tokenHash, Arg.Any<CancellationToken>())
            .Returns(token);

        repoDeviceReadEnroll
            .DeviceSerialExistsAsync("DEV-12345", Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new EnrollDeviceCommand(
            MachineId: Guid.NewGuid(),
            DeviceSerialNumber: "DEV-12345",
            PlainEnrollmentToken: plainText,
            PublicKeyPem: "PUBLIC_KEY"
        );

        var sut = new EnrollDeviceHandler(
            repoDeviceWriteEnroll,
            repoDeviceReadEnroll,
            caService,
            _secretService,
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
