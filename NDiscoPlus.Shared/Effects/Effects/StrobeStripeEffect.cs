using NDiscoPlus.Shared.Effects.API;
using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Effects.API.Channels.Effects.Intrinsics;
using NDiscoPlus.Shared.Effects.Effects.Strobes;
using NDiscoPlus.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.Shared.Effects.Effects;
internal sealed class StrobeStripeEffect : NDPEffect
{
    public StrobeStripeEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    public override void Generate(EffectContext ctx, EffectAPI api)
    {
        EffectChannel? channel = api.GetChannel(Channel.Strobe);
        if (channel is null)
            return;

        ImmutableArray<NDPLight> path = ColorStripeEffect.ComputePath(channel.Lights).ToImmutableArray();
        (int groupCount, ImmutableArray<NDPInterval> timings) = BaseStrobeLightEffect.GenerateSyncIntervals(ctx);

        BaseStrobeLightEffect.ClearChannelsForStrobes(api, timings);

        for (int timingIndex = 0; timingIndex < timings.Length; timingIndex++)
        {
            NDPInterval interval = timings[timingIndex];
            for (int lightIndex = 0; lightIndex < path.Length; lightIndex++)
            {
                NDPLight light = path[lightIndex];

                int strobeIndex = lightIndex - timingIndex;
                // Negative modulo is handled correctly as 3 % 3 => 0 and -3 % 3 => 0
                if ((strobeIndex % groupCount) == 0)
                    channel.Add(Effect.CreateStrobe(api.Config, light.Id, interval));
            }
        }
    }
}
