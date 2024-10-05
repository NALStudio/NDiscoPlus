using Microsoft.Extensions.Logging;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace NDiscoPlus.Spotify.Players;

record PlayingContext
{
    public PlayingContext(DateTimeOffset fetchTimestamp, CurrentlyPlayingContext? context, FullTrack? track)
    {
        if (context is null != track is null)
            throw new ArgumentException("Context and track have to both be equally null.");

        FetchTimestamp = fetchTimestamp;
        Context = context;
        Track = track;
    }

    public DateTimeOffset FetchTimestamp { get; }

    [NotNullIfNotNull(nameof(Track))]
    public CurrentlyPlayingContext? Context { get; }

    [NotNullIfNotNull(nameof(Context))]
    public FullTrack? Track { get; }

    public TimeSpan ComputeCurrentProgress(DateTimeOffset nowUtc)
    {
        return TimeSpan.FromMilliseconds(Context!.ProgressMs) + (nowUtc - FetchTimestamp);
    }
}

public class SpotifyWebPlayer : SpotifyPlayer
{
    // how many seconds there should be between polls (very coarse; elapsed time is computed very inaccurately)
    const int _kSuperFastPollRate = 1;
    const int _kFastPollRate = 2;
    const int _kRegularPollRate = 4;

    // Poll quicker before context count is more or equal (so that we can start to use statistical positioning (eliminate extremes) earlier)
    static int GetPollRate(int contextCount)
    {

        if (contextCount < halfContextWindowSize)
            return _kSuperFastPollRate;
        else if (contextCount < contextWindowSize)
            return _kFastPollRate;
        else
            return _kRegularPollRate;
    }

    const int contextWindowSize = 15;
    // MIN CASE: 7*1 second + 8*2 seconds = 23 seconds
    // MAX CASE: 15*4 seconds             = 60 seconds
    const int halfContextWindowSize = contextWindowSize / 2;

    // We want to eliminate 2 with the minimum amount of contexts as well
    // since these contexts were generated with _kSuperFastPollRate and thus are quite inaccurate
    const int eliminateExtremesCount = 2; // 2 from start, 2 from end => 4 total
    static bool ShouldEliminateExtremes(int contextCount) => contextCount > halfContextWindowSize;

    const double seekToleranceSeconds = 3;

