namespace WorldPinball.Application.Interfaces.Repositories;

public interface IStreamingPartnerRepository
{
    Task<bool> IsActivePartnerAsync(Guid userId, CancellationToken token = default);
}