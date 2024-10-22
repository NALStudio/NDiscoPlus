using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Models;
using NDiscoPlus.Shared.Models.Color;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects;
internal class ColorStripeEffect : NDPEffect
{
    public ColorStripeEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Default);
        if (channel is null)
            return;

        ImmutableArray<NDPLight> lightPath = ComputePath(channel.Lights).ToImmutableArray();

        ImmutableArray<NDPInterval> syncIntervals = ctx.Section.Timings.Tatums;
        if (syncIntervals.Any(static s => s.Duration <= EffectConstants.MinEffectDuration))
            syncIntervals = ctx.Section.Timings.Beats;

        NDPColorPalette palette = ctx.Palette;

        for (int syncIndex = 0; syncIndex < syncIntervals.Length; syncIndex++)
        {
            NDPInterval sync = syncIntervals[syncIndex];

            for (int lightIndex = 0; lightIndex < lightPath.Length; lightIndex++)
            {
                NDPLight light = lightPath[lightIndex];

                // Minus sync index so that instead of moving colors forward
                // (and thus moving the path backwards), we do the opposite (path moves forwards)
                int colorIndex = (lightIndex - syncIndex) % palette.Count;
                if (colorIndex < 0) // C# modulo keeps sign, ie. -5 % 3 => -2
                    colorIndex += palette.Count;

                NDPColor color = palette[colorIndex]; // Iterate colors backwards so that snake moves forwards

                channel.Add(
                    new Effect(
                        light.Id,
                        sync.Start,
                        sync.Duration
                    )
                    {
                        X = color.X,
                        Y = color.Y,
                        Brightness = api.Config.EffectBaseBrightness
                    }
                );
            }
        }
    }

    private static List<NDPLight> ComputePath(NDPLightCollection lightCollection)
    {
        static double EuclideanDistance(LightPosition a, LightPosition b)
        {
            double x = a.X - b.X;
            double y = a.Y - b.Y;
            double z = a.Z - b.Z;
            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }

        Dictionary<LightId, NDPLight> remainingLights = new(lightCollection);

        // Start path from the leftmost light found (I wanted it to be topleft, but no idea how to use MinBy to find that...)
        NDPLight initialLight = remainingLights.MinBy(static l => l.Value.Position.X).Value;
        remainingLights.Remove(initialLight.Id);

        List<NDPLight> path = new() { initialLight };
        while (remainingLights.Count > 0)
        {
            // Keep adding lights to path until all lights have been used

            NDPLight prevLight = path[^1];

            // Find the next light by looking for the shortest path possible
            NDPLight nextLight = remainingLights.MinBy(l => EuclideanDistance(prevLight.Position, l.Value.Position)).Value;
            remainingLights.Remove(nextLight.Id);

            path.Add(nextLight);
        }

        return path;
    }
}
