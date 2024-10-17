using Microsoft.Extensions.Logging;
using NDiscoPlus.Spotify.Models;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Spotify.Players.WebPlayer;

public partial class NewSpotifyWebPlayer
{
    private static class Constants
    {
        private static double Lerp(double a, double b, double t) => a + (b - a) * t;
        private static double Remap(double value, double from1, double to1, double from2, double to2) => ((value - from1) / (to1 - from1) * (to2 - from2)) + from2;

        public const int ContextWindowSize = 15;

        public static readonly TimeSpan StartFetchingNextTrackWhenFromEnd = TimeSpan.FromSeconds(20); // Start fetching when less than 20 seconds from end
        public static readonly long NextTrackTicksBetweenRequests = 5L * TimeConversions.TicksPerSecond; // Fetch every 5 seconds
        public static readonly TimeSpan StopFetchingNextTrackWhenFromEnd = TimeSpan.FromSeconds(2); // Stop fetching when less than two seconds from end

        // Tolerance: 1,75 s
        // since Spotify seems to jump around 1,5 seconds when fucking with us
        public static readonly long SpotifyIsFuckingWithUsTolerance = 1750 * TimeConversions.TicksPerMs;

        public static int EliminateExtremesCount(int contextCount)
        {
            // 3 / 4 => 0 per end => 0 removed => 3 remaining
            // 4 / 4 => 1 per end => 2 removed => 2 remaining
            // 8 / 4 => 2 per end => 4 removed => 4 remaining
            // 12 / 4 => 3 per end => 6 removed => 6 remaining
            // 15 / 4 => 3 per end => 6 removed => 9 remaining
            return contextCount / 4;
        }

        public static long StatePollRate(double contextCount)
        {
            const int min = 1;
            const int max = 4;

            double valueSeconds = Remap(contextCount, from1: 0d, to1: ContextWindowSize, from2: min, to2: max);
            valueSeconds = Math.Clamp(valueSeconds, min, max);

            long valueMs = checked((long)(valueSeconds * 1000d));
            return valueMs * TimeConversions.TicksPerMs;
        }
    }

    private static class TimeConversions
    {
        public static readonly long TicksPerSecond = Stopwatch.Frequency;
        public static readonly long TicksPerMs = ComputeTicksPerMs();

        private static long ComputeTicksPerMs()
        {
            long ticksPerMs = TicksPerSecond / 1000L;
            Debug.Assert((ticksPerMs * 1000L) == TicksPerSecond, "We should not lose accuracy when dividing if both values are a power of 10.");
            return ticksPerMs;
        }

        public static TimeSpan AsTimeSpan(long ticks) => Stopwatch.GetElapsedTime(0L, ticks);
        public static decimal AsTotalSeconds(long ticks) => ticks / (decimal)TicksPerSecond;
    }
}
