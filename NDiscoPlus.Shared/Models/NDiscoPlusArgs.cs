using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;

public class NDiscoPlusArgs
{
    public NDiscoPlusArgs(SpotifyPlayerTrack track, TrackAudioFeatures features, TrackAudioAnalysis analysis, EffectConfig effects, ImmutableArray<LightRecord> lights)
    {
        Track = track;

        Features = features;
        Analysis = analysis;

        Effects = effects;
        Lights = lights;
    }

    public SpotifyPlayerTrack Track { get; }

    public TrackAudioFeatures Features { get; }
    public TrackAudioAnalysis Analysis { get; }

    public EffectConfig Effects { get; }
    public ImmutableArray<LightRecord> Lights { get; }

    public bool AllowHDR { get; init; } = true;
    /// <summary>
    /// <para>If <see langword="null"/>, use a random default color palette.</para>
    /// </summary>
    public NDPColorPalette? ReferencePalette { get; init; } = null;
}