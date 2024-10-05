using SpotifyAPI.Web;
using System.Collections.Immutable;

namespace NDiscoPlus.Spotify.Models;

public readonly record struct TrackImage(int Size, string Url);

public class SpotifyPlayerTrack
{
    public SpotifyPlayerTrack(string id, string name, TimeSpan length, IEnumerable<TrackImage> images, ImmutableArray<string> artists)
    {
        Id = id;
        Name = name;
        Length = length;
        Images = images.OrderByDescending(img => img.Size).ToImmutableArray();
        Artists = artists;
    }

    public string Id { get; init; }
    public string Name { get; init; }
    public TimeSpan Length { get; init; }

    /// <summary>
    /// Track album cover images from largest to smallest.
    /// </summary>
    public ImmutableArray<TrackImage> Images { get; init; }

    public TrackImage LargestImage => Images[0];
    public TrackImage SmallestImage => Images[^1];

    public ImmutableArray<string> Artists { get; init; }

    public static SpotifyPlayerTrack FromSpotifyTrack(FullTrack track)
    {
        ImmutableArray<TrackImage> images = track.Album.Images.Where(i => i.Width == i.Height)
                                           .Select(i => new TrackImage(i.Width, i.Url))
                                           .ToImmutableArray();
        if (images.Length < 1)
            throw new ArgumentException("No valid album cover images in track.");

        return new SpotifyPlayerTrack(
            id: track.Id,
            name: track.Name,
            length: TimeSpan.FromMilliseconds(track.DurationMs),
            images: images,
            artists: track.Artists.Select(a => a.Name).ToImmutableArray()
        );
    }

    public static SpotifyPlayerTrack? FromSpotifyTrackOrNull(FullTrack? track)
    {
        if (track is null)
            return null;
        return FromSpotifyTrack(track);
    }
}

public class SpotifyPlayerContext
{
    public SpotifyPlayerContext(TimeSpan progress, bool isPlaying, SpotifyPlayerTrack track, SpotifyPlayerTrack? nextTrack)
    {
        Progress = progress;
        IsPlaying = isPlaying;
        Track = track;
        NextTrack = nextTrack;
    }

    public TimeSpan Progress { get; }
    public bool IsPlaying { get; }
    public SpotifyPlayerTrack Track { get; }

    /// <summary>
    /// The next track information is supplied once we are almost certain what the next track is going to be.
    /// This depends on the implementation:
    ///     - Spotify web player gives this info in the last 20 seconds of the currently playing song and updates it every 5 seconds.
    /// </summary>
    public SpotifyPlayerTrack? NextTrack { get; }
}