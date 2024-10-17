using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Spotify.Players.WebPlayer;

public partial class NewSpotifyWebPlayer
{
    private class AdjustingClock
    {
        private bool _reset = false;

        private long resetTimestamp;
        private long progress;
        private long adjust;

        public long GetProgress(long currentTimestamp)
        {
            ThrowIfNotReset();

            long elapsed = currentTimestamp - resetTimestamp;
            return progress + elapsed + adjust;
        }

        /// <summary>
        /// Returns how many ticks behind the clock was.
        /// </summary>
        public long AdjustClock(long currentTimestamp, long expectedProgress)
        {
            ThrowIfNotReset();

            long progress = GetProgress(currentTimestamp);

            // if clock is ahead, progress > expectedProgress => diff < 0
            // if clock is behind, expectedProgress > progress => diff > 0
            long diff = expectedProgress - progress;

            // adjust by 1 % of the total difference
            long adjust = diff / 100;
            this.adjust += adjust;

            return diff;
        }

        public void Reset(long resetTimestamp, long progress)
        {
            this.resetTimestamp = resetTimestamp;
            this.progress = progress;
            adjust = 0L;

            _reset = true;
        }

        public void ThrowIfNotReset()
        {
            if (!_reset)
                throw new InvalidOperationException("Clock must be reset before operating on it.");
        }
    }
}
