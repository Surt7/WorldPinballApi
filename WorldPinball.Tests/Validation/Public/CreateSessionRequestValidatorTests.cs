using FluentAssertions;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Api.Validation.Admin;
using WorldPinball.Api.Validation.Public;

namespace WorldPinball.Tests.Validation.Public;

public sealed class CreateSessionRequestValidatorTests
{
    private readonly CreateSessionRequestValidator _validator = new();

    private static readonly CreateGameSessionRequest ValidRequest = new(
        ModeCode: "classic",
        ClientVersion: "1.0.0"
    );

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(ValidRequest);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenModeCodeIsEmpty_ShouldHaveError(string? invalidModeCode)
    {
        var request = ValidRequest with { ModeCode = invalidModeCode! };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateGameSessionRequest.ModeCode));
    }

    [Fact]
    public void Validate_WhenModeCodeExceeds32Characters_ShouldHaveError()
    {
        var request = ValidRequest with { ModeCode = new string('a', 33) };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateGameSessionRequest.ModeCode));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("v1.0.0")]
    [InlineData("1.0")]
    [InlineData("1.0.0-beta")]
    [InlineData("abc")]
    public void Validate_WhenClientVersionIsInvalid_ShouldHaveError(string? invalidVersion)
    {
        var request = ValidRequest with { ClientVersion = invalidVersion! };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateGameSessionRequest.ClientVersion));
    }
}
