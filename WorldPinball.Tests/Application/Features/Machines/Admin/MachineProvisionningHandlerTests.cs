using FluentAssertions;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Features.Machines.Admin.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Security;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Application.Features.Machines.Admin;

public sealed class MachineProvisionningHandlerTests
{

    [Fact]
    public async Task HandleAsync_WhenIpIsUnknown_ShouldReturnUnknownIp()
    {
        // Arrange
        const string ipSlug = "resident-evil";

        var ipLicenseRepository =
            Substitute.For<IIpLicenseRepository>();

        var machineWriteRepository =
            Substitute.For<IMachineWriteRepository>();

        var secretService = Substitute.For<ISecretService>();

        ipLicenseRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns((IpLicense?)null);

        var handler = new ProvisionMachineHandler(
            ipLicenseRepository,
            machineWriteRepository,
            secretService,
            TimeProvider.System);

        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: "WP-123",
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var unknownIp = result.Should()
            .BeOfType<ProvisionMachineResult.UnknownIp>()
            .Which;

        unknownIp.IpSlug.Should().Be(ipSlug);

        await ipLicenseRepository
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

        var ipLicenseRepository =
            Substitute.For<IIpLicenseRepository>();

        var machineWriteRepository =
            Substitute.For<IMachineWriteRepository>();

        var secretService = Substitute.For<ISecretService>();

        var activeIp = new IpLicense
        {
            IpId = Guid.NewGuid(),
            Name = "Resident Evil",
            Slug = ipSlug,
            IsActive = true
        };

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
            TimeProvider.System);

        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: serialNumber,
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
            IssuedByUserId: "admin-123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        var duplicateSerial = result.Should()
            .BeOfType<ProvisionMachineResult.DuplicateSerial>()
            .Which;
        
        duplicateSerial.SerialNumber.Should().Be(serialNumber);
    }
    
    
    [Fact]
    public async Task HandleAsync_WhenProvisioningDataIsValid_ShouldReturnSuccess()
    {
        
        //Arrange
        const string serialNumber = "WP-123";
        const string ipSlug = "resident-evil";
        const string plainToken = "plain-enrollment-token";
        const string tokenHash = "hashed-enrollment-token";
        
        var ipLicenseRepository = Substitute.For<IIpLicenseRepository>();
        
        var machineWriteRepository = Substitute.For<IMachineWriteRepository>();
        var secretService = Substitute.For<ISecretService>();

        var activeIp = new IpLicense
        {
            IpId = Guid.NewGuid(),
            Name = "Resident Evil",
            Slug = ipSlug,
            IsActive = true
        };
        
        ipLicenseRepository
            .GetActiveBySlugAsync(
                ipSlug,
                Arg.Any<CancellationToken>())
            .Returns(activeIp);
        
        
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
            ipLicenseRepository,
            machineWriteRepository,
            secretService,
            TimeProvider.System);
        
        var command = new ProvisionMachineCommand(
            WorldPinballSerialNumber: serialNumber,
            IpSlug: ipSlug,
            DisplayName: "Resident Evil",
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
}
