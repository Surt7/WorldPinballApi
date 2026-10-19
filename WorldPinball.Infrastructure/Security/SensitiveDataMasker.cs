namespace WorldPinball.Infrastructure.Security;

    
public static class SensitiveDataMasker
{
    
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "authorization", "apikey", "secret", "token"
    };
    
    
    
    public static string? Mask(string key, string? value)
    {
        return SensitiveKeys.Contains(key) ? "[REDACTED]" : value;
    }
    
    
    
}
