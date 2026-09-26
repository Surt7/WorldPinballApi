

using WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken.Commands;

namespace WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken;

public interface ICreateOperatorTokenHandler
{
    Task<CreateOperatorTokenResult> HandleAsync(
        CreateOperatorTokenCommand command,
        CancellationToken ct = default);
}