using Microsoft.AspNetCore.WebUtilities;
using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Spotify.Models;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NDiscoPlus.Shared.Models;

public class NDPData
{
    internal NDPData(
        SpotifyPlayerTrack track,
        NDPColorPalette referencePalette, NDPColorPalette effectPalette,
        EffectConfig effectConfig, ChunkedEffectsCollection effects,
        ImmutableArray<LightRecord> lights
    )
    {
        Track = track;
        ReferencePalette = referencePalette;
        EffectPalette = effectPalette;

        EffectConfig = effectConfig;
        Effects = effects;

        Lights = lights;
    }

    public SpotifyPlayerTrack Track { get; }

    public NDPColorPalette ReferencePalette { get; }
    public NDPColorPalette EffectPalette { get; }

    public EffectConfig EffectConfig { get; }
    public ChunkedEffectsCollection Effects { get; }

    public ImmutableArray<LightRecord> Lights { get; }
}