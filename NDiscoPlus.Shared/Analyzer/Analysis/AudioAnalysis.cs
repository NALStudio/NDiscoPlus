using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer.Analysis;
internal class AudioAnalysis
{
    public AudioAnalysisTrack Track { get; }
    public AudioAnalysisFeatures Features { get; }

    public AudioAnalysisTimings Timings { get; }

    public ImmutableArray<AudioAnalysisSection> Sections { get; }
    public AudioAnalysisSegments Segments { get; }

    internal AudioAnalysis(AudioAnalysisTrack track, AudioAnalysisFeatures features, AudioAnalysisTimings timings, ImmutableArray<AudioAnalysisSection> sections, AudioAnalysisSegments segments)
    {
        Track = track;
        Features = features;
        Timings = timings;
        Sections = sections;
        Segments = segments;
    }
}