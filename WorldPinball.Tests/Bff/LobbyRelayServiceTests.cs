extern alias BffApp;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using ILogger = Microsoft.Extensions.Logging.ILogger<BffApp::WorldPinball.Bff.Services.LobbyRelayService>;
using LobbyHub = BffApp::WorldPinball.Bff.Hubs.LobbyHub;

using SessionLobbyResponse = BffApp::WorldPinball.Protos.V1.GameSessions.SessionLobbyResponse;

namespace WorldPinball.Tests.Bff;

using FluentAssertions;
using NSubstitute;
using LobbyRelayService = BffApp::WorldPinball.Bff.Services.LobbyRelayService;
using GameSessionService = BffApp::WorldPinball.Protos.V1.GameSessions.GameSessionService;
using WatchLobbyRequest = BffApp::WorldPinball.Protos.V1.GameSessions.WatchLobbyRequest;

public class LobbyRelayServiceTests
{
    private readonly GameSessionService.GameSessionServiceClient _grpcClient =
        Substitute.For<GameSessionService.GameSessionServiceClient>();

    private readonly IHubContext<LobbyHub> _hubContext = Substitute.For<IHubContext<LobbyHub>>();
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    /*
     * https://learn.microsoft.com/en-us/aspnet/core/grpc/test-services?view=aspnetcore-10.0
     * We have to mock like that as the AsyncServerStreamingCall<T> encapsulate Http/2
     * statusCode and metadata
     * */
    private static AsyncServerStreamingCall<SessionLobbyResponse> CreateFakeStreamingCall()
    {
        var responseStream = Substitute.For<IAsyncStreamReader<SessionLobbyResponse>>();
        return new AsyncServerStreamingCall<SessionLobbyResponse>(
            responseStream,
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });
    }


    [Fact]
    public async Task EnsureWatchingLobbyAsync_WhenSessionNotBeingWatched_InitiatesGrpcWatchStream()
    {
        var callInvoked = new TaskCompletionSource<bool>();
        _grpcClient.WatchLobby(Arg.Any<WatchLobbyRequest>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callInvoked.TrySetResult(true);
                return CreateFakeStreamingCall();
            });

        var sessionId = Guid.NewGuid();
        var sut = new LobbyRelayService(_grpcClient, _hubContext, _logger);

        await sut.EnsureWatchingLobbyAsync(sessionId, CancellationToken.None);
        await Task.WhenAny(callInvoked.Task, Task.Delay(5000));

        _grpcClient.Received(1).WatchLobby(
            Arg.Is<WatchLobbyRequest>(r =>
                r.SessionId == sessionId.ToString()),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnsureWatchingLobbyAsync_WhenSessionAlreadyBeingWatched_DoesNotInitiateDuplicateGrpcStream()
    {
        var callInvoked = new TaskCompletionSource<bool>();
        _grpcClient.WatchLobby(Arg.Any<WatchLobbyRequest>(), cancellationToken: Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callInvoked.TrySetResult(true);
                return CreateFakeStreamingCall();
            });

        // Arrange
        var sessionId = Guid.NewGuid();
        var sut = new LobbyRelayService(_grpcClient, _hubContext, _logger);

        // Act 
        await sut.EnsureWatchingLobbyAsync(sessionId, CancellationToken.None);
        await sut.EnsureWatchingLobbyAsync(sessionId, CancellationToken.None);
        await Task.WhenAny(callInvoked.Task, Task.Delay(5000));

        // Assert  
        _grpcClient.Received(1).WatchLobby(
            Arg.Is<WatchLobbyRequest>(r => r.SessionId == sessionId.ToString()),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
