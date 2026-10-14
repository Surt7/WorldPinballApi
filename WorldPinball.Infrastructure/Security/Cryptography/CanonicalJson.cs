// WorldPinball.Infrastructure/Security/CanonicalJson.cs

using System.Text.Json;
using System.Text.Json.Nodes;

namespace WorldPinball.Infrastructure.Security.Cryptography;

/// <summary>
/// Implements deterministic JSON canonicalization (aligned with RFC 8785 - JSON Canonicalization Scheme).
/// Cryptographic signatures (ECDSA/SHA-256) are byte-sensitive: differences in whitespace, line endings,
/// or dictionary property ordering between client and server would produce completely different hashes.
/// Recursively sorts all JSON object keys using <see cref="StringComparer.Ordinal"/> (byte-level ordering, avoiding culture issues)
/// to produce a reproducible canonical byte representation for signature verification.
/// </summary>
public static class CanonicalJson
{
    /// <summary>
    /// Parses a raw JSON payload, recursively sorts object properties deterministically,
    /// and formats a normalized JSON string ready for cryptographic hashing.
    /// </summary>
    public static string Canonicalize(string rawJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(rawJson);

        JsonNode? node;

        try
        {
            node = JsonNode.Parse(rawJson);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "The provided text is invalid JSON.",
                nameof(rawJson),
                ex);
        }

        if (node is null)
        {
          // "null" is a valid json technically
            return "null";
        }

        var sorted = Sort(node);


        return sorted.ToJsonString();

        static JsonNode Sort(JsonNode token) => token switch
        {
            JsonObject obj => new JsonObject(
                    obj
                .OrderBy(property => property.Key, StringComparer.Ordinal)
                .Select(property =>
                    new KeyValuePair<string, JsonNode?>(
                        property.Key, property.Value is null
                        ? null
                        : Sort(property.Value)))
            ),
            JsonArray a => new JsonArray(
                a.Select(item =>
                        item is null ?
                            null
                            : Sort(item))
                    .ToArray()

            ),
            _ => token.DeepClone()
        };
    }
}