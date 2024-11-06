using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Spotify.Players.WebPlayer;

public partial class NewSpotifyWebPlayer
{
    private class AdjustingClock
    {
        public int UpdateFrequency { get; }
        private readonly long adjustDivisor;

        private readonly ILogger<NewSpotifyWebPlayer>? logger;

        public AdjustingClock(ILogger<NewSpotifyWebPlayer>? logger, int updateFrequency)
        {
            UpdateFrequency = updateFrequency;
            adjustDivisor = updateFrequency * 5; // 75 fps => 375 => 0,2666... % adjust per update

            this.logger = logger;
        }

        public bool HasBeenReset { get; private set; } = false;

        private long resetTimestamp;
        private long progress;
        private long adjust;

        // At the start of a song, Spotify jumps backwards around 1,4 - 1,5 seconds
        // (if track is skipped instead of letting it run to the end, the jump is around -0,8 seconds instead)
        // This seems to always happen before 30 seconds of the song have elapsed.
        private bool spotifyIsFuckingWithUs = false;
        private static readonly long spotifyIsFuckingWithUsAmount = -1450L * TimeConversions.TicksPerMs;
        private static readonly long spotifyIsFuckingWithUsAtLeastBefore = 30L * TimeConversions.TicksPerSecond;
        private static readonly string spotifyIsFuckingWithUsOverMessage = $"Over {spotifyIsFuckingWithUsAtLeastBefore / (decimal)TimeConversions.TicksPerSecond:0.0} seconds have passed.";

        // Adjust input progress when we think that spotify is fucking with us
        private void AdjustProgress(ref long progress)
        {
            // if progress is over 30 seconds, Spotify will most likely NOT randomly jump progress anymore
            if (spotifyIsFuckingWithUs && progress > spotifyIsFuckingWithUsAtLeastBefore)
            {
                LogClockAdjustDeactivate(spotifyIsFuckingWithUsOverMessage);
                spotifyIsFuckingWithUs = false;
            }

            if (spotifyIsFuckingWithUs)
                progress += spotifyIsFuckingWithUsAmount;
        }

        public long GetProgress(long currentTimestamp)
        {
            ThrowIfNotReset();

            long elapsed = currentTimestamp - resetTimestamp;

            long progress = this.progress + elapsed + adjust;
            AdjustProgress(ref progress);
            return progress;
        }

        /// <summary>
        /// Returns how many ticks behind the clock was.
        /// </summary>
        public long AdjustClock(long currentTimestamp, long expectedProgress)
        {
            ThrowIfNotReset();

            // GetProgress gets adjusted with AdjustProgress
            long progress = GetProgress(currentTimestamp);
            // so we must also adjust the expected progress
            AdjustProgress(ref expectedProgress);

            // if clock is ahead, progress > expectedProgress => diff < 0
            // if clock is behind, expectedProgress > progress => diff > 0
            long diff = expectedProgress - progress;

            // adjust by 1 % of the total difference
            long adjust = diff / adjustDivisor;
            this.adjust += adjust;

            return diff;
        }

        public void Reset(long resetTimestamp, long progress, bool trackHasChanged)
        {
            long? progressBefore = HasBeenReset ? GetProgress(resetTimestamp) : null;

            this.resetTimestamp = resetTimestamp;
            this.progress = progress;

            bool keepClockProgress = HandleTrackChange(trackHasChanged);
            // Progress can only be kept if the clock has been reset before (and thus progressBefore is fetchable)
            if (!keepClockProgress || !progressBefore.HasValue)
            {
                // If we don't need to keep clock progress, we can reset adjust as well to get the most accurate time possible
                adjust = 0L;
            }
            else
            {
                // Keep clock progress the same by adjusting the adjust so that the progress stays the same
                long progressAfter = GetProgress(resetTimestamp);
                adjust += progressBefore.Value - progressAfter;

#if DEBUG
                // extract assertProgress into a separate variable so that the debugger can inspect its value
                long assertProgress = GetProgress(resetTimestamp);
                Debug.Assert(assertProgress == progressBefore);
#endif
                logger?.LogInformation("Clock progress was kept. Clock will gradually move towards the target progress instead.");
            }

            HasBeenReset = true;
        }

        private bool HandleTrackChange(bool trackHasChanged)
        {
            bool keepClockProgress = false;

            // Reset spotifyIsFuckingWithUs
            // or if it's already set, set it as false so that we know
            // that we are measuring time properly now
            if (trackHasChanged)
            {
                if (!spotifyIsFuckingWithUs)
                {
                    spotifyIsFuckingWithUs = true;
                    logger?.LogInformation("Activated clock adjust for track start.");
                }
            }
            else if (spotifyIsFuckingWithUs)
            {
                spotifyIsFuckingWithUs = false;
                LogClockAdjustDeactivate("Spotify provided accurate progress info.");

                // Try to keep clock progress and adjust to the new progress gradually
                keepClockProgress = true;
            }

            return keepClockProgress;
        }

        public void ThrowIfNotReset()
        {
            if (!HasBeenReset)
                throw new InvalidOperationException("Clock must be reset before operating on it.");
        }

        private void LogClockAdjustDeactivate(string reason)
        {
            logger?.LogInformation("Deactivated clock adjust. {}", reason);
        }
    }
}
