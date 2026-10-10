using WorldPinball.Application.Features.StreamingAccess.Public.Commands;

namespace WorldPinball.Application.Features.StreamingAccess.Public;

public interface IRequestStreamingAccessHandler
{
    Task<RequestStreamingAccessResult> HandleAsync(RequestStreamingAccessCommand command, CancellationToken token = default);
}