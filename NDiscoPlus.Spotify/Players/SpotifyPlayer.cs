using NDiscoPlus.Spotify.Models;
using System.Runtime.CompilerServices;

namespace NDiscoPlus.Spotify.Players;

public delegate void TrackChanged(SpotifyPlayerTrack? oldTrack, SpotifyPlayerTrack? newTrack);

public abstract class SpotifyPlayer
{
    protected abstract ValueTask Init();
    protected abstract SpotifyPlayerContext? Update(); // Synchronous so that the timer isn't blocked by slow requests etc.

    public async IAsyncEnumerable<SpotifyPlayerContext?> ListenAsync(int frequency, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Init();

        double periodSeconds = 1d / frequency; // Hz = 1/s => s = 1/Hz

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(periodSeconds));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Update();
        }
    }
}
