using Microsoft.Extensions.Logging;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System.Diagnostics;

namespace NDiscoPlus.Spotify.Players.WebPlayer;

public partial class NewSpotifyWebPlayer : SpotifyPlayer
{
    private readonly SpotifyClient spotify;
    private readonly ILogger<NewSpotifyWebPlayer>? logger;

    public NewSpotifyWebPlayer(SpotifyClient spotify, ILogger<NewSpotifyWebPlayer>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(spotify);

        this.spotify = spotify;
        this.logger = logger;
    }

    private Task<Request<PlaybackState?>>? currentFetch = null;
    private Request<PlaybackState?>? previousFetch = null;

    private Task<Request<SpotifyPlayerTrack?>>? nextTrackFetch = null;
    private Request<SpotifyPlayerTrack?>? nextTrack = null;

    /// <summary>
    /// The modified timestamp provided by Spotify.
    /// </summary>
    private long? statesLastModifiedTimestampMs = null;
    private Queue<Request<PlaybackState?>> states = new(capacity: Constants.ContextWindowSize);

    private long? clockLastModifiedTimestampMs = null;
    private AdjustingClock? clock;
    private long clockTicksBehind = 0L;

    protected override ValueTask Init(int updateFrequency)
    {
        clock = new AdjustingClock(logger, updateFrequency);
        return ValueTask.CompletedTask;
    }

    private static long ExtrapolateStateProgress(Request<PlaybackState?> state, long currentTimestamp)
    {
        Debug.Assert(state.Value is not null);

        long elapsed = currentTimestamp - state.ReceivedTimestamp;
        return state.Value.ProgressTicks + elapsed;
    }

    private static async Task<Request<PlaybackState?>> FetchState(SpotifyClient spotify)
    {
        long sendTimestamp = Stopwatch.GetTimestamp();
        CurrentlyPlayingContext? ctx = await spotify.Player.GetCurrentPlayback();
        long receiveTimestamp = Stopwatch.GetTimestamp();

        if (ctx is null || ctx.Item is not FullTrack track)
            return new Request<PlaybackState?>(sendTimestamp, receiveTimestamp, null);


        return new Request<PlaybackState?>(
            sendTimestamp,
            receiveTimestamp,
            new PlaybackState()
            {
                Track = SpotifyPlayerTrack.FromSpotifyTrack(track),
                IsPlaying = ctx.IsPlaying,
                LastEditedTimestampMs = ctx.Timestamp,
                ProgressMs = ctx.ProgressMs
            }
        );
    }

    private void UpdateState(long timestamp, out bool trackHasChanged)
    {
        if (ShouldUpdateState(timestamp, out long? pollRate))
        {
            if (logger is not null)
            {
                decimal msBehind = clockTicksBehind / (decimal)TimeConversions.TicksPerMs;
                string aheadBehind = msBehind >= 0m ? "behind" : "ahead";

                logger.LogInformation(
                    "Fetching new state... (rate: {:0.0} s, clock: {:0.000} ms {})",
                    pollRate.HasValue ? TimeConversions.AsTotalSeconds(pollRate.Value) : null,
                    Math.Abs(msBehind),
                    aheadBehind
                );
            }
            currentFetch = Task.Run(async () => await FetchState(spotify));
        }

        // Handle finished fetch
        trackHasChanged = false;
        if (currentFetch?.IsCompleted == true)
        {
            Request<PlaybackState?>? state;
            if (currentFetch.IsCompletedSuccessfully)
            {
                state = currentFetch.Result;
            }
            else
            {
                logger?.LogError("Fetch failed with error:\n{}", currentFetch.Exception);
                state = null;
            }

            currentFetch = null;
            if (state is not null)
            {
                UpdateQueueWithNewState(state, previousFetch, ref trackHasChanged);
                previousFetch = state;
            }
        }
    }

