using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;
using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;

namespace WorldPinball.Tests.Application.Features.CreateOperatorToken;

public class CreateOperatorTokenHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldCreateTokenAndReturnSuccess()
    {
        // Arrange
        var repositoryWrite = Substitute.For<IOperatorTokenWriteRepository>();
        var logger = new FakeLogger<CreateOperatorTokenHandler>();
        var timeProvider = TimeProvider.System;

        var handler = new CreateOperatorTokenHandler(
            repositoryWrite,
            timeProvider,
            logger
        );

        var command = new CreateOperatorTokenCommand(
            Name: "USB Technician Key",
            Scopes: "enroll",
            ValidityDays: 30,
            IssuedByUserId: "admin-456");
        // Act
        var result = await handler.HandleAsync(command);
//Assert

        await repositoryWrite.Received(1).AddAsync(
            Arg.Is<OperatorToken>(t => t.Name == "USB Technician Key"
                                       && t.Scopes == "enroll"));
    }
}