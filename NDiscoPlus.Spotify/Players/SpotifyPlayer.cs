using NDiscoPlus.Spotify.Models;
using System.Runtime.CompilerServices;

namespace NDiscoPlus.Spotify.Players;

public delegate void TrackChanged(SpotifyPlayerTrack? oldTrack, SpotifyPlayerTrack? newTrack);

public abstract class SpotifyPlayer : IDisposable
{
    private PeriodicTimer? _timer;

    protected abstract ValueTask Init(int updateFrequency);
    protected abstract SpotifyPlayerContext? Update(); // Synchronous so that the timer isn't blocked by slow requests etc.

    public async IAsyncEnumerable<SpotifyPlayerContext?> ListenAsync(int frequency, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        double periodSeconds = 1d / frequency; // Hz = 1/s => s = 1/Hz

        // initialize timer before awaiting so that we dispose correctly.
        _timer = new(TimeSpan.FromSeconds(periodSeconds));

        await Init(frequency);

        while (await _timer.WaitForNextTickAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Update();
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
