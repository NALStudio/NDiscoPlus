using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using NDiscoPlus.Shared;
using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Music;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System.Collections.Immutable;

namespace NDiscoPlus.Components;

// TODO: Only render audio analyzed sections and draw lines on the bottom bar for spotify sections
public class TrackDebugCanvasRenderMeta : TrackDebugCanvasRender
{
    public TrackDebugCanvasRenderMeta(Batch2D canvas, int canvasWidth, int canvasHeight, SpotifyPlayerContext context, TrackAudioFeatures features, TrackAudioAnalysis analysis) : base(canvas, canvasWidth, canvasHeight, context, features, analysis)
    {
    }

    public override async Task RenderAsync()
    {
        int trackHeight = 50;
        int trackY = canvasHeight - trackHeight;

        int sectionsHeight = 100;
        int sectionsY = trackY - sectionsHeight;

        int segmentsY = 0;
        int segmentsHeight = sectionsY - segmentsY;

        await RenderTrack(y: trackY, height: trackHeight);
        await RenderSections(y: sectionsY, height: sectionsHeight);
        await RenderPlayer();
    }

    double Transform(TimeSpan time)
        => Transform(time.TotalSeconds);

    double Transform(double time)
    {
        double length = player.Track.Length.TotalSeconds;

        double progress = time / length;

        return progress * canvasWidth;
    }

    async Task RenderTrack(int y, int height)
    {
        EffectIntensity intensity = MusicEffectGenerator.DebugIntensityFromFeatures(features);

        await RenderIntensity(intensity, originalInterval: null, modifiedInterval: NDPInterval.FromSeconds(0f, analysis.Track.Duration), y, height);
        // keep using RenderIntensity's text color

        string[] data = ["Track Features:", $"{features.Loudness} dB", $"{features.Tempo} BPM"];

        double textY = 0d;
        foreach (string d in data)
        {
            textY += lineHeight;
            await canvas.FillTextAsync(d, 0d, y + textY);
        }
    }

    async Task RenderSections(int y, int height)
    {
        IReadOnlyList<ComputedIntensity> intensities = MusicEffectGenerator.DebugComputeIntensities(features, analysis);

        for (int i = 0; i < intensities.Count; i++)
        {
            Section spotifySection = analysis.Sections[i];
            (EffectIntensity intensity, AudioAnalysisSection section) = intensities[i];

            double x = Transform(section.Interval.Start);
            double width = Transform(section.Interval.Duration);

            await RenderIntensity(intensity,
                originalInterval: NDPInterval.FromSeconds(spotifySection.Start, spotifySection.Duration),
                modifiedInterval: section.Interval,
                y, height, centerTextVertically: false);
            // keep using RenderIntensity's text color

            string[] data = [$"Section {i}:", $"{section.Loudness} dB", $"{section.Tempo.TempoValue} BPM", $"{section.Interval.Duration.TotalSeconds:.000} seconds"];

            double textY = 0d;
            foreach (string d in data)
            {
                textY += lineHeight;
                await canvas.FillTextAsync(d, x, y + textY);
            }
        }
    }

    async Task RenderIntensity(EffectIntensity intensity, NDPInterval? originalInterval, NDPInterval modifiedInterval, double y, double height, bool centerTextVertically = true)
    {
        double x = Transform(modifiedInterval.Start);
        double width = Transform(modifiedInterval.Duration);

        double topLineX = originalInterval.HasValue ? Transform(originalInterval.Value.Start) : x;
        double topLineWidth = originalInterval.HasValue ? Transform(originalInterval.Value.Duration) : width;

        EffectIntensity[] allIntensities = Enum.GetValues<EffectIntensity>();
        EffectIntensity minIntensity = allIntensities.Min();
        EffectIntensity maxIntensity = allIntensities.Max();

        double hue = DoubleHelpers.Remap((int)intensity, (int)minIntensity, (int)maxIntensity, 0d, 120d);
        string hsl50 = $"hsl({(int)hue}, 100%, 50%)";
        string hsl35 = $"hsl({(int)hue}, 100%, 35%)";

        const double originalLineWidth = 1d;

        await canvas.FillStyleAsync(hsl50);
        await canvas.LineWidthAsync(originalLineWidth);
        await canvas.StrokeStyleAsync(hsl35);

        await canvas.FillRectAsync(x, y, width, height);
        await canvas.StrokeRectAsync(x, y, width, height);

        if (x != topLineX || width != topLineWidth)
        {
            await canvas.LineWidthAsync(5);

            await canvas.BeginPathAsync();
            await canvas.MoveToAsync(topLineX, y);
            await canvas.LineToAsync(topLineX + topLineWidth, y);
            await canvas.StrokeAsync();

            await canvas.LineWidthAsync(originalLineWidth);
        }

        await canvas.FillStyleAsync(hsl35);

        string intensityText = $"{intensity:G} ({intensity:D})";

        // clipping
        await canvas.SaveAsync();
        await canvas.BeginPathAsync();
        await canvas.RectAsync(x, y, width, height);
        await canvas.ClipAsync(FillRule.NonZero);

        // text
        double textX = width / 2d;
        double textY = height;
        if (centerTextVertically)
            textY /= 2d;
        await canvas.TextBaseLineAsync(centerTextVertically ? TextBaseLine.Middle : TextBaseLine.Bottom);
        await canvas.TextAlignAsync(TextAlign.Center);
        await canvas.FillTextAsync(intensityText, x + textX, y + textY);

        // remove clipping
        await canvas.RestoreAsync();
    }

    async Task RenderPlayer()
    {
        double pos = Transform(player.Progress);

        await canvas.StrokeStyleAsync("#000000");
        await canvas.LineWidthAsync(2);

        await canvas.BeginPathAsync();
        await canvas.MoveToAsync(pos, 0d);
        await canvas.LineToAsync(pos, canvasHeight);
        await canvas.StrokeAsync();
    }
}
