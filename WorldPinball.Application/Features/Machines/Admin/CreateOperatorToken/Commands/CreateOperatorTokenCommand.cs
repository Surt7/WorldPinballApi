namespace WorldPinball.Application.Features.Machines.Admin.CreateOperatorToken.Commands;

public sealed record CreateOperatorTokenCommand(
    string Name,
    string Scopes,
    int? ValidityDays,
    string IssuedByUserId);