    private void UpdateQueueWithNewState(Request<PlaybackState?> state, Request<PlaybackState?>? previousState, ref bool trackHasChanged)
    {
        long? stateLastModTimestamp = state.Value?.LastEditedTimestampMs;

        if (stateLastModTimestamp == statesLastModifiedTimestampMs)
        {
            // If timestamps match, we know the playing state is the same so we can keep previous states to extrapolate the current progress
            // Of course we must dequeue some values so that we don't go over the context window size
            while (states.Count >= Constants.ContextWindowSize)
                states.Dequeue();
        }
        else
        {
            // If timestamps don't match, the playing state has changed and thus it's not comparable to any previous states
            states.Clear();
            statesLastModifiedTimestampMs = stateLastModTimestamp;

            if (state.Value?.Track.Id != previousState?.Value?.Track.Id)
                trackHasChanged = true;
        }

        states.Enqueue(state);
    }

    private bool TrackHasEndedSinceFetch(long timestamp, Request<PlaybackState?> fetch)
    {
        if (fetch.Value is null)
            return false;

        // Use received timestamp so that we determine track to have ended late rather than early
        // and thus we don't accidentally spam the server with unnecessary requests
        long elapsedSince = timestamp - fetch.ReceivedTimestamp;
        long progressThen = fetch.Value.ProgressTicks;

        long progressNow = progressThen + elapsedSince;
        return TimeConversions.AsTimeSpan(progressNow) > fetch.Value.Track.Length;
    }

    private bool ShouldUpdateState(long timestamp, out long? statePollRate)
    {
        statePollRate = null;

        if (currentFetch is not null)
            return false;

        if (previousFetch is not null)
        {
            long pollRate = Constants.StatePollRate(states.Count);
            long elapsedTicks = timestamp - previousFetch.SentTimestamp;

            // When track has ended, start polling for next song immediately
            // so that the played doesn't lag behind
            if (TrackHasEndedSinceFetch(timestamp, previousFetch))
                pollRate = 0L;

            if (elapsedTicks < pollRate)
                return false;
            statePollRate = pollRate;
        }

        return true;
    }

    private long ComputeProgressPlaying(long timestamp)
    {
        Debug.Assert(states.All(static s => s.Value?.IsPlaying == true));

        long progressSum = 0L;
        checked // checked so that if the integer overflows, we catch it in time
        {
            foreach (Request<PlaybackState?> s in states)
                progressSum += ExtrapolateStateProgress(s, timestamp);
        }

        // We don't care about last digit accuracy as it's so small it doesn't make a difference
        return progressSum / states.Count;
    }

    private long ComputeProgressNotPlaying()
    {
        Debug.Assert(states.All(static s => s.Value?.IsPlaying != true));

        int? progressMs = null;
        if (states.TryPeek(out Request<PlaybackState?>? oldestState))
            progressMs = oldestState.Value?.ProgressMs;

        Debug.Assert(states.All(s => s.Value?.ProgressMs == progressMs));

        return (progressMs ?? 0) * TimeConversions.TicksPerMs;
    }

    private long ComputeProgress(long timestamp, bool trackHasChanged)
    {
        Request<PlaybackState?>? latestState = previousFetch;

        bool isPlaying = latestState?.Value?.IsPlaying == true;

        long progress;
        if (isPlaying)
            progress = ComputeProgressPlaying(timestamp);
        else
            progress = ComputeProgressNotPlaying();


        if (isPlaying)
        {
            // BUG: When user adds or removes a song from the queue, the modified timestamp gets changed
            // and thus firing a clock reset. This is fine for now, but I should probably look into returning
            // to the old progress based way of determining if a seek happened.
            if (statesLastModifiedTimestampMs != clockLastModifiedTimestampMs)
            {
                decimal? progressBefore = null;
                if (clock!.HasBeenReset)
                    progressBefore = clock!.GetProgress(timestamp) / (decimal)TimeConversions.TicksPerSecond;

                clock!.Reset(timestamp, progress, trackHasChanged: trackHasChanged);
                clockLastModifiedTimestampMs = statesLastModifiedTimestampMs;

                decimal progressAfter = clock.GetProgress(timestamp) / (decimal)TimeConversions.TicksPerSecond;
                logger?.LogInformation("Clock reset. (before: {:0.000} s, after: {:0.000} s)", progressBefore, progressAfter);
            }
            else
            {
                clockTicksBehind = clock!.AdjustClock(timestamp, progress);
                // logger?.LogInformation("{:0.000} ms", clockTicksBehind / (decimal)TimeConversions.TicksPerMs);
            }

            progress = clock.GetProgress(timestamp);
        }
        else
        {
            clockTicksBehind = 0L;
        }

        return progress;
    }

