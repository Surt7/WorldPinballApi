using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WorldPinball.Application.Features.StreamingAccess.Admin;
using WorldPinball.Application.Features.StreamingAccess.Admin.Commands;
using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Tests.Application.Features.StreamingAccess.Admin;

public class ApproveStreamingAccessHandlerTests
{
    private readonly IStreamingAccessRequestRepository _requestRepo = Substitute.For<IStreamingAccessRequestRepository>();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 9, 30, 19, 30, 0, TimeSpan.Zero));
    private readonly NullLogger<ApproveStreamingAccessHandler> _logger = NullLogger<ApproveStreamingAccessHandler>.Instance;
    private readonly ApproveStreamingAccessHandler _sut;

    public ApproveStreamingAccessHandlerTests()
    {
        _sut = new ApproveStreamingAccessHandler(_requestRepo, _timeProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ShouldApproveRequestAndPersist()
    {
        // Arrange
        var request = new StreamingAccessRequest(Guid.NewGuid(), Guid.NewGuid(), _timeProvider.GetUtcNow().UtcDateTime);
        _requestRepo.GetByIdAsync(request.RequestId, Arg.Any<CancellationToken>()).Returns(request);

        var duration = TimeSpan.FromHours(3);
        var command = new ApproveStreamingAccessCommand(request.RequestId, duration);

        // Act
        var result = await _sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ApproveStreamingAccessResult.Success>();
        request.Status.Should().Be(StreamingRequestStatus.Approved);
        await _requestRepo.Received(1).UpdateAsync(request, Arg.Any<CancellationToken>());
    }
}