using FluentValidation;
using WorldPinball.Application.Contracts.Admin.Machine;

namespace WorldPinball.Api.Validation.Admin;

public sealed class ProvisionMachineRequestValidator
    : AbstractValidator<ProvisionMachineRequest>
{
    public ProvisionMachineRequestValidator()
    {
        RuleFor(request => request.WorldPinballSerialNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The WorldPinball serial number is required.")
            .MaximumLength(32)
            .WithMessage("The WorldPinball serial number must not exceed 32 characters.");

        RuleFor(request => request.IpSlug)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The IP slug is required.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("The IP slug must contain only lowercase letters, numbers, and single hyphens.");

        RuleFor(request => request.DisplayName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The display name is required.")
            .MaximumLength(150)
            .WithMessage("The display name must not exceed 150 characters.");

        RuleFor(request => request.PublicKey)
            .NotEmpty()
            .When(request => request.PublicKey is not null)
            .WithMessage("The public key cannot be empty when provided.");
    }
}
