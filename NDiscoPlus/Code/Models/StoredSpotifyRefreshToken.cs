using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Code.Models;

internal class StoredSpotifyRefreshToken
{
    public string RefreshToken { get; }
    public ImmutableHashSet<string> Scope { get; }

    [JsonConstructor]
    public StoredSpotifyRefreshToken(string refreshToken, ImmutableHashSet<string> scope)
    {
        RefreshToken = refreshToken;
        Scope = scope;
    }

    public StoredSpotifyRefreshToken(string refreshToken, ICollection<string> scope)
    {
        RefreshToken = refreshToken;
        Scope = scope.ToImmutableHashSet();
    }

    public static string Serialize(StoredSpotifyRefreshToken refreshToken)
    {
        return JsonSerializer.Serialize(refreshToken);
    }

    public static bool TryDeserialize(string json, [MaybeNullWhen(false)] out StoredSpotifyRefreshToken refreshToken)
    {
        refreshToken = JsonSerializer.Deserialize<StoredSpotifyRefreshToken>(json);
        return refreshToken is not null;
    }

    public static StoredSpotifyRefreshToken Deserialize(string json)
    {
        if (TryDeserialize(json, out StoredSpotifyRefreshToken? rt))
            return rt;
        else
            throw new ArgumentException("Cannot deserialize json.");
    }

    public static StoredSpotifyRefreshToken? DeserializeOrNull(string? json)
    {
        if (json is null)
            return null;

        if (!TryDeserialize(json, out StoredSpotifyRefreshToken? rt))
            return null;

        return rt;
    }
}
