using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Security;

public interface IOperatorTokenValidator
{
    Task<OperatorToken?> ValidateAsync(string rawToken, CancellationToken cancellationToken = default);
}
