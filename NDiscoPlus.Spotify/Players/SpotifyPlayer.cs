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

        PeriodicTimer timer = new(TimeSpan.FromSeconds(periodSeconds));

        while (true)
        {
            // Catch result inside loop since yield return does not work inside a try catch block
            bool result;
            try
            {
                result = await timer.WaitForNextTickAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                result = false;
            }

            if (!result)
            {
                timer.Dispose();
                yield break;
            }

            yield return Update();
        }
    }
}
