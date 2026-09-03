using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin;
using WorldPinball.Application.Features.Machines.Admin.Commands;
using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Api.Controllers.Admin;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace WorldPinball.Tests.Controller;

public class AdminMachinesControllerTests
{
    [Fact]
    public async Task Provision_WhenHandlerReturnsSuccess_ShouldReturnOk()
    {
        var expectedMachineId = Guid.NewGuid();
        const string expectedEnrollmentToken = "enrollment-token";
        
               var validator = Substitute.For<IValidator<ProvisionMachineRequest>>();
        validator
            .ValidateAsync(
                Arg.Any<ProvisionMachineRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        
        var handler = Substitute.For<IProvisionMachineHandler>();

        handler.HandleAsync(
                Arg.Any<ProvisionMachineCommand>(), 
                Arg.Any<CancellationToken>())
            .Returns(new ProvisionMachineResult.Success(
                expectedMachineId,
                expectedEnrollmentToken
            ));
        
        var request = new ProvisionMachineRequest(
            "WP-123",
            "resident-evil",
            "Resident Evil",
            null);
        
        var controller = new AdminMachinesController(handler,validator);

        var result = await controller.Provision(
            request,
            CancellationToken.None);
        
        var okResult = result
            .Result
            .Should()
            .BeOfType<OkObjectResult>()
            .Which;
        
        var response = okResult
            .Value
            .Should()
            .BeOfType<ProvisionMachineResponse>()
            .Which;
        
        response.MachineId.Should().Be(expectedMachineId);
        response.EnrollmentToken.Should().Be(expectedEnrollmentToken);
    }
}