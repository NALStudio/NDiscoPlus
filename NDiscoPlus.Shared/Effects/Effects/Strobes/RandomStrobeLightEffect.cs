using NDiscoPlus.Shared.Effects.API.Channels.Effects;
using NDiscoPlus.Shared.Helpers;
using NDiscoPlus.Shared.Models;
using System.Collections.Immutable;
using System.Diagnostics;

namespace NDiscoPlus.Shared.Effects.Effects.Strobes;
internal class RandomStrobeLightEffect : BaseStrobeLightEffect
{
    public RandomStrobeLightEffect(EffectIntensity intensity) : base(intensity)
    {
    }

    protected override IEnumerable<LightGroup> Group(EffectContext ctx, NDPLightCollection lights, int frameCount, int groupCount)
    {
        int lightsPerFrame = Math.Max(lights.Count / groupCount, 1);

        HashSet<LightId>? lastFrame = null;
        for (int i = 0; i < frameCount; i++)
        {
            HashSet<LightId> currentFrame = new(capacity: lightsPerFrame);

            for (int j = 0; j < lightsPerFrame; j++)
            {
                LightId light;
                do
                {
                    light = lights.Random(ctx.Random).Id;
                } while (currentFrame.Contains(light) || (lastFrame?.Contains(light) == true));

                bool wasAdded = currentFrame.Add(light);
                Debug.Assert(wasAdded);
            }

            lastFrame = currentFrame;
            yield return LightGroup.FromIds(currentFrame);
        }
    }
}
