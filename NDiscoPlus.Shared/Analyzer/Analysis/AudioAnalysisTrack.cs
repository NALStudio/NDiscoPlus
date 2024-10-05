using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Analyzer.Analysis;
internal readonly record struct Fade(TimeSpan Start, TimeSpan End)
{
    public TimeSpan Duration => End - Start;
}

public readonly struct Tempo
{
    public Tempo(float tempo, int timeSignature)
    {
        TempoValue = tempo;
        TimeSignature = timeSignature;
    }

    public float TempoValue { get; }

    /// <summary>
    /// <para>An estimated time signature.</para>
    /// <para>The time signature (meter) is a notational convention to specify how many beats are in each bar (or measure).</para>
    /// <para>The time signature ranges from 3 to 7 indicating time signatures of "3/4", to "7/4".</para>
    /// </summary>
    public int TimeSignature { get; }

    /// <summary>
    /// <para>How many minutes there are in a beat.</para>
    /// <para>Inverse of <see cref="TempoValue"/>.</para>
    /// </summary>
    public double MinutesPerBeat => 1d / TempoValue;

    /// <summary>
    /// How many seconds there are in a beat.
    /// </summary>
    public double SecondsPerBeat => MinutesPerBeat * 60d;

    /// <summary>
    /// How many seconds there are in a bar.
    /// </summary>
    public double SecondsPerBar => SecondsPerBeat * TimeSignature;

    public static implicit operator float(Tempo tempo) => tempo.TempoValue;
    public static implicit operator double(Tempo tempo) => tempo.TempoValue;
}

internal class AudioAnalysisTrack
{
    /// <summary>
    /// Length of the track.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// <para>The track's fade-in data (if any)</para>
    /// <para>Fade-in start will always be <see cref="TimeSpan.Zero"/></para>
    /// </summary>
    public Fade? FadeIn { get; }

    /// <summary>
    /// <para>The track's fade-out data (if any)</para>
    /// <para>Fade-out end will always be <see cref="Duration"/></para>
    /// </summary>
    public Fade? FadeOut { get; }

    /// <summary>
    /// The overall loudness of the effect's section in decibels (dB).
    /// </summary>
    public float Loudness { get; }

    /// <summary>
    /// <para>The overall estimated tempo of the effect's section in beats per minute (BPM).</para>
    /// <para>In musical terminology, tempo is the speed or pace of a given piece and derives directly from the average beat duration.</para>
    /// </summary>
    public Tempo Tempo { get; }

    private AudioAnalysisTrack(TimeSpan duration, Fade? fadeIn, Fade? fadeOut, float loudness, Tempo tempo)
    {
        Duration = duration;
        FadeIn = fadeIn;
        FadeOut = fadeOut;
        Loudness = loudness;
        Tempo = tempo;
    }

    private static Fade? ConstructFadeIfNecessary(TimeSpan start, TimeSpan end)
    {
        if (end > start)
            return new Fade(start, end);
        return null;
    }

    public static AudioAnalysisTrack FromSpotify(SpotifyAPI.Web.TrackAudio track)
    {
        TimeSpan trackDuration = TimeSpan.FromSeconds(track.Duration);

        Fade? fadeIn = ConstructFadeIfNecessary(start: TimeSpan.Zero, end: TimeSpan.FromSeconds(track.EndOfFadeIn));
        Fade? fadeOut = ConstructFadeIfNecessary(start: TimeSpan.FromSeconds(track.StartOfFadeOut), trackDuration);

        return new(
            duration: trackDuration,
            fadeIn: fadeIn,
            fadeOut: fadeOut,
            loudness: track.Loudness,
            tempo: new Tempo(track.Tempo, track.TimeSignature)
        );
    }
}