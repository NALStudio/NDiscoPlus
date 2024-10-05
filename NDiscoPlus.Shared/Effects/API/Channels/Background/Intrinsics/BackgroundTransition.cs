using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;

namespace NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;

public readonly struct BackgroundTransition
{
    public LightId LightId { get; }

    public TimeSpan Start { get; }
    public TimeSpan Duration { get; }

    public TimeSpan End => Start + Duration;

    public NDPColor Color { get; }

    public BackgroundTransition(LightId lightId, TimeSpan start, TimeSpan duration, NDPColor color)
    {
        LightId = lightId;
        Start = start;
        Duration = duration;
        Color = color;
    }

    public NDPColor Interpolate(TimeSpan progress, NDPColor from)
    {
        double t = (progress - Start) / Duration;
        return NDPColor.Lerp(from, Color, t);
    }
}