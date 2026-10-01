using WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant.Commands;

namespace WorldPinball.Application.Features.StreamingAccess.Admin.CreateStreamAccessGrant;

public interface ICreateStreamAccessGrantHandler
{
    Task<CreateStreamAccessGrantResult> HandleAsync(
        CreateStreamAccessGrantCommand command,
        CancellationToken cancellationToken = default);
}