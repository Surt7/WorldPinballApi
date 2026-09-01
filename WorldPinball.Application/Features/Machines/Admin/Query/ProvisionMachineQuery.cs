using WorldPinball.Domain.Enums;

namespace WorldPinball.Application.Features.Machines.Admin.Query;

public sealed record ProvisionMachineQuery(
    string SerialNumber);
