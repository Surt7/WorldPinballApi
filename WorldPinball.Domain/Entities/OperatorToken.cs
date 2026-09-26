using System.Security.Cryptography;
using System.Text;

namespace WorldPinball.Domain.Entities;

public class OperatorToken
{
    private static class ErrorMessages
    {
        public const string TokenNameEmpty = "Token name cannot be empty.";
        public const string TokenTooShort = "Token must be at least 16 characters long.";
        public const string DateCannotBeDefault = "Date cannot be default.";
    }

    public Guid TokenId { get; private set; }
    public string Name { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public string Prefix { get; private set; } = null!;
    public string Scopes { get; private set; } = "*";
    public bool IsActive { get; private set; } = true;
    public string? IssuedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    // Clean enumerable exposing individual scopes for authorization
    public IEnumerable<string> ScopeList =>
        Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private OperatorToken() { } // EF Core

    public OperatorToken(
        string name,
        string rawToken,
        string scopes = "*",
        string? issuedByUserId = null,
        DateTime? expiresAtUtc = null,
        DateTime? nowUtc = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorMessages.TokenNameEmpty, nameof(name));

        if (string.IsNullOrWhiteSpace(rawToken) || rawToken.Length < 16)
            throw new ArgumentException(ErrorMessages.TokenTooShort, nameof(rawToken));

        var now = nowUtc ?? DateTime.UtcNow;

        TokenId = Guid.NewGuid();
        Name = name.Trim();
        TokenHash = HashToken(rawToken);
        Prefix = rawToken.Length >= 8 ? rawToken[..8] + "..." : rawToken;
        Scopes = string.IsNullOrWhiteSpace(scopes) ? "*" : scopes.Trim();
        IsActive = true;
        IssuedByUserId = issuedByUserId;
        CreatedAtUtc = now;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool IsValid(DateTime nowUtc)
    {
        if (!IsActive) return false;
        if (RevokedAtUtc.HasValue && RevokedAtUtc.Value <= nowUtc) return false;
        if (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= nowUtc) return false;
        return true;
    }

    public bool HasScope(string requiredScope)
    {
        if (string.IsNullOrWhiteSpace(requiredScope)) return true;
        if (Scopes == "*") return true;

        return ScopeList.Contains("*", StringComparer.OrdinalIgnoreCase) 
            || ScopeList.Contains(requiredScope, StringComparer.OrdinalIgnoreCase);
    }

    public void Revoke(DateTime nowUtc)
    {
        if (nowUtc == default)
            throw new ArgumentException(ErrorMessages.DateCannotBeDefault, nameof(nowUtc));

        IsActive = false;
        RevokedAtUtc = nowUtc;
    }
}