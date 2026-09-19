using NSubstitute;

using WorldPinball.Application.Contracts.Admin.Machine;
using WorldPinball.Api.Controllers.Admin;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using WorldPinball.Api.Contracts.Machine;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;

namespace WorldPinball.Tests.Controller;

public class AdminMachinesControllerTests
{
    [Fact]
    public async Task Provision_WhenHandlerReturnsSuccess_ShouldReturnOk()
    {
        var expectedMachineId = Guid.NewGuid();
        const string expectedEnrollmentToken = "enrollment-token";
        var idempotencyKey = Guid.NewGuid();
               var validator = Substitute.For<IValidator<ProvisionMachineRequest>>();

               int? validityHour = 1;
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
            idempotencyKey,
            "Resident Evil",
            null,
            validityHour);
        
        // create controller
        var controller = new AdminMachinesController(handler,validator);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                            new Claim("sub", "test-admin-id")
                        ],
                        authenticationType: "Test"))
            }
        };

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
