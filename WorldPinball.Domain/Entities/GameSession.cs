using WorldPinball.Domain.Common;
using WorldPinball.Domain.Enums;
using WorldPinball.Domain.Events;

namespace WorldPinball.Domain.Entities;

public class GameSession : Entity
{
    private static class ErrorMessages
    {
        public const string MachineId = "MachineId cannot be empty.";
        public const string DeviceId = "DeviceId cannot be empty";
        public const string ModeId = "ModeId cannot be empty.";
        public const string Nonce = "Nonce cannot be empty.";
        public const string StartedAtUtc = "StartedAtUtc must be a valid date.";
        public const string ClientVersion = "ClientVersion cannot be empty.";
        public const string EndSession = "Cannot end a session that is not in Started state.";
        public const string Earlier = "EndedAtUtc cannot be earlier than StartedAtUtc.";
        public const string PauseSession = "Can only pause an active session.";
        public const string ResumeSession = "Can only resume a paused session.";
        public const string ClosedSession = "Session is already closed.";
        public const string AddPlayerNotStarted = "Cannot add a player to a session that is not in a joinable state.";
        public const string InvalidSlotNumber = "Slot number must be between 1 and 4.";
        public const string MaxPlayersReached = "A game session cannot have more than 4 players.";
        public const string SlotAlreadyOccupied = "The specified slot is already occupied.";
        public const string UserAlreadyRegistered = "This user is already registered in this session.";
        public const string RegisteredUserRequired = "A registered player (UserId) is required for this game mode or tournament.";
        public const string CannotSearchPlayers = "Can only search for players from a created, pending, or match-found session.";
        public const string MatchFoundNotSearching = "Cannot mark match as found unless currently searching for players.";
        public const string NotEnoughPlayersForMatch = "Cannot mark match as found without enough players.";
        public const string CannotRemovePlayerActiveSession = "Cannot remove a player once the session has started or ended.";
        public const string PlayerNotFound = "Player not found in the specified slot.";
        public const string CannotStartSession = "Can only start a session that is in Created or MatchFound state.";
        public const string CannotCancelSession = "Cannot cancel a session that is already active or closed.";
    }
    private GameSession() { }


    private readonly List<ScoreEntry> _scoreEntries = [];
    public IReadOnlyCollection<ScoreEntry>  ScoreEntries => _scoreEntries.AsReadOnly();
    
    public Guid SessionId { get; private set; }

    public Guid MachineId { get; private set; } 
    public Machine Machine { get; private set; } = null!;

    public Guid DeviceId { get; private set; }
    public Device Device { get; private set; } =  null!;

