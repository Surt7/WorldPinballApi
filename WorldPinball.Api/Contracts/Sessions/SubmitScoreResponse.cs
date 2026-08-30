namespace WorldPinball.Api.Contracts.Sessions;

public sealed record SubmitScoreResponse(Guid ScoreEntryId, bool IsValidated);
