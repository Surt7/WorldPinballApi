using FluentValidation;
using WorldPinball.Api.Contracts.Machine;

namespace WorldPinball.Api.Validation.Public;

public sealed class EnrollDeviceRequestValidator : AbstractValidator<EnrollDeviceRequest>
{
    public EnrollDeviceRequestValidator()
    {
        RuleFor(x => x.MachineId)
            .NotEmpty()
            .WithMessage("MachineId is required.");

        RuleFor(x => x.PlainEnrollmentToken)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Plain enrollment token is required.");

        RuleFor(x => x.PublicKeyPem)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Public key PEM is required.");

        RuleFor(x => x.DeviceSerialNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Device serial number is required.")
            .MaximumLength(64)
            .WithMessage("Device serial number must not exceed 64 characters.");
    }
}
