using NDiscoPlus.Shared.Analyzer.Analysis;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer;

// Bursts are a collection of multiple short (and approximately equal length) segments
internal static class AudioAnalyzerBurst
{
    private class Burst
    {
        public List<BurstNode> Nodes { get; }
        public int SplitNodeCount { get; set; }

        public double AverageConfidence => Nodes.Sum(n => (double)n.SegmentReference.Confidence) / Nodes.Count;

        public Burst()
        {
            Nodes = new();
            SplitNodeCount = 0;
        }
    }

    private readonly record struct BurstNode(NDPInterval Interval, TimeSpan SegmentOffset, NDPSegment SegmentReference)
    {
        public BurstNode(TimeSpan Start, TimeSpan Duration, TimeSpan SegmentOffset, NDPSegment SegmentReference) : this(
            Interval: new(Start, Duration),
            SegmentOffset: SegmentOffset,
            SegmentReference: SegmentReference
        )
        { }

        public static BurstNode NoOffset(NDPSegment segment)
        {
            return new(
                Interval: segment.Interval,
                SegmentOffset: TimeSpan.Zero,
                SegmentReference: segment
            );
        }

        public static (BurstNode First, BurstNode Second) SplitInHalf(NDPSegment segment)
        {
            IEnumerator<BurstNode> enumerator = SplitInto(segment, 2).GetEnumerator();

            bool firstMoveNext = enumerator.MoveNext();
            Debug.Assert(firstMoveNext);
            BurstNode first = enumerator.Current;

            bool secondMoveNext = enumerator.MoveNext();
            Debug.Assert(secondMoveNext);
            BurstNode second = enumerator.Current;

            Debug.Assert(!enumerator.MoveNext());

            return (first, second);
        }

        /// <summary>
        /// Split segment into <paramref name="count"/> count of <see cref="BurstNode"/> objects.
        /// </summary>
        private static IEnumerable<BurstNode> SplitInto(NDPSegment segment, int count)
        {
            TimeSpan duration = segment.Interval.Duration / count;
            for (int i = 0; i < count; i++)
            {
                TimeSpan startOffset = i * duration;
                TimeSpan start = segment.Interval.Start + startOffset;
                yield return new BurstNode(start, duration, startOffset, segment);
            }
        }
    }

    const double MaxDurationDifferenceRatio = 0.15d;
    const double MaxTimbreDistance = 100d;
    const int MinBurstLength = 4;
    const double MinBurstSegmentConfidence = 0.15d;

    private static bool NodeCanBeAddedToBurst(BurstNode node, Burst burst)
    {
        // No bursts to reference => segment can be added into burst
        if (burst.Nodes.Count < 1)
            return true;

        // Use last node in burst as reference
        BurstNode refNode = burst.Nodes[^1];


        if (Timbre.EuclideanDistance(node.SegmentReference.Timbre, refNode.SegmentReference.Timbre) > MaxTimbreDistance)
            return false;

        // ratio valid values: 0,9 - 1,1   (when MaxDurationDifference 0,1)
        double ratio = node.Interval.Duration.TotalSeconds / refNode.Interval.Duration.TotalSeconds;
        // diff valid values: -0,1 - 0,1   (when MaxDurationDifference 0,1)
        double diff = ratio - 1d;
        return Math.Abs(diff) <= MaxDurationDifferenceRatio;
    }

    private static bool BurstIsValid(Burst burst)
    {
        static int BurstMaxSplitCount(Burst burst)
        {
            // maximum 1/4 of segments can be split
            // or technically 1/3 since the split segment is 2 nodes
            return (int)Math.Round((1d / 4d) * burst.Nodes.Count);
        }

        static bool VerifyNodeDuration(BurstNode node, Burst burst)
        {
            const double minDur = 0.10;
            const double maxDur = 0.20;
            double maxSegmentDurationSeconds = ((double)burst.Nodes.Count)
                                                .Remap(4, 12, minDur, maxDur)
                                                .Clamp(minDur, maxDur);

            return node.Interval.Duration.TotalSeconds < maxSegmentDurationSeconds;
        }

        if (burst.SplitNodeCount > BurstMaxSplitCount(burst))
            return false;
        if (burst.Nodes.Count < MinBurstLength)
            return false;

        // Spotify is never completely confident on bursts
        // but bursts need to have at least some confidence
        if (burst.Nodes.Any(n => n.SegmentReference.Confidence < MinBurstSegmentConfidence))
            return false;
        if (!burst.Nodes.All(n => VerifyNodeDuration(n, burst)))
            return false;

        // minimum burst loudness ???

        return true;
    }

    // TODO: How to differentiate correct bursts from incorrect ones?
    // Now, some very important bursts are missing and some unnecessary ones exist...
    public static IEnumerable<ImmutableArray<NDPInterval>> AnalyzeBursts(ImmutableArray<NDPSegment> segments)
    {
        List<Burst> bursts = new() { new() };

        foreach (NDPSegment segment in segments)
        {
            // we initialize bursts list with a value so bursts.Count should always be above 0
            Burst lastBurst = bursts[^1];

            // Try add segment into burst
            BurstNode wholeSegment = BurstNode.NoOffset(segment);
            if (NodeCanBeAddedToBurst(wholeSegment, lastBurst))
            {
                lastBurst.Nodes.Add(wholeSegment);
                continue;
            }

            // Try split segment and add both parts into burst
            // (Spotify sometimes merges two segments into a longer segment)
            (BurstNode splitFirst, BurstNode splitSecond) = BurstNode.SplitInHalf(segment);
            if (NodeCanBeAddedToBurst(splitFirst, lastBurst) && NodeCanBeAddedToBurst(splitSecond, lastBurst))
            {
                lastBurst.Nodes.Add(splitFirst);
                lastBurst.Nodes.Add(splitSecond);
                lastBurst.SplitNodeCount++;
                continue;
            }

            // Start building a new burst with the whole segment
            Burst newBurst = new();
            newBurst.Nodes.Add(wholeSegment);
            bursts.Add(newBurst);
        }


        // enumerate output
        foreach (Burst burst in bursts)
        {
            if (BurstIsValid(burst))
                yield return burst.Nodes.Select(b => b.Interval).ToImmutableArray();
        }
    }
}