    // At which positions to refresh the next track at the end. Set as empty to disable.
    static readonly TimeSpan[] refreshNextTrackFromEnd = [TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)];

    static readonly TimeSpan NextSongTolerance = TimeSpan.FromMilliseconds(250); // Add a bit of tolerance to make sure we don't spam Spotify with requests

    private readonly SpotifyClient client;

    readonly Queue<PlayingContext> contexts = new(capacity: contextWindowSize);
    Task? playerFetch = null;

    readonly Task<FullTrack?>?[] nextTrackFetches = Enumerable.Repeat<Task<FullTrack?>?>(null, refreshNextTrackFromEnd.Length).ToArray();

    TimeSpan? previousProgress;

    bool canRefreshNextTrackOnTrackEnd = true;

    readonly ILogger<SpotifyWebPlayer>? logger;

    public SpotifyWebPlayer(SpotifyClient client, ILogger<SpotifyWebPlayer>? logger = null)
    {
        this.client = client;
        this.logger = logger;
    }

    private async Task _UnrestrictedFetchPlayer(int? debugPollRate = null)
    {
        logger?.LogInformation("Fetching new playing context... (poll rate: {})", debugPollRate?.ToString() ?? "null");
        CurrentlyPlayingContext? playContext = await client.Player.GetCurrentPlayback();
        HandleUpdate(playContext);
    }

    private async Task<FullTrack?> UnrestrictedFetchNextTrack()
    {
        logger?.LogInformation("Fetching next track context...");
        QueueResponse queue = await client.Player.GetQueue();
        return queue.Queue.OfType<FullTrack>().FirstOrDefault();
    }

    /// <summary>
    /// Returns null if there is a fetch already running.
    /// </summary>
    private Task FetchPlayer(int? debugPollRate = null)
    {
        if (playerFetch is null || playerFetch.IsCompleted)
            playerFetch = _UnrestrictedFetchPlayer(debugPollRate: debugPollRate);
        return playerFetch;
    }

    private void HandleUpdate(CurrentlyPlayingContext? playContext)
    {
        DateTimeOffset time = DateTimeOffset.UtcNow;

        FullTrack? track = playContext?.Item as FullTrack;
        bool trackChanged = false;

        lock (contexts)
        {
            if (contexts.TryPeek(out var c))
            {
                if (track is null)
                {
                    contexts.Clear();
                }
                else if (c.Track?.Id != track.Id)
                {
                    contexts.Clear();
                    trackChanged = true;
                }
            }

            if (!(playContext?.IsPlaying ?? false))
                contexts.Clear();

            PlayingContext? lastContext = contexts.LastOrDefault();
            if (playContext is not null && lastContext is not null)
            {
                TimeSpan diff = TimeSpan.FromMilliseconds(playContext.ProgressMs) - lastContext.ComputeCurrentProgress(time);
                // If the difference between the last context's extrapolated time and the current context differ too much, we most likely seeked.
                if (Math.Abs(diff.TotalSeconds) > seekToleranceSeconds)
                {
                    logger?.LogInformation("Seek detected. Clearing contexts...");
                    contexts.Clear();
                }
            }

            while (contexts.Count >= contextWindowSize)
                contexts.Dequeue();

            PlayingContext ctx = new(time, playContext, track);
            contexts.Enqueue(ctx);
        }

        if (trackChanged)
            Array.Fill(nextTrackFetches, null);
    }

    protected override async ValueTask Init()
    {
        await FetchPlayer();
    }

    protected override SpotifyPlayerContext? Update()
    {
        PlayingContext[] contexts;
        lock (this.contexts)
        {
            contexts = this.contexts.ToArray();
        }

        PlayingContext lastContext = contexts[^1];
        TimeSpan ahead = DateTimeOffset.UtcNow - lastContext.FetchTimestamp;

        int pollRate = GetPollRate(contexts.Length);
        if (ahead.TotalSeconds > pollRate)
            FetchPlayer(debugPollRate: pollRate);

        if (contexts.Length < 1)
            return null;

        if (lastContext.Context is null)
        {
            Debug.Assert(lastContext.Track is null);
            Debug.Assert(contexts.Length == 1);
            return null;
        }

        Debug.Assert(lastContext.Track is not null);


        TimeSpan progress;
        if (lastContext.Context.IsPlaying)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            TimeSpan[] progresses = contexts.Where(ctx => ctx.Context?.IsPlaying ?? false).Select(ctx => ctx.ComputeCurrentProgress(now)).ToArray();
            if (ShouldEliminateExtremes(contexts.Length))
                progresses = progresses.Order().Skip(eliminateExtremesCount).SkipLast(eliminateExtremesCount).ToArray();

            TimeSpan acc = TimeSpan.Zero;
            foreach (TimeSpan p in progresses)
                acc += p;
            progress = acc / progresses.Length;
        }
        else
        {
            progress = TimeSpan.FromMilliseconds(lastContext.Context.ProgressMs);
        }

        TimeSpan trackDuration = TimeSpan.FromMilliseconds(lastContext.Track.DurationMs);

        for (int i = 0; i < refreshNextTrackFromEnd.Length; i++)
        {
#if DEBUG
            if (i > 0)
                Debug.Assert(refreshNextTrackFromEnd[i - 1] > refreshNextTrackFromEnd[i]);
#endif
            TimeSpan left = trackDuration - progress;
            if (left <= refreshNextTrackFromEnd[i])
            {
                if (nextTrackFetches[i] is null)
                    nextTrackFetches[i] = UnrestrictedFetchNextTrack();
            }
            else
            {
                // if index is 0, we are at a point before track refreshing should happen
                // we should thus discard all next track fetches if there are any.

                // check for null so we don't unnecessarily fill the array on each frame
                if (i == 0 && nextTrackFetches[i] is not null)
                    Array.Fill(nextTrackFetches, null);

                break; // break so we don't unnecessarily check for the next refreshes as they should be ordered from largest to smallest anyway
            }
        }

        if ((progress - TimeSpan.FromMilliseconds(lastContext.Track.DurationMs)) > NextSongTolerance)
        {
            if (canRefreshNextTrackOnTrackEnd)
            {
                FetchPlayer();
                canRefreshNextTrackOnTrackEnd = false;
            }
        }
        else
        {
            canRefreshNextTrackOnTrackEnd = true;
        }

        // Do not allow progress to jump backwards due to clock sync changes
        // We use max tolerance of 1 second so that if the user adjust the music position manually, we don't bug out.
        if (previousProgress is TimeSpan pp && progress < pp && Math.Abs((progress - pp).TotalSeconds) < 1)
            progress = pp;
        previousProgress = progress;

        FullTrack? nextTrack = null;
        for (int i = (nextTrackFetches.Length - 1); i >= 0; i--)
        {
            Task<FullTrack?>? nextTrackFetch = nextTrackFetches[i];
            if (nextTrackFetch?.IsCompleted == true)
            {
                nextTrack = nextTrackFetch.Result;
                break;
            }
        }

        return new SpotifyPlayerContext(
            progress: progress,
            isPlaying: lastContext.Context.IsPlaying,
            track: SpotifyPlayerTrack.FromSpotifyTrack(lastContext.Track),
            nextTrack: SpotifyPlayerTrack.FromSpotifyTrackOrNull(nextTrack)
        );
    }
}
