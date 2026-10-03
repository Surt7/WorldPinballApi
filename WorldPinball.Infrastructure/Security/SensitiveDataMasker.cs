namespace WorldPinball.Infrastructure.Security;

    
public class SensitiveDataMasker
{
    
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "authorization", "apikey", "secret", "token"
    };
    
    
    
    public string? Mask(string key, string? value)
    {
        if (SensitiveKeys.Contains(key))
        {
            return "[REDACTED]";
        }
        
        return value;
    }
    
    
    
}
