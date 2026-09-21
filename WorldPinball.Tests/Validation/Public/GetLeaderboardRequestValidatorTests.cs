using FluentAssertions;
using WorldPinball.Api.Contracts.Leaderboard;
using WorldPinball.Api.Validation.Public;

namespace WorldPinball.Tests.Validation.Public;

public sealed class GetLeaderboardRequestValidatorTests
{
    private readonly GetLeaderboardRequestValidator _validator = new();
    [Fact]
    public void Validate_WhenRequestHasValidGuids_ShouldBeValid()
    {
        // Arrange
        var request = new GetLeaderboardRequest(
            IpId: Guid.NewGuid(),
            ModeId: Guid.NewGuid(),
            Limit: 50);
        // Act
        var result = _validator.Validate(request);
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(201)]
    public void Validate_WhenLimitIsOutOfRange_ShouldHaveError(int invalidLimit)
    {
        // Arrange
        var request = new GetLeaderboardRequest(
            IpId: Guid.NewGuid(),
            ModeId: Guid.NewGuid(),
            Limit: invalidLimit);
        // Act
        var result = _validator.Validate(request);
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(GetLeaderboardRequest.Limit));
    }
    
    [Fact]
    public void Validate_WhenBothGuidsAndSlugsAreMissing_ShouldHaveError()
    {
        // Arrange (Tout est vide)
        var request = new GetLeaderboardRequest(Limit: 50);
        // Act
        var result = _validator.Validate(request);
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Either"));
    }
    
}