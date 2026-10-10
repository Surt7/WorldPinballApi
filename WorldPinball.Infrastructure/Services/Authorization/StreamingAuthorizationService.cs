using WorldPinball.Application.Interfaces.Repositories;
using WorldPinball.Application.Interfaces.Services;
using WorldPinball.Domain.Enums;

namespace WorldPinball.Infrastructure.Services.Authorization;


/* Authorization is not about policies
 or related to any middleware security
 it's about Authorization in domain meaning
*/
public sealed class StreamingAuthorizationService(
    IStreamingPartnerRepository partnerRepository,
    IStreamingAccessRequestRepository requestRepository,
    TimeProvider timeProvider
) : IStreamingAuthorizationService
{
    public async Task<bool> CanStreamAsync(Guid userId, Guid sessionId, CancellationToken token = default)
    {
        // 1. Fast-Path: Check if user is an active partner on the whitelist
        if (await partnerRepository.IsActivePartnerAsync(userId, token))
            return true;

        // 2. Request-Path: Check if session has a valid approved lease
        var request = await requestRepository.GetLatestRequestAsync(sessionId, token);
        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;

        return request is not null
               && request.Status is StreamingRequestStatus.Approved
               && request.ExpiresAtUtc > nowUtc;
    }
}