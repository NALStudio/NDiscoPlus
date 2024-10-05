using NDiscoPlus.Shared.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer.Analysis;
public class AudioAnalysisTimings
{
    /// <summary>
    /// <para>The time intervals of the bars throughout the track.</para>
    /// <para>A bar (or measure) is a segment of time defined as a given number of beats.</para>
    /// </summary>
    public ImmutableArray<NDPInterval> Bars { get; }

    /// <summary>
    /// <para>The time intervals of beats throughout the track.</para>
    /// <para>A beat is the basic time unit of a piece of music; for example, each tick of a metronome. Beats are typically multiples of tatums.</para>
    /// </summary>
    public ImmutableArray<NDPInterval> Beats { get; }

    /// <summary>
    /// A tatum represents the lowest regular pulse train that a listener intuitively infers from the timing of perceived musical events (segments).
    /// </summary>
    public ImmutableArray<NDPInterval> Tatums { get; }

    /// <summary>
    /// Each segment contains a roughly conisistent sound throughout its duration.
    /// </summary>
    public ImmutableArray<NDPInterval> Segments { get; }

    public AudioAnalysisTimings(ImmutableArray<NDPInterval> bars, ImmutableArray<NDPInterval> beats, ImmutableArray<NDPInterval> tatums, ImmutableArray<NDPInterval> segments)
    {
        Bars = bars;
        Beats = beats;
        Tatums = tatums;
        Segments = segments;
    }
}
