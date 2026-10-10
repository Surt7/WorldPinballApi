using FluentAssertions;
using WorldPinball.Infrastructure.Services.Outbox;

namespace WorldPinball.Tests.Infrastructure.Services;

public class OutboxCleanupChannelTests
{
    private readonly OutboxCleanupChannel _sut = new();
    
    [Fact]
    public async Task RequestCleanupAsync_WhenCalled_ShouldWriteSignalToReader()
    {
        
        // Act
        await _sut.RequestCleanupAsync();
        
        // Assert
        var hasData = _sut.Reader.TryRead(out var signal);
        hasData.Should().BeTrue();
        signal.Should().BeTrue();
    }
    
    
    [Fact]
    public async Task RequestCleanupAsync_WhenCalledMultipleTimes_ShouldNotBlockAndRetainSingleSignal()
    {
        // Act: send multiple signals concurrently or in sequence
        await _sut.RequestCleanupAsync();
        
        await _sut.RequestCleanupAsync();
        await _sut.RequestCleanupAsync();
        
        // Assert: only the single bounded slot is consumed, next read is empty 
        _sut.Reader.TryRead(out _).Should().BeTrue();
        _sut.Reader.TryRead(out _).Should().BeFalse();
    }
}