using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Background;
using NDiscoPlus.Shared.Effects.API.Channels.Background.Intrinsics;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;

namespace NDiscoPlus.Shared.Effects.BaseEffects;

internal sealed class ColorCycleBackgroundEffect : NDPBackgroundEffect
{
    private readonly struct Animation
    {
        public LightId LightId { get; }
        public TimeSpan End { get; }

        public Animation(LightId light, TimeSpan end)
        {
            LightId = light;
            End = end;
        }
    }

    public const double AnimationSeconds = 10d;

    public override void Generate(Context ctx, EffectAPI api)
    {
        BackgroundChannel? channel = api.Background;
        if (channel is null)
            return;

        TimeSpan animationDuration = TimeSpan.FromSeconds(AnimationSeconds);

        List<Animation> animations = channel.Lights.Values.Select(l => new Animation(l.Id, GetRandomAnimationCooldown(ctx.Random))).ToList();

        bool running = true;
        while (running)
        {
            TimeSpan time = animations.Min(a => a.End);
            animations.RemoveAll(a => a.End <= time);

            if (animations.Count >= channel.Lights.Count)
                continue;

            foreach (NDPLight l in channel.Lights.Values.Where(l => animations.All(a => l.Id != a.LightId)))
            {
                TimeSpan animationEnd = time + animationDuration + GetRandomAnimationCooldown(ctx.Random);
                if (animationEnd > ctx.Analysis.Track.Duration)
                {
                    running = false;
                    continue; // continue creating animations for the rest of the lights.
                }

                NDPColor color = PickNewRandomColor(ctx);
                color = color.CopyWith(brightness: api.Config.BaseBrightness);

                animations.Add(new Animation(l.Id, animationEnd));
                channel.Add(new BackgroundTransition(l.Id, time, animationDuration, color));
            }
        }
    }

    private static TimeSpan GetRandomAnimationCooldown(Random random)
        => TimeSpan.FromSeconds(random.NextDouble().Remap(0d, 1d, 2d, 10d));

    static NDPColor PickNewRandomColor(Context ctx)
    {
        // Custom function so that we can switch to a more fancy randomizer in the future if needed.
        return ctx.Palette[ctx.Random.Next(ctx.Palette.Count)];
    }
}
