using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer.Analysis;

public sealed class AudioAnalysisSection
{
    private AudioAnalysisSection(NDPInterval interval, AudioAnalysisTimings timings, float loudness, Tempo tempo)
    {
        Interval = interval;
        Timings = timings;

        Loudness = loudness;
        Tempo = tempo;
    }

    public static AudioAnalysisSection ConstructFromSpotify(AudioAnalysisTimings timings, Section section, NDPInterval? intervalOverride)
    {
        NDPInterval interval = intervalOverride ?? NDPInterval.FromSeconds(section.Start, section.Duration);

        ImmutableArray<NDPInterval> bars = TakeSectionSlice(interval, timings.Bars);
        ImmutableArray<NDPInterval> beats = TakeSectionSlice(interval, timings.Beats);
        ImmutableArray<NDPInterval> tatums = TakeSectionSlice(interval, timings.Tatums);
        ImmutableArray<NDPInterval> segments = TakeSectionSlice(interval, timings.Segments);

        return new AudioAnalysisSection(
            interval: interval,
            timings: new AudioAnalysisTimings(
                bars: bars,
                beats: beats,
                tatums: tatums,
                segments: segments
            ),
            loudness: section.Loudness,
            tempo: new Tempo(section.Tempo, section.TimeSignature)
        );
    }

    /// <summary>
    /// Take all intervals that fall inside this section.
    /// </summary>
    private static ImmutableArray<NDPInterval> TakeSectionSlice(NDPInterval sectionInterval, ImmutableArray<NDPInterval> intervals)
    {
        static bool StartInsideSectionStart(TimeSpan sectionStart, NDPInterval interval)
            => interval.Start >= sectionStart;
        static bool StartInsideSectionEnd(TimeSpan sectionEnd, NDPInterval interval)
            => interval.Start < sectionEnd;

        int start = 0;
        while (start < intervals.Length && !StartInsideSectionStart(sectionStart: sectionInterval.Start, intervals[start]))
            start++;

        int end = start;
        while (end < intervals.Length && StartInsideSectionEnd(sectionEnd: sectionInterval.End, intervals[end]))
            end++;

        return intervals[start..end];  // Makes a new immutable array from the same backing array (items aren't copied)
    }

    public NDPInterval Interval { get; }
    public AudioAnalysisTimings Timings { get; }

    /// <summary>
    /// <para>The overall loudness of the section in decibels (dB).</para>
    /// <para>Loudness values are useful for comparing relative loudness of sections within tracks.</para>
    /// </summary>
    public float Loudness { get; }

    /// <summary>
    /// <para>The overall estimated tempo of the section in beats per minute (BPM).</para>
    /// <para>In musical terminology, tempo is the speed or pace of a given piece and derives directly from the average beat duration.</para>
    /// </summary>
    public Tempo Tempo { get; }
}
