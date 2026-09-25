namespace WorldPinball.Api.Contracts.Sessions;

public sealed record SubmitScoreResponse(IReadOnlyList<Guid> ScoreEntryIds);
