using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Models;

[JsonDerivedType(typeof(ScreenLightId), "screen")]
[JsonDerivedType(typeof(HueLightId), "hue")]
public abstract class LightId
{
    [JsonIgnore]
    public abstract string HumanReadableString { get; }

    public abstract override bool Equals(object? obj);
    public abstract override int GetHashCode();

    public static bool operator ==(LightId? a, object? b)
        => a is not null ? a.Equals(b) : b == null;
    public static bool operator !=(LightId? a, object? b)
        => !(a == b);
}