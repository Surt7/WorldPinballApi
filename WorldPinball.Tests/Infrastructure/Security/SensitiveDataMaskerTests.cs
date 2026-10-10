

using FluentAssertions;
using WorldPinball.Infrastructure.Security;

namespace WorldPinball.Tests.Infrastructure.Security;



public class SensitiveDataMaskerTests
{

    [Theory]
    [InlineData("password", "Secret123!")]
    [InlineData("Authorization", "Bearer eyJhbGci...")]
    [InlineData("ApiKey", "api_key_xyz")]
    public void Mask_WhenKeyIsSensitive_ShouldReturnRedacted(string key, string value)
    {
        var sut = new SensitiveDataMasker();
        
        var result = sut.Mask(key, value);

        result.Should().Be("[REDACTED]");
    }

    [Fact]
    public void Mask_WhenKeyIsNotSensitive_ShouldReturnOriginalValue()
    {
        // Arrange
        var sut = new SensitiveDataMasker();
        // Act
        var result = sut.Mask("username", "pinball_wizard");
        // Assert
        result.Should().Be("pinball_wizard");
    }
    
}