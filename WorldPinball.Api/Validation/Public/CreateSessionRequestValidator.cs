using FluentValidation;
using WorldPinball.Api.Contracts.Sessions;

namespace WorldPinball.Api.Validation.Public;

public class CreateGameSessionRequestValidator<T> : AbstractValidator<T> where T : CreateGameSessionRequest
{
    public CreateGameSessionRequestValidator()
    {
        RuleFor(x => x.ModeCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Please specify a mode code")
            .MaximumLength(32)
            .WithMessage("The mode code must not exceed 32 characters");

        RuleFor(x => x.ClientVersion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The client version is required.")
            .Matches(@"^[0-9]+\.[0-9]+\.[0-9]+$")
            .WithMessage("The client version must follow semantic versioning format (e.g. 1.0.0).");
    }
}

public sealed class CreateGameSessionRequestValidator : CreateGameSessionRequestValidator<CreateGameSessionRequest>;

public sealed class CreateSessionRequestValidator : CreateGameSessionRequestValidator<CreateGameSessionRequest>;