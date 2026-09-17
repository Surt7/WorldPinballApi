namespace WorldPinball.Domain.Enums;

public enum GameSessionState
{
    // 1. Initialization / Lobby / Matchmaking
    None = 0,
    Created = 1,
    Pending = 2,
    SearchingPlayer = 3,
    MatchFound = 4,
    Joined = 5,

    // 2. Active / In-Game
    Started = 6,
    InGame = 7,
    Paused = 8,
    Resumed = 9,

    // 3. Termination / Closure
    Ended = 10,
    Cancelled = 11,
    Abandoned = 12,
    Aborted = 13
}
