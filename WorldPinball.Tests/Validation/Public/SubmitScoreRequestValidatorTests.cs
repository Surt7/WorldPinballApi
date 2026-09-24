using FluentAssertions;
using WorldPinball.Api.Contracts.Sessions;
using WorldPinball.Api.Validation.Admin;
using WorldPinball.Api.Validation.Public;

namespace WorldPinball.Tests.Validation.Public;

public sealed class SubmitScoreRequestValidatorTests
{
    private readonly SubmitScoreRequestValidator _validator = new();

    private static readonly SubmitScoreRequest ValidRequest = new(
        DeviceId: Guid.NewGuid(),
        Score: 150_000_000,
        SlotNumber: 1
    );

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(ValidRequest);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenDeviceIdIsEmpty_ShouldHaveError()
    {
        var request = ValidRequest with { DeviceId = Guid.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(SubmitScoreRequest.DeviceId));
    }

    [Fact]
    public void Validate_WhenScoreIsNegative_ShouldHaveError()
    {
        var request = ValidRequest with { Score = -1 };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(SubmitScoreRequest.Score));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WhenSlotNumberIsOutOfRange_ShouldHaveError(int invalidSlot)
    {
        var request = ValidRequest with { SlotNumber = invalidSlot };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(SubmitScoreRequest.SlotNumber));
    }
}
