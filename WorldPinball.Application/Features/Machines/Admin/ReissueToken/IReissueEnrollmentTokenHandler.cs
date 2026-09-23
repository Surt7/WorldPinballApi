using WorldPinball.Application.Features.Machines.Admin.ReissueToken.Commands;

namespace WorldPinball.Application.Features.Machines.Admin.ReissueToken;

public interface IReissueEnrollmentTokenHandler
{
    Task<ReissueEnrollmentTokenResult> HandleAsync(
        ReissueEnrollmentTokenCommand cmd,
        CancellationToken ct = default);
}
