using SpotifyAPI.Web;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace NDiscoPlus.Shared.Models;

public readonly record struct NDPInterval(TimeSpan Start, TimeSpan Duration)
{
    public static NDPInterval FromSeconds(double Start, double Duration)
        => new(TimeSpan.FromSeconds(Start), TimeSpan.FromSeconds(Duration));
    public static NDPInterval FromStartAndEnd(TimeSpan Start, TimeSpan End)
        => new(Start, End - Start);

    public TimeSpan End => Start + Duration;

    /// <summary>
    /// Check whether this <see cref="NDPInterval"/> contains the provided <see cref="TimeSpan"/>.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Start"/> is inclusive.</para>
    /// <para><see cref="End"/> is exclusive.</para>
    /// </remarks>
    public bool Contains(TimeSpan t)
    {
        return t >= Start && t < End;
    }

    /// <summary>
    /// Check whether this <see cref="NDPInterval"/> contains another <see cref="NDPInterval"/>.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Start"/> is inclusive.</para>
    /// <para><see cref="End"/> is inclusive.</para>
    /// </remarks>
    public bool ContainsInterval(NDPInterval interval)
    {
        return interval.Start >= Start && interval.End <= End;
    }

    /// <summary>
    /// Check if the two <see cref="NDPInterval"/>s overlap.
    /// </summary>
    /// <remarks>
    /// <para><see cref="Start"/> is exclusive.</para>
    /// <para><see cref="End"/> is exclusive.</para>
    /// </remarks>
    public static bool Overlap(NDPInterval a, NDPInterval b)
    {
        return a.Start < b.End && b.Start < a.End;
    }

    public static explicit operator NDPInterval(TimeInterval interval)
        => FromSeconds(interval.Start, interval.Duration);
}