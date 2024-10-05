using NDiscoPlus.Code.LightHandlers;
using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static NDiscoPlus.Code.Models.LightProfile.LightProfile;

namespace NDiscoPlus.Code.Models.LightProfile;
public class SerializableLightProfile
{
    public string Name { get; }
    public ImmutableArray<LightHandlerConfig> Handlers { get; }
    public ImmutableDictionary<LightId, LightConfig> LightConfigurationOverrides { get; }

    [JsonConstructor]
    private SerializableLightProfile(string name, ImmutableArray<LightHandlerConfig> handlers, ImmutableDictionary<LightId, LightConfig>? lightConfigurationOverrides)
    {
        Name = name;
        Handlers = handlers;
        LightConfigurationOverrides = lightConfigurationOverrides ?? ImmutableDictionary<LightId, LightConfig>.Empty; // empty dictionary for backwards compat
    }

    public static SerializableLightProfile Construct(LightProfile profile)
    {
        ImmutableArray<LightHandlerConfig> handlers = profile.Handlers.Select(h => h.Config).ToImmutableArray();

        return new(
            profile.Name,
            handlers: handlers,
            lightConfigurationOverrides: profile.LightConfigurationOverrides.ToImmutableDictionary()
        );
    }

    public static LightProfile Deconstruct(SerializableLightProfile profile)
    {
        return new LightProfile(
            name: profile.Name,
            handlers: profile.Handlers.Select(h => h.CreateLightHandler()),
            lightConfigurationOverrides: profile.LightConfigurationOverrides
        );
    }
}