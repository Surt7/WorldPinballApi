using FluentAssertions;
using WorldPinball.Api.Contracts.Machine;
using WorldPinball.Api.Validation.Public;

namespace WorldPinball.Tests.Validation.Public;

public sealed class EnrollDeviceRequestValidatorTests
{
    private readonly EnrollDeviceRequestValidator _validator = new();

    private static readonly EnrollDeviceRequest ValidRequest = new(
        MachineId: Guid.NewGuid(),
        PlainEnrollmentToken: "ENROLL-SECRET-TOKEN-12345",
        PublicKeyPem: "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A\n-----END PUBLIC KEY-----",
        DeviceSerialNumber: "DEV-SN-001"
    );

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(ValidRequest);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenMachineIdIsEmpty_ShouldHaveError()
    {
        var request = ValidRequest with { MachineId = Guid.Empty };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(EnrollDeviceRequest.MachineId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPlainEnrollmentTokenIsEmpty_ShouldHaveError(string? invalidToken)
    {
        var request = ValidRequest with { PlainEnrollmentToken = invalidToken! };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(EnrollDeviceRequest.PlainEnrollmentToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenPublicKeyPemIsEmpty_ShouldHaveError(string? invalidKey)
    {
        var request = ValidRequest with { PublicKeyPem = invalidKey! };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(EnrollDeviceRequest.PublicKeyPem));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WhenDeviceSerialNumberIsEmpty_ShouldHaveError(string? invalidSerial)
    {
        var request = ValidRequest with { DeviceSerialNumber = invalidSerial! };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(EnrollDeviceRequest.DeviceSerialNumber));
    }

    [Fact]
    public void Validate_WhenDeviceSerialNumberExceeds64Characters_ShouldHaveError()
    {
        var request = ValidRequest with { DeviceSerialNumber = new string('X', 65) };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(EnrollDeviceRequest.DeviceSerialNumber));
    }
}
