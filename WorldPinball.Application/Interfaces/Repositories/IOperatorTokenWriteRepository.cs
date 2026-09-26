using WorldPinball.Domain.Entities;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IOperatorTokenWriteRepository
{
    Task AddAsync(OperatorToken token, CancellationToken ct = default);
}