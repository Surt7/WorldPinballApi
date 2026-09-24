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
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Dtos;
using WorldPinball.Application.Features.Leaderboard.Admin.Dashboard.Queries;
using WorldPinball.Application.Features.Machines.Admin.Provision;
using WorldPinball.Application.Features.Machines.Admin.Provision.Commands;
using WorldPinball.Application.Features.Machines.Admin.ReissueToken;

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
        var reissueHandler = Substitute.For<IReissueEnrollmentTokenHandler>();
        var controller = new AdminMachinesController(handler, reissueHandler, validator);
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

    [Fact]
    public async Task GetDashboard_WhenMachineExists_ShouldReturnOkWithDashboardDto()
    {
        var machineId = Guid.NewGuid();

        var expectedDto = new MachineDashboardDto(
            machineId,
            "TZ-01",                // SerialNumber
            42,                     // TotalGamesPlayed
            1500000,                // HighScore
            250000,                 // AverageScore
            10,                     // TotalUniquePlayers
            DateTime.UtcNow);       // LastPlayedAtUtc
        
        var controller = new AdminMachinesController(
            Substitute.For<IProvisionMachineHandler>(),
            Substitute.For<IReissueEnrollmentTokenHandler>(),
            Substitute.For<IValidator<ProvisionMachineRequest>>());
        var handler = Substitute.For<IGetMachineDashboardHandler>();
        
        handler.HandleAsync(
                Arg.Is<GetMachineDashboardQuery>(q => q.MachineId == machineId),
                Arg.Any<CancellationToken>())
            .Returns(new GetMachineDashboardResult.Success(expectedDto));
        
        var result = await controller.GetDashboard(machineId, handler, CancellationToken.None);
        
        
        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedDto);
    }
    
    
    [Fact]
    public async Task GetDashboard_WhenMachineNotFound_ShouldReturnNotFound()
    {
        var machineId = Guid.NewGuid();
        
        var controller = new AdminMachinesController(
            Substitute.For<IProvisionMachineHandler>(),
            Substitute.For<IReissueEnrollmentTokenHandler>(),
            Substitute.For<IValidator<ProvisionMachineRequest>>());
        var handler = Substitute.For<IGetMachineDashboardHandler>();
        
        handler.HandleAsync(
                Arg.Is<GetMachineDashboardQuery>(q => q.MachineId == machineId),
                Arg.Any<CancellationToken>())
            .Returns(new GetMachineDashboardResult.NotFound(machineId));
        
        var result = await controller.GetDashboard(machineId, handler, CancellationToken.None);


        result.Result.Should().BeOfType<NotFoundObjectResult>();

    }
}