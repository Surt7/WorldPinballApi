using FluentAssertions;
using WorldPinball.Api.Validation.Admin;
using WorldPinball.Application.Contracts.Admin.Machine;

namespace WorldPinball.Tests.Validation.Admin;

public sealed class ProvisionMachineRequestValidatorTests
{
    private readonly ProvisionMachineRequestValidator _validator = new();

    private static readonly ProvisionMachineRequest ValidRequest = new(
        "WP-K7M4-9X2P-R8HD",
        "resident-evil",
        "Resident Evil",
        null);

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
    public void Validate_WhenSerialNumberIsEmpty_ShouldHaveSerialNumberError(
        string? invalidSerialNumber)
    {
        var request = ValidRequest with
        {
            WorldPinballSerialNumber = invalidSerialNumber!
        };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.WorldPinballSerialNumber));
    }

    [Fact]
    public void Validate_WhenSerialNumberExceeds32Characters_ShouldHaveSerialNumberError()
    {
        var request = ValidRequest with
        {
            WorldPinballSerialNumber = new string('A', 33)
        };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.WorldPinballSerialNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenIpSlugIsEmpty_ShouldHaveIpSlugError(string? invalidIpSlug)
    {
        var request = ValidRequest with { IpSlug = invalidIpSlug! };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.IpSlug));
    }

    [Theory]
    [InlineData("Resident-Evil")]
    [InlineData("resident evil")]
    [InlineData("resident--evil")]
    [InlineData("-resident-evil")]
    [InlineData("resident-evil-")]
    public void Validate_WhenIpSlugHasInvalidFormat_ShouldHaveIpSlugError(string invalidIpSlug)
    {
        var request = ValidRequest with { IpSlug = invalidIpSlug };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.IpSlug));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDisplayNameIsEmpty_ShouldHaveDisplayNameError(
        string? invalidDisplayName)
    {
        var request = ValidRequest with { DisplayName = invalidDisplayName! };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.DisplayName));
    }

    [Fact]
    public void Validate_WhenDisplayNameExceeds150Characters_ShouldHaveDisplayNameError()
    {
        var request = ValidRequest with { DisplayName = new string('A', 151) };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.DisplayName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPublicKeyIsEmpty_ShouldHavePublicKeyError(string invalidPublicKey)
    {
        var request = ValidRequest with { PublicKey = invalidPublicKey };

        var result = _validator.Validate(request);

        result.Errors.Should().ContainSingle(error =>
            error.PropertyName == nameof(ProvisionMachineRequest.PublicKey));
    }

    [Fact]
    public void Validate_WhenPublicKeyIsNull_ShouldHaveNoErrors()
    {
        var request = ValidRequest with { PublicKey = null };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
