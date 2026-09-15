namespace WorldPinball.Domain.Entities;

public class GameMode
{
    private static class ErrorMessages
    {
        public const string InvalidCode = "Mode code cannot be empty.";
        public const string InvalidName = "Mode name cannot be empty.";
        public const string InvalidDate = "Date cannot be default.";
    }

    private GameMode() { }

    public Guid ModeId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    public GameMode(
        string code,
        string name,
        string? description = null,
        bool isActive = true,
        DateTime? createdAtUtc = null,
        Guid? modeId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException(ErrorMessages.InvalidCode, nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        ModeId = modeId ?? Guid.NewGuid();
        Code = code.Trim().ToLowerInvariant();
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.InvalidName, nameof(name));

        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAtUtc = now;
    }

    public void Activate(DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        IsActive = true;
        UpdatedAtUtc = now;
    }

    public void Deactivate(DateTime now)
    {
        if (now == default)
            throw new ArgumentException(ErrorMessages.InvalidDate, nameof(now));

        IsActive = false;
        UpdatedAtUtc = now;
    }
}
