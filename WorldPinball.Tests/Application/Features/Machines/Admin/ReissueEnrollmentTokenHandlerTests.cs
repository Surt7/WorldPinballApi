using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security.Secrets;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.Machines.Admin;

public sealed class ReissueEnrollmentTokenHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenMachineDoesNotExist_ShouldReturnMachineNotFound()
    {
        // Arrange
        var machineId = Guid.NewGuid();
        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();
        var logger = new FakeLogger<ReissueEnrollmentTokenHandler>();

        machineWriteRepository
            .GetByIdAsync(machineId, Arg.Any<CancellationToken>())
            .Returns((Machine?)null);

        var handler = new ReissueEnrollmentTokenHandler(
            machineWriteRepository,
            secretService,
            logger,
            TimeProvider.System);

        var command = new ReissueEnrollmentTokenCommand(
            MachineId: machineId,
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var notFound = result.Should()
            .BeOfType<ReissueEnrollmentTokenResult.MachineNotFound>()
            .Which;

        notFound.MachineId.Should().Be(machineId);

        await machineWriteRepository
            .DidNotReceive()
            .AddEnrollmentTokenAsync(
                Arg.Any<EnrollmentToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMachineStatusIsNotPendingEnrollment_ShouldReturnInvalidMachineStatus()
    {
        // Arrange
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();
        var logger = new FakeLogger<ReissueEnrollmentTokenHandler>();

        var machine = new Machine(
            ipId: Guid.NewGuid(),
            serialNumber: "WP-TEST-ACTIVE",
            displayName: "Test Active Pinball",
            registeredAtUtc: now,
            idempotencyKey: Guid.NewGuid(),
            nonce: "nonce-123");

        machine.CompleteEnrollment(); // Status = MachineStatus.Active
        var machineId = machine.MachineId;

        machineWriteRepository
            .GetByIdAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(machine);

        var handler = new ReissueEnrollmentTokenHandler(
            machineWriteRepository,
            secretService,
            logger,
            timeProvider);

        var command = new ReissueEnrollmentTokenCommand(
            MachineId: machineId,
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var invalidStatus = result.Should()
            .BeOfType<ReissueEnrollmentTokenResult.InvalidMachineStatus>()
            .Which;

        invalidStatus.MachineId.Should().Be(machineId);
        invalidStatus.Status.Should().Be(MachineStatus.Active);

        await machineWriteRepository
            .DidNotReceive()
            .AddEnrollmentTokenAsync(
                Arg.Any<EnrollmentToken>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenMachineIsPendingEnrollment_ShouldSaveTokenAndReturnSuccess()
    {
        // Arrange
        var fixedNow = new DateTimeOffset(2026, 9, 23, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(fixedNow);
        var now = fixedNow.UtcDateTime;

        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();
        var logger = new FakeLogger<ReissueEnrollmentTokenHandler>();

        var machine = new Machine(
            ipId: Guid.NewGuid(),
            serialNumber: "WP-TEST-PENDING",
            displayName: "Test Pending Pinball",
            registeredAtUtc: now,
            idempotencyKey: Guid.NewGuid(),
            nonce: "nonce-456");

        var machineId = machine.MachineId;

        machineWriteRepository
            .GetByIdAsync(machineId, Arg.Any<CancellationToken>())
            .Returns(machine);

        const string plainToken = "reissued-plain-token-base64";
        const string tokenHashHex = "hash-hex-reissued-token";

        secretService.Generate().Returns(new SecretGenerationResult(plainToken, tokenHashHex, "encrypted-secret"));

        var handler = new ReissueEnrollmentTokenHandler(
            machineWriteRepository,
            secretService,
            logger,
            timeProvider);

        var command = new ReissueEnrollmentTokenCommand(
            MachineId: machineId,
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var success = result.Should()
            .BeOfType<ReissueEnrollmentTokenResult.Success>()
            .Which;

        success.MachineId.Should().Be(machineId);
        success.EnrollmentToken.Should().Be(plainToken);
        success.ExpiresAtUtc.Should().Be(now.AddHours(24));

        await machineWriteRepository
            .Received(1)
            .AddEnrollmentTokenAsync(
                Arg.Is<EnrollmentToken>(t =>
                    t.MachineId == machineId &&
                    t.TokenHash == tokenHashHex &&
                    t.IssuedByUserId == "admin-123" &&
                    t.TokenType == EnrollmentTokenType.DeviceEnroll &&
                    t.ExpiresAtUtc == now.AddHours(24)),
                Arg.Any<CancellationToken>());
    }
}
