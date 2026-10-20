

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
        var result = SensitiveDataMasker.Mask(key, value);

        result.Should().Be("[REDACTED]");
    }

    [Fact]
    public void Mask_WhenKeyIsNotSensitive_ShouldReturnOriginalValue()
    {
        var result = SensitiveDataMasker.Mask("username", "pinball_wizard");
       
        result.Should().Be("pinball_wizard");
    }
    
}