using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security;
using WorldPinball.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;

namespace WorldPinball.Tests.Application.Features.Machines.Admin;

public sealed class MachineProvisionningHandlerTests
{

    [Fact]
    public async Task HandleAsync_WhenIpIsUnknown_ShouldReturnUnknownIp()
    {
        // Arrange
        const string ipSlug = "resident-evil";

        var ipLicenseReadRepository =
            Substitute.For<IIpLicenseReadRepository>();

        var machineWriteRepository =
            Substitute.For<IMachineWriteRepository>();

        var logger = new FakeLogger<ProvisionMachineHandler>();
        var secretService = Substitute.For<ISecretService>();
        var idempotencyKey = Guid.NewGuid();
        ipLicenseReadRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns((IpLicense?)null);

        var handler = new ProvisionMachineHandler(
            ipLicenseReadRepository,
            machineWriteRepository,
            secretService,
            logger,
            TimeProvider.System);

        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: "WP-123",
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IdempotencyKey: idempotencyKey,
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var unknownIp = result.Should()
            .BeOfType<ProvisionMachineResult.UnknownIp>()
            .Which;

        unknownIp.IpSlug.Should().Be(ipSlug);

        await ipLicenseReadRepository
            .Received(1)
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task HandleAsync_WhenSerialAlreadyExists_ShouldReturnDuplicateSerial()
    {
        // Arrange
        const string serialNumber = "WP-123";
        const string ipSlug = "resident-evil";

        var logger = new FakeLogger<ProvisionMachineHandler>();
        var ipLicenseRepository =
            Substitute.For<IIpLicenseReadRepository>();

        var machineWriteRepository =
            Substitute.For<IMachineWriteRepository>();

        var secretService = Substitute.For<ISecretService>();

        var idempotencyKey = Guid.NewGuid();
        var activeIp = new IpLicense(
            name: "Resident Evil",
            slug: ipSlug,
            isActive: true,
            ipId: Guid.NewGuid());

        ipLicenseRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns(activeIp);

        machineWriteRepository
            .SerialExistsAsync(
                serialNumber,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new ProvisionMachineHandler(
            ipLicenseRepository,
            machineWriteRepository,
            secretService,
            logger,
            TimeProvider.System);

        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: serialNumber,
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IdempotencyKey: idempotencyKey,
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var duplicateSerial = result.Should()
            .BeOfType<ProvisionMachineResult.DuplicateSerial>()
            .Which;

        duplicateSerial.SerialNumber.Should().Be(serialNumber);

        await machineWriteRepository
            .Received(1)
            .SerialExistsAsync(
                serialNumber,
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldReturnSuccess()
    {
        //Arrange
        const string serialNumber = "WP-123";
        const string ipSlug = "resident-evil";
        const string plainToken = "plain-enrollment-token";
        const string tokenHash = "hashed-enrollment-token";
        
        var ipLicenseReadRepository = Substitute.For<IIpLicenseReadRepository>();
        
        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();
        var idempotencyKey = Guid.NewGuid();
        
        var activeIp = new IpLicense(
            name: "Resident Evil",
            slug: ipSlug,
            isActive: true,
            ipId: Guid.NewGuid());
        
        ipLicenseReadRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns(activeIp);
        
        var logger = new FakeLogger<ProvisionMachineHandler>();
        
        machineWriteRepository.SerialExistsAsync(
            serialNumber,
            Arg.Any<CancellationToken>()
        ).Returns(false);
        
        secretService.Generate()
            .Returns(new SecretGenerationResult(
                PlainBase64: plainToken,
                HashHex: tokenHash,
                Encrypted:"encrypted-token"));
            
        var handler = new ProvisionMachineHandler(
            ipLicenseReadRepository,
            machineWriteRepository,
            secretService,
            logger,
            TimeProvider.System);
        
        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: serialNumber,
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IdempotencyKey: idempotencyKey,
            IssuedByUserId: "admin-123");
        
        // Act
        var result = await handler.HandleAsync(command);

        // Assert 
        var success = result.Should()
            .BeOfType<ProvisionMachineResult.Success>()
            .Which;
        
        success.MachineId.Should().NotBeEmpty();
        success.EnrollmentToken.Should().Be(plainToken);
        
        await machineWriteRepository
            .Received(1)
            .AddProvisioningAsync(
                Arg.Any<Machine>(),
                Arg.Any<EnrollmentToken>(),
        Arg.Any<CancellationToken>());

    }


    [Fact]
    public async Task HandleAsync_WhenIdempotencyKeyIsSame_ShouldReturnSameIdempotencyKey()
    {
    //Arrange
        const string serialNumber = "WP-123";
        const string ipSlug = "resident-evil";
        const string plainToken = "plain-enrollment-token";
        const string tokenHash = "hashed-enrollment-token";
        // const string nonce = "hmylhaDqKqx9Y/V04JWEIKa8rvZYpiJLDYEfmRAupdA=";
        
        var ipLicenseReadRepository = Substitute.For<IIpLicenseReadRepository>();
        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();
        var idempotencyKey = Guid.NewGuid();
        
        
        var activeIp = new IpLicense(
            name: "Resident Evil",
            slug: ipSlug,
            isActive: true,
            ipId: Guid.NewGuid());
        
        ipLicenseReadRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns(activeIp);
        
        var logger = new FakeLogger<ProvisionMachineHandler>();
     
        
        secretService.Generate()
            .Returns(new SecretGenerationResult(
                PlainBase64: plainToken,
                HashHex: tokenHash,
                Encrypted:"encrypted-token"));
            
        var handler = new ProvisionMachineHandler(
            ipLicenseReadRepository,
            machineWriteRepository,
            secretService,
            logger,
            TimeProvider.System);
        
        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: serialNumber,
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IdempotencyKey: idempotencyKey,
            IssuedByUserId: "admin-123");


        machineWriteRepository.SameIdempotencyKeyExistsAsync(idempotencyKey, Arg.Any<CancellationToken>())
            .Returns(true);
        
        // Act
        var result = await handler.HandleAsync(command);

        // Assert 
        var sameIdempotentcy = result.Should()
            .BeOfType<ProvisionMachineResult.SameIdempotencyKey>()
            .Which;


        await machineWriteRepository.DidNotReceive().AddProvisioningAsync(Arg.Any<Machine>(),Arg.Any<EnrollmentToken>(),Arg.Any<CancellationToken>());

    }
}
