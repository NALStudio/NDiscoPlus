using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer;

// Highlights are a collection of segments that stand out from the rest of the song
internal static class AudioAnalyzerHighlight
{
    private const double TimbreDistMustBeOverBeforeStart = 120d;
    private const double TimbreDistMustBeLessThanStartMinusThis = 90d; // 120 - 90 = 30
    private const double TimbreDistMustBeLessThanThisMax = 70d; // distance must be less than 30 - 70

    private const int MinHighlightSegmentCount = 3; // 3 had too many misfires
    private const int MaxHighlightSegmentCount = 8;

    private const double MaxSegmentDuration = 0.25d;

    private class Highlight
    {
        public double StartingTimbreDistance { get; }

        private readonly List<NDPSegment> segments;
        public IReadOnlyList<NDPSegment> Segments => segments.AsReadOnly();

        public TimeSpan Start => Segments.Count > 0 ? Segments[0].Interval.Start : throw new InvalidOperationException("Segments list empty");
        public TimeSpan End => Segments.Count > 0 ? Segments[^1].Interval.End : throw new InvalidOperationException("Segments list empty");

        public double AverageDistanceToStart()
        {
            double dist = AverageDistance();
            Debug.Assert(StartingTimbreDistance > dist);
            return StartingTimbreDistance - dist;
        }

        private double AverageDistance()
        {
            Debug.Assert(segments.Count > 0);

            double dist = 0d;

            int distCount = segments.Count - 1;
            for (int i = 0; i < distCount; i++)
                dist += Timbre.EuclideanDistance(segments[i].Timbre, segments[i + 1].Timbre);

            return dist / distCount;
        }

        public Highlight(double startingTimbreDistance, NDPSegment startingSegment)
        {
            StartingTimbreDistance = startingTimbreDistance;
            segments = new() { startingSegment };
        }

        public void Add(NDPSegment segment) => segments.Add(segment);
    }

    public static IEnumerable<ImmutableArray<NDPInterval>> AnalyzeHighlights(ImmutableArray<NDPSegment> segments)
    {
        List<Highlight> output = new();

        foreach (Highlight highlight in ConstructHighlights(segments))
        {
            if (!IsHighlightValid(highlight))
                continue;

            int overlapIndex = HighlightOverlapsWithIndex(highlight, output);
            if (overlapIndex != -1)
            {
                // If this highlight has a smaller timbre distance than the overlapping highlight,
                // replace the overlapping one with this instead
                if (highlight.AverageDistanceToStart() < output[overlapIndex].AverageDistanceToStart())
                    output[overlapIndex] = highlight;
            }
            else
            {
                output.Add(highlight);
            }
        }

        return EnumerateOutput(output);
    }

    private static IEnumerable<ImmutableArray<NDPInterval>> EnumerateOutput(List<Highlight> output)
    {
        foreach (Highlight h in output)
            yield return h.Segments.Select(static s => s.Interval).ToImmutableArray();
    }

    private static IEnumerable<Highlight> ConstructHighlights(ImmutableArray<NDPSegment> segments)
    {
        List<Highlight> highlights = new();
        NDPSegment? previous = null;
        foreach (NDPSegment segment in segments)
        {
            // Handle existing highlights
            List<Highlight> finishedHighlights = new();
            foreach (Highlight highlight in highlights)
            {
                if (CanBeAddedIntoHighlight(segment, highlight))
                {
                    // Add segment to highlight
                    highlight.Add(segment);
                }
                else
                {
                    // Add highlight to finished
                    // These highlights will be yield returned and then removed from the highlights list.
                    finishedHighlights.Add(highlight);
                }
            }

            // Remove finished highlights
            foreach (Highlight finished in finishedHighlights)
            {
                highlights.Remove(finished);
                yield return finished;
            }

            // Start new highlight if possible (after highlight handling so the first segment isn't added twice)
            if (CanStartNewHighlight(previous, segment, out double distance))
                highlights.Add(new Highlight(distance, segment));

            previous = segment;
        }

        // Yield any highlights still in buffer
        foreach (Highlight stillInBuffer in highlights)
            yield return stillInBuffer;
    }

    private static bool CanBeAddedIntoHighlight(NDPSegment segment, Highlight highlight)
    {
        double maxDistance = highlight.StartingTimbreDistance - TimbreDistMustBeLessThanStartMinusThis;
        if (maxDistance > TimbreDistMustBeLessThanThisMax)
            maxDistance = TimbreDistMustBeLessThanThisMax;

        NDPSegment last = highlight.Segments[^1];
        double distance = Timbre.EuclideanDistance(segment.Timbre, last.Timbre);

        return distance < maxDistance;
    }

    private static bool CanStartNewHighlight(NDPSegment? previous, NDPSegment current, [MaybeNullWhen(false)] out double distance)
    {
        if (!previous.HasValue)
        {
            distance = default;
            return false;
        }

        distance = Timbre.EuclideanDistance(previous.Value.Timbre, current.Timbre);
        return distance > TimbreDistMustBeOverBeforeStart;
    }

    private static bool IsHighlightValid(Highlight highlight)
    {
        if (highlight.Segments.Count < MinHighlightSegmentCount)
            return false;
        if (highlight.Segments.Count > MaxHighlightSegmentCount)
            return false;

        // All segments must be above minimum volume
        if (!highlight.Segments.All(static s => AudioAnalysisHelpers.SegmentHasProperSound(s)))
            return false;

        // All segments must be less than max duration
        if (highlight.Segments.Any(static s => s.Interval.Duration.TotalSeconds > MaxSegmentDuration))
            return false;

        return true;
    }

    /// <summary>
    /// -1 if doesn't overlap.
    /// </summary>
    private static int HighlightOverlapsWithIndex(Highlight highlight, IReadOnlyList<Highlight> all)
    {
        NDPInterval interval = NDPInterval.FromStartAndEnd(highlight.Start, highlight.End);

        for (int i = 0; i < all.Count; i++)
        {
            Highlight reference = all[i];
            NDPInterval referenceInterval = NDPInterval.FromStartAndEnd(reference.Start, reference.End);

            if (NDPInterval.Overlap(interval, referenceInterval))
                return i;
        }

        return -1;
    }
}
