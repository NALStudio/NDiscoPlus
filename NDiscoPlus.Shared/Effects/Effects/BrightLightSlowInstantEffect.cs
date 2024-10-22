using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects;

/// <summary>
/// Similar to <see cref="BrightLightEffect"/> but lights jump to max brightness
/// and more lights animate at once. Effect is also much slower (timed on bars, not beats)
/// </summary>
internal class BrightLightSlowInstantEffect : NDPEffect
{
    public BrightLightSlowInstantEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Default);
        if (channel is null)
            return;

        // 1/3 of the lights per bar
        // so when animation is 1,8 bars long
        // around 2/4 lights are on at any one time
        int lightsPerBar = channel.Lights.Count / 3;

        HashSet<LightId> previousLights = new();
        foreach (NDPInterval bar in ctx.Section.Timings.Bars.SkipLast(1)) // Skip last bar since our animations are 1,8 bars long
        {
            // Select all lights that weren't in use in the last frame
            List<LightId> availableLights = channel.Lights.Keys
                                                   .Where(l => !previousLights.Contains(l))
                                                   .ToList();

            HashSet<LightId> lights = new();
            while (lights.Count < lightsPerBar && availableLights.Count > 0)
            {
                // Take random available light and add it to lights (while removing it from the available lights)
                int index = ctx.Random.Next(availableLights.Count);
                bool added = lights.Add(availableLights[index]);
                availableLights.RemoveAt(index);

                Debug.Assert(added);
            }

            // Create effects
            // We don't use the last frame's lights so the duration can be over the duration of the bar.
            TimeSpan duration = 1.8d * bar.Duration;
            foreach (LightId light in lights)
            {
                channel.Add(
                    new Effect(
                        light,
                        bar.Start,
                        TimeSpan.Zero
                    )
                    {
                        Brightness = 1d,
                        FadeOut = duration
                    }
                );
            }

            // Assign previous lights for next frame
            previousLights = lights;
        }
    }
}
