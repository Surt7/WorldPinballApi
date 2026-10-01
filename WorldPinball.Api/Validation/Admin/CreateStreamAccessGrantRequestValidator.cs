using FluentValidation;
using WorldPinball.Api.Contracts.StreamingAccess;

namespace WorldPinball.Api.Validation.Admin;

public sealed class CreateStreamAccessGrantRequestValidator : AbstractValidator<CreateStreamAccessGrantRequest>
{
    public CreateStreamAccessGrantRequestValidator()
    {
        RuleFor(x => x.PrincipalId)
            .NotEmpty()
            .WithMessage("PrincipalId is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required.")
            .MaximumLength(200)
            .WithMessage("DisplayName cannot exceed 200 characters.");

        RuleFor(x => x.PrincipalType)
            .IsInEnum()
            .WithMessage("A valid PrincipalType is required.");

        RuleFor(x => x.Origin)
            .IsInEnum()
            .WithMessage("A valid GrantOrigin is required.");
    }
}
