using NDiscoPlus.Code.LightHandlers;
using NDiscoPlus.Code.LightHandlers.Screen;
using NDiscoPlus.Code.Models;
using NDiscoPlus.Shared;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.HomePagePlayer;

// MODELS for NDPPlayer
internal partial class NDPPlayer
{
    // Use struct so that when we lock the object, there are no other references to it that could cause a teared read
    public readonly record struct SharedData(
        SpotifyPlayerContext? PlayerState,
        NDPColorPalette? CurrentTrackPalette,
        FourColorGradient? CurrentTrackPaletteGradient, // null if less than 4 colors available
        ImmutableArray<BaseScreenLightHandler>? ScreenLightHandlers,
        ChunkedEffectsCollection.ExportedData? CurrentTrackEffectsExported
    );

    public readonly struct LightData
    {
        public required ImmutableArray<LightHandler> Handlers { get; init; }
        public required ImmutableArray<LightRecord> Lights { get; init; }
    }
}
