using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using MudBlazor;
using MudBlazor.Charts;
using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NDiscoPlus.Components;

public class TrackDebugCanvasRenderSegments : TrackDebugCanvasRender
{
    private enum Style
    {
        Decibels,
        Distance
    }

    private readonly record struct SegmentRender(Segment Segment, double X, double Width, Segment? PreviousSegment)
    {
        public bool GetVisible(double canvasWidth)
        {
            double Left = X;
            double Right = X + Width;
            return (Right >= 0d) && (Left <= canvasWidth);

        }
    }

    double windowSizeSeconds = 5d;

    const double minLoudness = -60;
    const double maxLoudness = 0;

    private readonly Style style;

    private TrackDebugCanvasRenderSegments(Style style, Batch2D canvas, int canvasWidth, int canvasHeight, SpotifyPlayerContext context, TrackAudioFeatures features, TrackAudioAnalysis analysis) : base(canvas, canvasWidth, canvasHeight, context, features, analysis)
    {
        this.style = style;
    }
    public static TrackDebugCanvasRenderSegments DecibelsStyle(Batch2D canvas, int canvasWidth, int canvasHeight, SpotifyPlayerContext context, TrackAudioFeatures features, TrackAudioAnalysis analysis)
        => new(Style.Decibels, canvas: canvas, canvasWidth: canvasWidth, canvasHeight: canvasHeight, context: context, features: features, analysis: analysis);
    public static TrackDebugCanvasRenderSegments DistanceStyle(Batch2D canvas, int canvasWidth, int canvasHeight, SpotifyPlayerContext context, TrackAudioFeatures features, TrackAudioAnalysis analysis)
        => new(Style.Distance, canvas: canvas, canvasWidth: canvasWidth, canvasHeight: canvasHeight, context: context, features: features, analysis: analysis);

    public override async Task RenderAsync()
    {
        int currentSegmentIndex = await RenderSegments();
        await RenderPlayer(currentSegmentIndex);
    }

    double GetScrollingX(double start)
    {
        double halfWindowSize = windowSizeSeconds / 2d;
        double progress = player.Progress.TotalSeconds;

        double windowLeft = progress - halfWindowSize;
        double windowRight = progress + halfWindowSize;

        return start.Remap(windowLeft, windowRight, 0d, canvasWidth);
    }

    double GetWidth(double duration)
    {
        double frac = duration / windowSizeSeconds;
        return frac * canvasWidth;
    }

    /// <summary>
    /// Return the index of the current segment.
    /// </summary>
    /// <returns></returns>
    async Task<int> RenderSegments()
    {
        // The return value i is such that all e in a[..i] have e <= x, and all e in a[i..] have e > x.
        // so Segments[i - 1].Start <= Progress
        int nextSegment = Bisect.BisectRight(analysis.Segments, player.Progress.TotalSeconds, segment => (double)segment.Start);
        int currentSegment = nextSegment - 1;

        await canvas.LineWidthAsync(1);

        SegmentRender[] renders = VisibleRenders(currentSegment, analysis.Segments).ToArray();

        if (style == Style.Decibels)
        {
            await canvas.FillStyleAsync("hsl(0, 100%, 50%)");
            await canvas.StrokeStyleAsync("hsl(0, 100%, 35%)");
            foreach (SegmentRender r in renders)
                await RenderSegmentMax(r);

            await canvas.FillStyleAsync("hsl(60, 100%, 50%)");
            await canvas.StrokeStyleAsync("hsl(60, 100%, 35%)");
            foreach (SegmentRender r in renders)
                await RenderSegmentRise(r);

            await canvas.FillStyleAsync("hsl(120, 100%, 50%)");
            // await canvas.StrokeStyleAsync("hsl(120, 100%, 35%)");
            foreach (SegmentRender r in renders)
                await RenderSegmentLoudnessStart(r);
        }
        else if (style == Style.Distance)
        {
            await canvas.FillStyleAsync("#000000");
            await canvas.StrokeStyleAsync("#000000");
            foreach (SegmentRender r in renders)
                await RenderSegmentTimbreDistance(r);
            foreach (SegmentRender r in renders)
                await RenderSegmentConfidence(r);
        }
        else
        {
            throw StyleNotImplementedError(style);
        }


        return currentSegment;
    }

    IEnumerable<SegmentRender> VisibleRenders(int currentIndex, List<Segment> segments)
    {
        SegmentRender CreateRender(int index)
        {
            Segment segment = segments[index];
            Segment? previousSegment = index > 0 ? segments[index - 1] : null;
            return new SegmentRender(segment, GetScrollingX(segment.Start), GetWidth(segment.Duration), previousSegment);
        }

        for (int i = currentIndex; i >= 0; i--)
        {
            SegmentRender rend = CreateRender(i);
            if (!rend.GetVisible(canvasWidth))
                break;
            yield return rend;
        }

        for (int i = currentIndex + 1; i < analysis.Segments.Count; i++)
        {
            SegmentRender rend = CreateRender(i);
            if (!rend.GetVisible(canvasWidth))
                break;
            yield return rend;
        }
    }

