using NDiscoPlus.Shared.Analyzer.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer;
internal static class AudioAnalysisHelpers
{
    public static bool SegmentHasProperSound(NDPSegment segment)
    {
        // Seems like -10 for max loudness and -15 for loudness start are best values
        // Currently I only check for max loudness, but this might change in the future.
        const double minLoudnessMax = -10d;
        if (segment.LoudnessMax < minLoudnessMax)
            return false;

        // What is the best way to check for sound?
        // LoudnessStart, LoudnessMax or should I compute the average loudness?
        // I currently only use LoudnessMax

        return true;
    }
}
