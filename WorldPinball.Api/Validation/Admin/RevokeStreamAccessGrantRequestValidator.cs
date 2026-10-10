using FluentValidation;
using WorldPinball.Api.Contracts.StreamingAccess;

namespace WorldPinball.Api.Validation.Admin;

public sealed class RevokeStreamAccessGrantRequestValidator : AbstractValidator<RevokeStreamAccessGrantRequest>
{
    public RevokeStreamAccessGrantRequestValidator()
    {
        RuleFor(x => x.Reason)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Revocation reason is required.")
            .MaximumLength(500)
            .WithMessage("Revocation reason cannot exceed 500 characters.");
    }
}