    async Task RenderSegmentLoudnessStart(SegmentRender segment)
    {
        double segmentHeight = DoubleHelpers.Remap(segment.Segment.LoudnessStart, minLoudness, maxLoudness, 0, canvasHeight);

        await canvas.FillRectAsync(segment.X, canvasHeight - segmentHeight, segment.Width, segmentHeight);
        // await canvas.StrokeRectAsync(segment.X, canvasHeight - segmentHeight, segment.Width, segmentHeight);
    }

    async Task RenderSegmentConfidence(SegmentRender segment)
    {
        // double height = DoubleHelpers.Lerp(0, canvasHeight, segment.Segment.Confidence);
        double hue = DoubleHelpers.Lerp(0, 120, segment.Segment.Confidence);
        double height = 0.9 * canvasHeight;
        double y = canvasHeight - height;

        await canvas.FillStyleAsync($"hsl({hue.ToString(CultureInfo.InvariantCulture)}, 100%, 50%)");
        await canvas.FillRectAsync(segment.X, y, segment.Width, height);
        await canvas.StrokeRectAsync(segment.X, y, segment.Width, height);
    }

    async Task RenderSegmentTimbreDistance(SegmentRender segment)
    {
        if (segment.PreviousSegment is null)
            return;

        Timbre currentTimbre = new(segment.Segment.Timbre);
        Timbre previousTimbre = new(segment.PreviousSegment.Timbre);
        string text = Timbre.EuclideanDistance(currentTimbre, previousTimbre).ToString("0.") + ".";

        double x = segment.X;
        double y = 0.075 * canvasHeight;
        await canvas.FillTextAsync(text, x, y);
    }

    async Task RenderSegmentMax(SegmentRender render)
    {
        Segment segment = render.Segment;

        double maxHeight = DoubleHelpers.Remap(segment.LoudnessMax, minLoudness, maxLoudness, 0, canvasHeight);
        double maxX = GetScrollingX(segment.Start + segment.LoudnessMaxTime);
        double maxWidth = GetWidth(segment.Duration - segment.LoudnessMaxTime);

        await canvas.FillRectAsync(maxX, canvasHeight - maxHeight, maxWidth, maxHeight);
        // await canvas.StrokeRectAsync(maxX, canvasHeight - maxHeight, maxWidth, maxHeight);
    }

    async Task RenderSegmentRise(SegmentRender render)
    {
        Segment segment = render.Segment;

        double maxHeight = DoubleHelpers.Remap(segment.LoudnessMax, minLoudness, maxLoudness, 0, canvasHeight);

        double width = GetWidth(segment.LoudnessMaxTime);

        await canvas.FillRectAsync(render.X, canvasHeight - maxHeight, width, maxHeight);
    }

    async Task RenderPlayer(int currentSegmentIndex)
    {
        await canvas.FillStyleAsync("#000000");

        int nextSegmentIndex = currentSegmentIndex + 1;
        int prevSegmentIndex = currentSegmentIndex - 1;

        Segment current = analysis.Segments[currentSegmentIndex];
        Segment? prev = prevSegmentIndex >= 0 ? analysis.Segments[prevSegmentIndex] : null;
        Segment? next = nextSegmentIndex < analysis.Segments.Count ? analysis.Segments[nextSegmentIndex] : null;

        string[] lines;
        if (style == Style.Decibels)
        {
            lines = [
                $"{current.LoudnessStart} dB for: {current.Duration} sec",
                $"{current.LoudnessMax} dB (max) at: {current.LoudnessMaxTime} sec",
            ];
        }
        else if (style == Style.Distance)
        {
            Timbre currentTimbre = new(current.Timbre);
            double? distanceFromPrev = prev is not null ? Timbre.EuclideanDistance(currentTimbre, new Timbre(prev.Timbre)) : null;
            double? distanceToNext = next is not null ? Timbre.EuclideanDistance(currentTimbre, new Timbre(next.Timbre)) : null;

            lines = [
                $"{current.Confidence * 100:.00} % confidence",
            ];
        }
        else
        {
            throw StyleNotImplementedError(style);
        }

        double pos = canvasWidth / 2d;

        await canvas.StrokeStyleAsync("#000000");
        await canvas.LineWidthAsync(2);

        await canvas.BeginPathAsync();
        await canvas.MoveToAsync(pos, 0d);
        await canvas.LineToAsync(pos, canvasHeight);
        await canvas.StrokeAsync();

        double textY = 0d;
        foreach (string line in lines)
        {
            textY += lineHeight;
            await canvas.FillTextAsync(line, pos + 1, textY);
        }
    }

    // return error as DoesNotReturnAttribute doesn't tell the compiler that the rest of the code is inaccessible.
    static NotImplementedException StyleNotImplementedError(Style style)
        => throw new NotImplementedException($"Style not implemented: {style}");

}
