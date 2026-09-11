

namespace WorldPinball.Domain.Enums;

public enum GameSessionState
{
    None = 0,
    Pending = 1,
    Started = 2,
    Paused = 3,
    Resumed = 4,
    Ended = 5,
    Cancelled = 6,
    Abandoned = 7,
    Aborted = 8,
    Created = 9
}
