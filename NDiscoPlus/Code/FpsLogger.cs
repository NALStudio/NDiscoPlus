using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Code;
internal sealed class FpsLogger
{
    private static readonly long _kTicksPerSecond = Stopwatch.Frequency;
    private static readonly decimal _kTicksPerSecondDecimal = _kTicksPerSecond;

    private readonly LogLevel level;
    private readonly ILogger logger;
    private readonly long measurePeriodTicks;
    private readonly string prefix;

    private long? timestamp = null;

    private int frameCount = 0;
    private long totalTicks = 0L;
    private long maxFrameTicks = long.MinValue;

    public FpsLogger(ILogger logger, LogLevel logLevel = LogLevel.Debug, int measurePeriodSeconds = 1, string prefix = "")
    {
        level = logLevel;
        this.logger = logger;

        measurePeriodTicks = measurePeriodSeconds * _kTicksPerSecond;
        this.prefix = prefix;
    }

    public void Tick()
    {
        if (!timestamp.HasValue)
        {
            timestamp = Stopwatch.GetTimestamp();
            return;
        }

        long newTimestamp = Stopwatch.GetTimestamp();
        long ticks = newTimestamp - timestamp.Value;
        timestamp = newTimestamp;

        frameCount++;
        totalTicks += ticks;
        if (ticks > maxFrameTicks)
            maxFrameTicks = ticks;

        if (totalTicks > measurePeriodTicks)
        {
            Log();
            frameCount = 0;
            totalTicks = 0L;
            maxFrameTicks = long.MinValue;
        }
    }

    private void Log()
    {
        decimal avgFps = ComputeFpsFromFrameCount(frameCount: frameCount, totalTicks: totalTicks);
        decimal minFps = ComputeFps(maxFrameTicks);

        logger.Log(level, "{}FPS: {:.00} (min: {:.00})", prefix + ' ', avgFps, minFps);
    }

    private static decimal ComputeFps(long ticks)
    {
        // 1 / (ticks / ticksPerSecond) => ticksPerSecond / ticks
        return _kTicksPerSecondDecimal / ticks;
    }

    private static decimal ComputeFpsFromFrameCount(int frameCount, long totalTicks)
    {
        // frameCount / (ticks / ticksPerSecond) => (frameCount * ticksPerSecond) / ticks
        long mult = frameCount * _kTicksPerSecond;
        return mult / (decimal)totalTicks;
    }
}
