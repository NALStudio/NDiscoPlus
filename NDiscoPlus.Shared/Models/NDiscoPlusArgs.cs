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
    public required SpotifyPlayerTrack Track { get; init; }

    public required TrackAudioFeatures Features { get; init; }
    public required TrackAudioAnalysis Analysis { get; init; }

    public required EffectConfig Effects { get; init; }
    public required ImmutableArray<LightRecord> Lights { get; init; }

    public bool AllowHDR { get; init; } = true;
    /// <summary>
    /// <para>If <see langword="null"/>, use a random default color palette.</para>
    /// </summary>
    public NDPColorPalette? ReferencePalette { get; init; } = null;
}