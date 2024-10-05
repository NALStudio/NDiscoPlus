using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System.Diagnostics;

namespace NDiscoPlus.Shared.Effects.Effects;

internal sealed class BrightLightEffect : NDPEffect
{
    private readonly bool white;
    private readonly bool slow;

    private BrightLightEffect(bool white, bool slow, EffectIntensity intensity) : base(intensity)
    {
        this.white = white;
        this.slow = slow;
    }

    public static BrightLightEffect Default(EffectIntensity intensity)
        => new(white: false, slow: false, intensity: intensity);

    public static BrightLightEffect Slow(EffectIntensity intensity)
        => new(white: false, slow: true, intensity: intensity);

    public static BrightLightEffect White(EffectIntensity intensity)
        => new(white: true, slow: false, intensity: intensity);

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Default);
        if (channel is null)
            return;

        int animationCount = ctx.Section.Tempo.TimeSignature;
        int maxSimultaneousAnimations = Math.Max((int)(animationCount * (2d / 3d)), 1);
        // Math.Max since if ctx.TimeSignature == 1, maxSimultaneousAnimations = 0

        double syncDuration = slow ? ctx.Section.Tempo.SecondsPerBar : ctx.Section.Tempo.SecondsPerBeat;

        IList<NDPInterval> syncIntervals = slow ? ctx.Section.Timings.Bars : ctx.Section.Timings.Beats;

        double animationDuration = syncDuration * maxSimultaneousAnimations;
        int lightsPerAnimation = Math.Max(channel.Lights.Count / animationCount, 1);
        // will flash all lights simultaneously if TimeSignature == 1

        TimeSpan fadeInDuration = TimeSpan.FromSeconds(0.2d * animationDuration);
        TimeSpan fadeOutDuration = TimeSpan.FromSeconds(0.8d * animationDuration);

        NDPColor? color = white ? api.Config.StrobeColor : null;

        foreach (NDPInterval interval in syncIntervals)
        {
            // offset backwards so that the rise is in the middle of the beat, not at the beginning
            TimeSpan pos = interval.Start - (fadeInDuration / 2d);

            for (int i = 0; i < lightsPerAnimation; i++)
            {
                NDPLight[] lights = channel.GetAvailableLights(pos).ToArray();
                LightId light;
                if (lights.Length > 0)
                    light = ctx.Random.Choice(lights).Id;
                else
                    light = channel.GetBusyEffects(pos).MinBy(e => e.End).LightId;

                Effect eff = new(
                    light,
                    interval.Start,
                    TimeSpan.Zero
                )
                {
                    X = color?.X,
                    Y = color?.Y,
                    Brightness = api.Config.MaxBrightness,
                    FadeIn = fadeInDuration,
                    FadeOut = fadeOutDuration
                };

                channel.Add(eff);
            }
        }
    }
}
