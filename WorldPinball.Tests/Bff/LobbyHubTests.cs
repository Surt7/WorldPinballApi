extern alias BffApp;

namespace WorldPinball.Tests.Bff;

using FluentAssertions;
using NSubstitute;
using Microsoft.AspNetCore.SignalR;
using ILobbyRelayService = BffApp::WorldPinball.Bff.Services.ILobbyRelayService;
using LobbyHub = BffApp::WorldPinball.Bff.Hubs.LobbyHub;

public class LobbyHubTests
{
    private readonly ILobbyRelayService _relayService = Substitute.For<ILobbyRelayService>();
    private readonly IGroupManager _groups = Substitute.For<IGroupManager>();
    private readonly HubCallerContext _context = Substitute.For<HubCallerContext>();
    
    [Fact]
    public async Task JoinLobby_WhenValidSessionId_AddsConnectionToGroupAndEnsuresRelayWatching()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _context.ConnectionId.Returns("conn-123");
        var sut = new LobbyHub(_relayService) { Context = _context, Groups = _groups };

        // Act
        await sut.JoinLobby(sessionId.ToString());

        // Assert
        await _groups.Received(1).AddToGroupAsync("conn-123",
            sessionId.ToString(), Arg.Any<CancellationToken>());
        
        await _relayService.Received(1).EnsureWatchingLobbyAsync(sessionId,
            Arg.Any<CancellationToken>());
        
    }

    
    [Fact]
    public async Task LeaveLobby_WhenCalled_RemovesConnectionFromGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _context.ConnectionId.Returns("conn-123");
        var sut = new LobbyHub(_relayService) { Context = _context, Groups = _groups };

        // Act
        await sut.LeaveLobby(sessionId.ToString());

        // Assert
        await _groups.Received(1).RemoveFromGroupAsync("conn-123", 
            sessionId.ToString(), Arg.Any<CancellationToken>());
    }

    
    [Fact]
    public async Task JoinLobby_WhenCalledWithValidSession_AddsToGroupAndTriggersRelay()
    {
        var sessionId = Guid.NewGuid();
        _context.ConnectionId.Returns("conn-1");
        var sut = new LobbyHub(_relayService) { Context = _context, Groups = _groups };
        
        await sut.JoinLobby(sessionId.ToString());
        
        await _groups.Received(1).AddToGroupAsync(
            "conn-1", sessionId.ToString(),
            Arg.Any<CancellationToken>());
        
        await _relayService.Received(1).EnsureWatchingLobbyAsync(
            sessionId, Arg.Any<CancellationToken>());
        
        
    }

    
    

}