    public Guid ModeId { get; private set; }
    public GameMode Mode { get; private set; } = null!;

    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }

    public GameSessionState State { get; private set; }
    public string Nonce { get; private set; }
    public string ClientVersion { get; private set; }

    public ScoreEntry? ScoreEntry { get; private set; }

    private readonly List<GameSessionPlayer> _players = [];
    public IReadOnlyCollection<GameSessionPlayer> Players => _players.AsReadOnly();
    
    public GameSession(
        Guid machineId,
        Guid deviceId,
        Guid modeId,
        string clientVersion,
        string nonce,
        DateTime startedAtUtc)
    {
        if (machineId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.MachineId, nameof(machineId));
        
        if (deviceId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.DeviceId, nameof(deviceId));
        
        if (modeId == Guid.Empty)
            throw new ArgumentException(ErrorMessages.ModeId, nameof(modeId));
        
        if (string.IsNullOrWhiteSpace(clientVersion))
            throw new ArgumentException(ErrorMessages.ClientVersion, nameof(clientVersion));
        
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException(ErrorMessages.Nonce, nameof(nonce));
        
        if (startedAtUtc == default)
            throw new ArgumentException(ErrorMessages.StartedAtUtc, nameof(startedAtUtc));
        
        SessionId = Guid.NewGuid();
        MachineId = machineId;
        DeviceId = deviceId;
        ModeId = modeId;
        ClientVersion = clientVersion;
        Nonce = nonce;
        // StartedAtUtc is initially set to the creation time, can be updated on Start
        StartedAtUtc = startedAtUtc;
        State = GameSessionState.Created;
    }

    public void Start(DateTime startedAtUtc)
    {
        if (State is not (GameSessionState.Created or GameSessionState.MatchFound))
            throw new InvalidOperationException(ErrorMessages.CannotCancelSession);

        StartedAtUtc = startedAtUtc;
        State = GameSessionState.Started;

        AddDomainEvent(new GameSessionStartedDomainEvent(
            SessionId,
            MachineId,
            DeviceId,
            ModeId,
            StartedAtUtc));
    }

    public void SearchPlayers()
    {
        if (State is not (GameSessionState.Created or GameSessionState.Pending or GameSessionState.MatchFound))
            throw new InvalidOperationException(ErrorMessages.CannotSearchPlayers);

        State = GameSessionState.SearchingPlayer;
    }
    
    public void Pause()
    {
        if (State is not (GameSessionState.Started or GameSessionState.Resumed))
            throw new InvalidOperationException(ErrorMessages.PauseSession);

        State = GameSessionState.Paused;
    }

    public void Resume()
    {
        if (State is not GameSessionState.Paused)
            throw new InvalidOperationException(ErrorMessages.ResumeSession);

        State = GameSessionState.Resumed;
    }
    
    public void SetMatchFound()
    {
        if (State is not GameSessionState.SearchingPlayer)
            throw new InvalidOperationException(ErrorMessages.MatchFoundNotSearching);
        if (_players.Count < 2)
            throw new InvalidOperationException(ErrorMessages.NotEnoughPlayersForMatch);

        State = GameSessionState.MatchFound;
    }

    public void RemovePlayer(int slotNumber)
    {
        if (State is not (GameSessionState.Created 
            or GameSessionState.Pending 
            or GameSessionState.SearchingPlayer 
            or GameSessionState.MatchFound))
        {
            throw new InvalidOperationException(ErrorMessages.CannotRemovePlayerActiveSession);
        }

        var player = _players.FirstOrDefault(p => p.SlotNumber == slotNumber);
        if (player is null)
            throw new InvalidOperationException(ErrorMessages.PlayerNotFound);
                
        _players.Remove(player);
        if (State is GameSessionState.MatchFound)
        {
            State = GameSessionState.Pending; 
        }
    }
    
    public void End(DateTime endedAtUtc)
    {
        if (State is not (GameSessionState.Started or GameSessionState.Resumed or GameSessionState.Paused))
            throw new InvalidOperationException(ErrorMessages.EndSession);
        if (endedAtUtc < StartedAtUtc)
            throw new ArgumentException(ErrorMessages.Earlier, nameof(endedAtUtc));
        EndedAtUtc = endedAtUtc;
        State = GameSessionState.Ended;

        AddDomainEvent(new GameSessionEndedDomainEvent(SessionId, endedAtUtc));
    }

    public void Abandon(DateTime abandonedAtUtc)
    {
        if (State is GameSessionState.Ended or GameSessionState.Abandoned or GameSessionState.Aborted or GameSessionState.Cancelled)
            throw new InvalidOperationException(ErrorMessages.ClosedSession);
        if (abandonedAtUtc < StartedAtUtc)
            throw new ArgumentException(ErrorMessages.Earlier, nameof(abandonedAtUtc));
        EndedAtUtc = abandonedAtUtc;
        State = GameSessionState.Abandoned;
    }

    public void Abort(DateTime abortedAtUtc)
    {
        if (State is GameSessionState.Ended or GameSessionState.Abandoned or GameSessionState.Aborted or GameSessionState.Cancelled)
            throw new InvalidOperationException(ErrorMessages.ClosedSession);
        if (abortedAtUtc < StartedAtUtc)
            throw new ArgumentException(ErrorMessages.Earlier, nameof(abortedAtUtc));
        EndedAtUtc = abortedAtUtc;
        State = GameSessionState.Aborted;
    }

    public void AddPlayer(
        int slotNumber,
        Guid? userId = null,
        string? guestInitials = null,
        bool requiresRegisteredUser = false)
    {
        if (requiresRegisteredUser && !userId.HasValue)
            throw new InvalidOperationException(ErrorMessages.RegisteredUserRequired);
        
        if (State is not (GameSessionState.Created or GameSessionState.Pending or GameSessionState.SearchingPlayer or GameSessionState.Started or GameSessionState.Resumed))
            throw new InvalidOperationException(ErrorMessages.AddPlayerNotStarted);

        if (slotNumber is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(slotNumber), ErrorMessages.InvalidSlotNumber);

        if (_players.Count >= 4)
            throw new InvalidOperationException(ErrorMessages.MaxPlayersReached);

        if (_players.Any(p => p.SlotNumber == slotNumber))
            throw new InvalidOperationException(ErrorMessages.SlotAlreadyOccupied);

        if (userId.HasValue && _players.Any(p => p.UserId == userId.Value))
            throw new InvalidOperationException(ErrorMessages.UserAlreadyRegistered);

        _players.Add(new GameSessionPlayer
        {
            SessionId = SessionId,
            SlotNumber = slotNumber,
            UserId = userId,
            GuestInitials = guestInitials,
            JoinedAtUtc = DateTime.UtcNow
        });
    }
}