    private static async Task<Request<SpotifyPlayerTrack?>> FetchNextTrack(SpotifyClient spotify)
    {
        long sendTimestamp = Stopwatch.GetTimestamp();
        QueueResponse response = await spotify.Player.GetQueue();
        long receiveTimestamp = Stopwatch.GetTimestamp();

        List<IPlayableItem> queue = response.Queue;

        FullTrack? track = queue.FirstOrDefault() as FullTrack;
        return new(sendTimestamp, receiveTimestamp, SpotifyPlayerTrack.FromSpotifyTrackOrNull(track));
    }


    private void UpdateNextTrack(long timestamp, long progress)
    {
        Availability available = NextTrackShouldBeAvailable(progress);
        if (ShouldUpdateNextTrack(timestamp, available))
        {
            logger?.LogInformation("Fetching next track...");
            nextTrackFetch = Task.Run(async () => await FetchNextTrack(spotify));
        }

        // Handle finished fetch
        if (nextTrackFetch?.IsCompleted == true)
        {
            Request<SpotifyPlayerTrack?>? next;
            if (nextTrackFetch.IsCompletedSuccessfully)
            {
                next = nextTrackFetch.Result;
            }
            else
            {
                logger?.LogError("Fetch failed with error:\n{}", nextTrackFetch.Exception);
                next = null;
            }

            nextTrackFetch = null;
            if (next is not null)
                nextTrack = next;
        }

        // set every frame in case nextTrackFetch finishes after nextTrack should not be available
        if (available == Availability.NotAvailable)
            nextTrack = null;
    }

    private Availability NextTrackShouldBeAvailable(long progressTicks)
    {
        PlaybackState? latestState = previousFetch?.Value;
        if (latestState is null)
            return Availability.NotAvailable;

        TimeSpan progress = TimeConversions.AsTimeSpan(progressTicks);
        TimeSpan trackLength = latestState.Track.Length;

        TimeSpan fromEnd = trackLength - progress;
        if (fromEnd > Constants.StartFetchingNextTrackWhenFromEnd)
            return Availability.NotAvailable;

        if (fromEnd < Constants.StopFetchingNextTrackWhenFromEnd)
            return Availability.DoesNotUpdate;

        return Availability.Available;
    }

    private bool ShouldUpdateNextTrack(long timestamp, Availability shouldBeAvailable)
    {
        if (shouldBeAvailable == Availability.NotAvailable)
            return false;
        if (shouldBeAvailable == Availability.DoesNotUpdate)
            return false;
        Debug.Assert(shouldBeAvailable == Availability.Available);

        if (nextTrackFetch is not null)
            return false;

        if (nextTrack is not null)
        {
            long elapsedTicks = timestamp - nextTrack.SentTimestamp;
            if (elapsedTicks < Constants.NextTrackTicksBetweenRequests)
                return false;
        }

        return true;
    }

    private SpotifyPlayerContext? ConstructResponse(long progress)
    {
        PlaybackState? latestState = previousFetch?.Value;

        if (latestState is null)
            return null;

        TimeSpan progressTimeSpan = TimeConversions.AsTimeSpan(progress);

        return new SpotifyPlayerContext()
        {
            Progress = progressTimeSpan,
            IsPlaying = latestState.IsPlaying,
            Track = latestState.Track,
            NextTrack = nextTrack?.Value
        };
    }

    protected override SpotifyPlayerContext? Update()
    {
        long timestamp = Stopwatch.GetTimestamp();

        UpdateState(timestamp, out bool trackHasChanged);
        long progress = ComputeProgress(timestamp, trackHasChanged);
        UpdateNextTrack(timestamp, progress);
        return ConstructResponse(progress);
    }
}
