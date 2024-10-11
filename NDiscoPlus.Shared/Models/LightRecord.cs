using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Models;

public class LightRecord
{
    public static readonly LightRecord Default = new(null);
    public static TimeSpan GetDefaultTimeOffset(NDPLight light) => GetDefaultTimeOffset(lightOrNull: light);
    private static TimeSpan GetDefaultTimeOffset(NDPLight? lightOrNull) => -(lightOrNull?.ExpectedLatency ?? TimeSpan.Zero);

    public static LightRecord CreateDefault(NDPLight light)
        => new(light);

    private readonly NDPLight? light;
    public NDPLight Light => light ?? throw new InvalidOperationException("Default configuration does not hold a reference to any lights.");

    public Channel Channel { get; init; } = Channel.All;
    public double Brightness { get; init; } = 1d;

    private readonly TimeSpan? timeOffset;
    public TimeSpan TimeOffset
    {
        get => timeOffset ?? throw new InvalidOperationException("Default configuration does not have a time offset, use GetDefaultTimeOffset instead.");
        init => timeOffset = value;
    }

    public LightRecord(NDPLight light) : this((NDPLight?)light)
    {
    }

    private LightRecord(NDPLight? light)
    {
        this.light = light;
        timeOffset = GetDefaultTimeOffset(light);
    }
}