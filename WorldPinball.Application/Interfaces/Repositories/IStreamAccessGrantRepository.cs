using WorldPinball.Domain.Entities;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Interfaces.Repositories;

public interface IStreamAccessGrantRepository
{
    Task<IReadOnlyList<StreamAccessGrant>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StreamAccessGrant?> GetByIdAsync(Guid grantId, CancellationToken cancellationToken = default);

    Task<StreamAccessGrant?> FindActiveGrantAsync(
        PrincipalType principalType,
        Guid principalId,
        CancellationToken cancellationToken = default);

    Task AddAsync(StreamAccessGrant grant, CancellationToken cancellationToken = default);

    Task UpdateAsync(StreamAccessGrant grant, CancellationToken cancellationToken = default);
}