# Diagramme de Classes du Domaine (WorldPinball)

```mermaid
classDiagram
    class GameSession {
        +Guid SessionId
        +Guid MachineId
        +Guid DeviceId
        +Guid ModeId
        +DateTime StartedAtUtc
        +DateTime EndedAtUtc
        +GameSessionState State
        +string Nonce
        +string ClientVersion
        +Start(startedAtUtc)
        +End(endedAtUtc)
        +AddPlayer(slotNumber, userId, guestInitials)
        +RemovePlayer(slotNumber)
    }

    class GameSessionPlayer {
        +Guid SessionId
        +int SlotNumber
        +Guid? UserId
        +string? GuestInitials
        +DateTime JoinedAtUtc
    }

    class ScoreEntry {
        +Guid ScoreEntryId
        +Guid SessionId
        +int SlotNumber
        +Guid DeviceId
        +Guid MachineId
        +Guid IpId
        +Guid ModeId
        +long Score
        +DateTime AchievedAtUtc
        +string? UserId
        +bool IsValidated
    }

    class Machine {
        +Guid MachineId
        +Guid IpId
        +string WorldPinballSerialNumber
    }

    class Device {
        +Guid DeviceId
        +string SerialPhysical
        +string Status
        +string PublicKey
    }

    class GameMode {
        +Guid ModeId
        +string Code
        +string Name
    }

    class IpLicense {
        +Guid IpId
        +string Name
    }

    GameSession "1" *-- "0..*" GameSessionPlayer : contains
    GameSession "1" *-- "0..*" ScoreEntry : records
    GameSession --> Machine : played on
    GameSession --> Device : authenticated by
    GameSession --> GameMode : rules
    Machine --> IpLicense : belongs to
    ScoreEntry --> Machine
    ScoreEntry --> Device
    ScoreEntry --> IpLicense
    ScoreEntry --> GameMode
```
