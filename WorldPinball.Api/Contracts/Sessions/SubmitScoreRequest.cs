namespace WorldPinball.Api.Contracts.Sessions;

public sealed record SubmitScoreRequest(int Score, long? EndedAt);
