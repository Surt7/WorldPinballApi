// WorldPinball.Infrastructure/Security/CanonicalJson.cs

using System.Text.Json;
using System.Text.Json.Nodes;

namespace WorldPinball.Infrastructure.Security;

public static class CanonicalJson
{
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
                "Le texte fourni n'est pas un JSON valide.",
                nameof(rawJson),
                ex);
        }

        if (node is null)
        {
            // Le texte "null" est un JSON valide.
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