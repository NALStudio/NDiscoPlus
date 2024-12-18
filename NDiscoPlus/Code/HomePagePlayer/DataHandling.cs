﻿using NDiscoPlus.Code.Models;
using NDiscoPlus.Shared;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code.HomePagePlayer;
internal partial class NDPPlayer
{
    private class DataRecord
    {
        public readonly record struct ColorPaletteData(NDPColorPalette? Palette, FourColorGradient? Gradient);

        // SoitufyClient is hopefully thread-safe...
        private readonly SpotifyClient spotify;

        public DataRecord(SpotifyClient spotify)
        {
            this.spotify = spotify;
        }

        public SpotifyPlayerTrack? Track { get; private set; }

        private Task<ColorPaletteData>? palette;
        private Task<NDPData>? data;

        public void Reset(SpotifyPlayerTrack? newTrack)
        {
            Track = newTrack;

            palette = null;
            data = null;
        }

        public void CopyFrom(DataRecord data)
        {
            Track = data.Track;
            palette = data.palette;
            this.data = data.data;
        }

        private static async Task<ColorPaletteData> GetPalette(SpotifyPlayerTrack track)
        {
            NDPColorPalette? palette = await new NDiscoPlusService().FetchImagePalette(track);

            FourColorGradient? gradient;
            if (palette is NDPColorPalette p)
                gradient = FourColorGradient.TryCreateFromPalette(p);
            else
                gradient = null;

            return new ColorPaletteData(palette, gradient);
        }

        // Use ImmutableArray instead of IEnumerable since this function is threaded and we don't want to have any thread-safety issues 
        private async Task<NDPData> GetData(SpotifyClient spotify, SpotifyPlayerTrack track, ImmutableArray<LightRecord> lights)
        {
            if (this.palette is null)
                RequestPalette();
            ColorPaletteData palette = await this.palette!;

            TrackAudioFeatures features = await spotify.Tracks.GetAudioFeatures(track.Id);
            TrackAudioAnalysis analysis = await spotify.Tracks.GetAudioAnalysis(track.Id);

            NDiscoPlusArgs args = new()
            {
                Track = track,
                Features = features,
                Analysis = analysis,
                Effects = EffectConfig.Default,
                Lights = lights,

                ReferencePalette = palette.Palette
            };

            return new NDiscoPlusService().ComputeData(args);
        }

        /// <summary>
        /// Request NDPColorPalette. Return value will be null until data is available.
        /// </summary>
        public ColorPaletteData? RequestPalette()
        {
            if (Track is null)
                return null;

            palette ??= Task.Run(() => GetPalette(Track));

            if (palette.IsCompleted)
                return palette.Result;
            else
                return null;
        }

        /// <summary>
        /// Request NDPData. Return value will be null until data is available.
        /// </summary>
        public NDPData? RequestData(ImmutableArray<LightRecord> lights)
        {
            if (Track is null)
                return null;

            data ??= Task.Run(() => GetData(spotify, Track, lights));

            return TryGetDataWithoutRequest();
        }

        public NDPData? TryGetDataWithoutRequest()
        {
            if (data?.IsCompleted == true)
                return data.Result;
            else
                return null;
        }
    }

    private class PlayerData
    {
        public DataRecord Current { get; }
        public DataRecord Next { get; }

        public PlayerData(SpotifyClient spotify)
        {
            Current = new(spotify);
            Next = new(spotify);
        }
    }
}
