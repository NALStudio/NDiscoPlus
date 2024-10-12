using NDiscoPlus.Code.LightHandlers;
using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.Models.LightProfile;
public sealed class SerializableLightProfile
{
    // LightId cannot be serialized as a key, so we make a wrapper struct for both values instead.
    public readonly record struct LightConfigRecord(LightId Light, LightConfig Config);

    public string Name { get; }
    public ImmutableArray<LightHandlerConfig> Handlers { get; }

    // Use ImmutableList instead of ImmutableArray so that JsonSerializer can handle null correctly
    // Use JsonPropertyName so that the old dictionary value of LightConfigurationOverrides isn't loaded
    [JsonPropertyName("LightConfigurationOverrides_V2")]
    public ImmutableList<LightConfigRecord> LightConfigurationOverrides { get; }

    [JsonConstructor]
    private SerializableLightProfile(string name, ImmutableArray<LightHandlerConfig> handlers, ImmutableList<LightConfigRecord>? lightConfigurationOverrides)
    {
        Name = name;
        Handlers = handlers;
        LightConfigurationOverrides = lightConfigurationOverrides ?? ImmutableList<LightConfigRecord>.Empty;
    }

    public static SerializableLightProfile Construct(LightProfile profile)
    {
        ImmutableArray<LightHandlerConfig> handlers = profile.Handlers.Select(h => h.Config).ToImmutableArray();

        return new(
            profile.Name,
            handlers: handlers,
            lightConfigurationOverrides: profile.LightConfigurationOverrides.Select(static x => new LightConfigRecord(x.Key, x.Value)).ToImmutableList()
        );
    }

    public static LightProfile Deconstruct(SerializableLightProfile profile)
    {
        return new LightProfile(
            name: profile.Name,
            handlers: profile.Handlers.Select(h => h.CreateLightHandler()),
            lightConfigurationOverrides: profile.LightConfigurationOverrides.Select(static x => new KeyValuePair<LightId, LightConfig>(x.Light, x.Config))
        );
    }
}